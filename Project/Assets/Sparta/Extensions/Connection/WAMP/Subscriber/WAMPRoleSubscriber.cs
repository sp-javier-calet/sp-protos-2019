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
        public const int IdIndex = 1;

        internal AttrList Data { get; private set; }

        internal HandlerSubscription Handler{ get; private set; }

        internal string Topic{ get; private set; }

        internal SubscribeRequest(HandlerSubscription handler, AttrList data, OnSubscribed completionHandler, string topic) : base(completionHandler)
        {
            Data = data;
            Handler = handler;
            Topic = topic;
        }
    }

    public delegate void OnUnsubscribed(Error error);

    public class UnsubscribeRequest : WAMPConnection.Request<OnUnsubscribed>
    {
        public const int IdIndex = 1;

        internal AttrList Data { get; private set; }

        internal UnsubscribeRequest(OnUnsubscribed completionHandler, AttrList data) : base(completionHandler)
        {
            Data = data;
        }
    }

    public class WAMPRoleSubscriber : WAMPRole
    {
        #region Data structures

        Dictionary<long, HandlerSubscription> _subscriptionHandlers;

        readonly Dictionary<long, SubscribeRequest> _subscribeRequests;
        readonly Dictionary<long, UnsubscribeRequest> _unsubscribeRequests;

        #endregion

        #region Public

        public WAMPRoleSubscriber(WAMPConnection connection) : base(connection)
        {
            _subscriptionHandlers = new Dictionary<long, HandlerSubscription>();

            _subscribeRequests = new Dictionary<long, SubscribeRequest>();
            _unsubscribeRequests = new Dictionary<long, UnsubscribeRequest>();
        }

        public SubscribeRequest CreateSubscribe(string topic, HandlerSubscription handler, OnSubscribed completionHandler)
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

            var data = CreateSubscribeData(topic);
            var request = new SubscribeRequest(handler, data, completionHandler, topic);
            return request;
        }

        public void SendSubscribe(SubscribeRequest request)
        {
            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_subscribeRequests.ContainsKey(requestId), "This requestId was already in use");

            request.Data.SetValue(SubscribeRequest.IdIndex, requestId);
            _subscribeRequests.Add(requestId, request);

            _connection.SendData(request.Data);
        }

        public UnsubscribeRequest CreateUnsubscribe(Subscription subscription, OnUnsubscribed completionHandler)
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

            var data = CreateUnsubscribeData(subscription);
            var request = new UnsubscribeRequest(completionHandler, data);
            return request;
        }

        public void SendUnsubscribe(UnsubscribeRequest request)
        {
            var requestId = _connection.GetAndIncrementRequestId();
            DebugUtils.Assert(!_unsubscribeRequests.ContainsKey(requestId), "This requestId was already in use");

            request.Data.SetValue(UnsubscribeRequest.IdIndex, requestId);
            _unsubscribeRequests.Add(requestId, request);

            _connection.SendData(request.Data);
        }

        public void AutoSubscribe(Subscription subscription, HandlerSubscription handler)
        {
            if(_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                Log.e("This subscriptionId was already in use");
                return;
            }
            _subscriptionHandlers.Add(subscription.Id, handler);
        }

        #endregion

        #region Private

        AttrList CreateSubscribeData(string topic)
        {
            /* [SUBSCRIBE, Request|id, Options|dict, Topic|uri]
             * [32, 713845233, {}, "com.myapp.mytopic1"]
             */
            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.SUBSCRIBE));
            data.Add(new AttrDic());
            data.AddValue(topic);

            //Placeholder id
            data.InsertValue(SubscribeRequest.IdIndex, 0L);

            return data;
        }

        AttrList CreateUnsubscribeData(Subscription subscription)
        {
            /* [UNSUBSCRIBE, Request|id, SUBSCRIBED.Subscription|id]
             * [34, 85346237, 5512315355]
             */
            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.UNSUBSCRIBE));
            data.AddValue(subscription.Id);

            //Placeholder id
            data.InsertValue(UnsubscribeRequest.IdIndex, 0L);

            return data;
        }

        internal void ProcessSubscribed(AttrList msg)
        {
            // [SUBSCRIBED, SUBSCRIBE.Request|id, Subscription|id]

            if(msg.Count != 3)
            {
                Log.e("Invalid SUBSCRIBED message structure - length must be 3");
                return;
            }

            if(!msg.Get(1).IsValue)
            {
                Log.e("Invalid SUBSCRIBED message structure - SUBSCRIBE.Request must be an integer");
                return;
            }

            long requestId = msg.Get(1).AsValue.ToLong();
            SubscribeRequest request;
            if(!_subscribeRequests.TryGetValue(requestId, out request))
            {
                Log.e("Bogus SUBSCRIBED message for non-pending request ID");
                return;
            }

            if(!msg.Get(2).IsValue)
            {
                Log.e("Invalid SUBSCRIBED message structure - SUBSCRIBED.Subscription must be an integer");
                return;
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
                Log.e("Invalid UNSUBSCRIBED message structure - length must be 2");
                return;
            }

            if(!msg.Get(1).IsValue)
            {
                Log.e("Invalid UNSUBSCRIBED message structure - UNSUBSCRIBE.Request must be an integer");
                return;
            }

            long requestId = msg.Get(1).AsValue.ToLong();
            UnsubscribeRequest request;
            if(!_unsubscribeRequests.TryGetValue(requestId, out request))
            {
                Log.e("Bogus UNSUBSCRIBED message for non-pending request ID");
                return;
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
                Log.e("Invalid EVENT message structure - length must be 4, 5 or 6");
                return;
            }

            if(!msg.Get(1).IsValue)
            {
                Log.e("Invalid EVENT message structure - SUBSCRIBED.Subscription must be an integer");
                return;
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
                        Log.e("Invalid RESULT message structure - YIELD.Arguments must be a list");
                        return;
                    }
                    listParams = msg.Get(4).AsList;
                }
                if(msg.Count >= 6)
                {
                    if(!msg.Get(5).IsDic)
                    {
                        Log.e("Invalid RESULT message structure - YIELD.ArgumentsKw must be a dictionary");
                        return;
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
                Log.e("Bogus ERROR message for non-pending SUBSCRIBE request ID");
                return;
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
                Log.e("Bogus ERROR message for non-pending UNSUBSCRIBE request ID");
                return;
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
            var itrSubscribe = _subscribeRequests.GetEnumerator();
            while(itrSubscribe.MoveNext())
            {
                var request = itrSubscribe.Current.Value;
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), null);
                }
            }
            itrSubscribe.Dispose();
            _subscribeRequests.Clear();

            var itrUnsubscribe = _unsubscribeRequests.GetEnumerator();
            while(itrUnsubscribe.MoveNext())
            {
                var request = itrUnsubscribe.Current.Value;
                if(request.CompletionHandler != null)
                {
                    request.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"));
                }
            }
            itrUnsubscribe.Dispose();
            _unsubscribeRequests.Clear();
        }

        #endregion
    }
}
