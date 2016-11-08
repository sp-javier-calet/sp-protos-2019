using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;

namespace SocialPoint.WAMP.Subscriber
{
    public class Subscription
    {
        public long Id{ get; private set; }

        public string Topic{ get; private set; }

        public Subscription(long id, string topic)
        {
            Id = id;
            Topic = topic;
        }
    }

    public delegate void HandlerSubscription(AttrList args, AttrDic kwargs);

    public delegate void OnSubscribed(Error error, Subscription subscription);

    public class SubscribeRequest : WAMPConnection.Request<OnSubscribed>
    {
        internal HandlerSubscription Handler{ get; private set; }

        internal string Topic{ get; private set; }

        internal SubscribeRequest(HandlerSubscription handler, OnSubscribed completionHandler, string topic) : base(completionHandler)
        {
            Handler = handler;
            Topic = topic;
        }
    }

    public delegate void OnUnsubscribed(Error error);

    public class UnsubscribeRequest : WAMPConnection.Request<OnUnsubscribed>
    {
        internal UnsubscribeRequest(OnUnsubscribed completionHandler) : base(completionHandler)
        {

        }
    }

    public class WAMPRoleSubscriber : WAMPRole
    {
        #region Data structures

        Dictionary<long, HandlerSubscription> _subscriptionHandlers;

        Dictionary<long, SubscribeRequest> _subscribeRequests;
        Dictionary<long, UnsubscribeRequest> _unsubscribeRequests;

        #endregion

        #region Public

        public WAMPRoleSubscriber(WAMPConnection connection) : base(connection)
        {
            _subscriptionHandlers = new Dictionary<long, HandlerSubscription>();

            _subscribeRequests = new Dictionary<long, SubscribeRequest>();
            _unsubscribeRequests = new Dictionary<long, UnsubscribeRequest>();
        }

        public SubscribeRequest Subscribe(string topic, HandlerSubscription handler, OnSubscribed completionHandler)
        {
            if(!_connection.HasActiveSession())
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession, "No current session"), null);
                }
                return null;
            }

            _connection.DebugMessage(string.Concat("Subscribe to event ", topic));

            /* [SUBSCRIBE, Request|id, Options|dict, Topic|uri]
             * [32, 713845233, {}, "com.myapp.mytopic1"]
             */
            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_subscribeRequests.ContainsKey(requestId), "This requestId was already in use");
            var request = new SubscribeRequest(handler, completionHandler, topic);
            _subscribeRequests.Add(requestId, request);

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.SUBSCRIBE));
            data.AddValue(requestId);
            data.Add(new AttrDic());
            data.AddValue(topic);

            _connection.SendData(data);

            return request;
        }

        public UnsubscribeRequest Unsubscribe(Subscription subscription, OnUnsubscribed completionHandler)
        {
            if(!_connection.HasActiveSession())
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession, "No current session"));
                }
                return null;
            }

            if(!_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.UnsubscribeError, string.Concat("Invalid subscription id: ", subscription.Id)));
                }
                return null;
            }

            _connection.DebugMessage(string.Concat("Unsubscribe to subscription ", subscription.Id));

            _subscriptionHandlers.Remove(subscription.Id);

            /* [UNSUBSCRIBE, Request|id, SUBSCRIBED.Subscription|id]
             * [34, 85346237, 5512315355]
             */
            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_unsubscribeRequests.ContainsKey(requestId), "This requestId was already in use");

            var request = new UnsubscribeRequest(completionHandler);
            _unsubscribeRequests.Add(requestId, request);

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.UNSUBSCRIBE));
            data.AddValue(requestId);
            data.AddValue(subscription.Id);

            _connection.SendData(data);

            return request;
        }

        public void AutoSubscribe(Subscription subscription, HandlerSubscription handler)
        {
            if(_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                throw new Exception("This subscriptionId was already in use");
            }
            _subscriptionHandlers.Add(subscription.Id, handler);
        }

        #endregion

        #region Private

        internal void ProcessSubscribed(AttrList msg)
        {
            // [SUBSCRIBED, SUBSCRIBE.Request|id, Subscription|id]

            if(msg.Count != 3)
            {
                throw new Exception("Invalid SUBSCRIBED message structure - length must be 3");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid SUBSCRIBED message structure - SUBSCRIBE.Request must be an integer");
            }

            long requestId = msg.Get(1).AsValue.ToLong();
            SubscribeRequest request;
            if(!_subscribeRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus SUBSCRIBED message for non-pending request ID");
            }

            if(!msg.Get(2).IsValue)
            {
                throw new Exception("Invalid SUBSCRIBED message structure - SUBSCRIBED.Subscription must be an integer");
            }
            long subscriptionId = msg.Get(2).AsValue.ToLong();

            var subscription = new Subscription(subscriptionId, request.Topic);
            AutoSubscribe(subscription, request.Handler);

            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(null, subscription);
            }
            _subscribeRequests.Remove(requestId);
        }

        internal void ProcessUnsubscribed(AttrList msg)
        {
            // [UNSUBSCRIBED, UNSUBSCRIBE.Request|id]

            if(msg.Count != 2)
            {
                throw new Exception("Invalid UNSUBSCRIBED message structure - length must be 2");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid UNSUBSCRIBED message structure - UNSUBSCRIBE.Request must be an integer");
            }

            long requestId = msg.Get(1).AsValue.ToLong();
            UnsubscribeRequest request;
            if(!_unsubscribeRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus UNSUBSCRIBED message for non-pending request ID");
            }

            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(null);
            }
            _unsubscribeRequests.Remove(requestId);
        }

        internal void ProcessEvent(AttrList msg)
        {
            // [EVENT, SUBSCRIBED.Subscription|id, PUBLISHED.Publication|id, Details|dict]
            // [EVENT, SUBSCRIBED.Subscription|id, PUBLISHED.Publication|id, Details|dict, PUBLISH.Arguments|list]
            // [EVENT, SUBSCRIBED.Subscription|id, PUBLISHED.Publication|id, Details|dict, PUBLISH.Arguments|list, PUBLISH.ArgumentsKw|dict]

            if(msg.Count < 4 || msg.Count > 6)
            {
                throw new Exception("Invalid EVENT message structure - length must be 4, 5 or 6");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid EVENT message structure - SUBSCRIBED.Subscription must be an integer");
            }
            long subscriptionId = msg.Get(1).AsValue.ToLong();

            HandlerSubscription handler; 
            if(!_subscriptionHandlers.TryGetValue(subscriptionId, out handler))
            {
                // silently swallow EVENT for non-existent subscription IDs.
                // We may have just unsubscribed, when this EVENT might be have
                // already been in-flight.
                _connection.DebugMessage(string.Concat("Skipping EVENT for non-existent subscription ID ", subscriptionId));
                return;
            }

            if(handler != null)
            {
                AttrList listParams = null;
                AttrDic dictParams = null;
                if(msg.Count >= 5)
                {
                    if(!msg.Get(4).IsList)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.Arguments must be a list");
                    }
                    listParams = msg.Get(4).AsList;
                }
                if(msg.Count >= 6)
                {
                    if(!msg.Get(5).IsDic)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.ArgumentsKw must be a dictionary");
                    }
                    dictParams = msg.Get(5).AsDic;
                }
                handler(listParams, dictParams);
            }
        }

        internal void ProcessSubscribeError(long requestId, string description)
        {
            SubscribeRequest request;
            if(!_subscribeRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus ERROR message for non-pending SUBSCRIBE request ID");
            }
            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(new Error(ErrorCodes.SubscribeError, description), new Subscription(requestId, request.Topic));
            }
            _subscribeRequests.Remove(requestId);
        }

        internal void ProcessUnsubscribeError(long requestId, string description)
        {
            UnsubscribeRequest request;
            if(!_unsubscribeRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus ERROR message for non-pending UNSUBSCRIBE request ID");
            }
            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(new Error(ErrorCodes.UnsubscribeError, description));
            }
            _unsubscribeRequests.Remove(requestId);
        }

        internal override void AddRoleDetails(AttrDic detailsDic)
        {
            detailsDic.Set("subscriber", new AttrDic());
        }

        internal override void ResetToInitialState()
        {
            for(var i = 0; i < _subscribeRequests.Count; i++)
            {
                var request = _subscribeRequests[i];
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), null);
                }
            }

            _subscribeRequests.Clear();

            for(var i = 0; i < _unsubscribeRequests.Count; i++)
            {
                var request = _unsubscribeRequests[i];
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"));
                }
            }

            _unsubscribeRequests.Clear();
        }

        #endregion
    }
}
