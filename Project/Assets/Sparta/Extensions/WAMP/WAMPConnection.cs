using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.WAMP
{
    internal static class MsgCode
    {
        internal const int HELLO = 1;
        internal const int WELCOME = 2;
        internal const int ABORT = 3;
        internal const int CHALLENGE = 4;
        internal const int AUTHENTICATE = 5;
        internal const int GOODBYE = 6;
        internal const int ERROR = 8;
        internal const int PUBLISH = 16;
        internal const int PUBLISHED = 17;
        internal const int SUBSCRIBE = 32;
        internal const int SUBSCRIBED = 33;
        internal const int UNSUBSCRIBE = 34;
        internal const int UNSUBSCRIBED = 35;
        internal const int EVENT = 36;
        internal const int CALL = 48;
        internal const int CANCEL = 49;
        internal const int RESULT = 50;
        internal const int REGISTER = 64;
        internal const int REGISTERED = 65;
        internal const int UNREGISTER = 66;
        internal const int UNREGISTERED = 67;
        internal const int INVOCATION = 68;
        internal const int INTERRUPT = 69;
        internal const int YIELD = 70;
    }

    internal static class ErrorCodes
    {
        internal const int NoSession = 1001;
        internal const int JoinInProgress = 1002;
        internal const int LeaveInProgress = 1003;
        internal const int SessionAborted = 1004;
        internal const int CallError = 1005;
        internal const int PublishError = 1006;
        internal const int SubscribeError = 1007;
        internal const int UnsubscribeError = 1008;
        internal const int ConnectionClosed = 1009;
    }

    public class WAMPConnection : INetworkClientDelegate, INetworkMessageReceiver
    {
        #region Data structures

        public class Request<TCompletion> : IDisposable where TCompletion : class
        {
            public TCompletion CompletionHandler{ get; protected set; }

            protected Request(TCompletion completionHandler)
            {
                CompletionHandler = completionHandler;
            }

            void IDisposable.Dispose()
            {
                CompletionHandler = null;
            }

        }

        public class StartRequest : Request<Action>
        {
            internal StartRequest(Action action) : base(action)
            {

            }
        }

        public class StopRequest : Request<Action>
        {
            internal StopRequest(Action action) : base(action)
            {

            }
        }

        public class JoinRequest : Request<OnJoinCompleted>
        {
            internal JoinRequest(OnJoinCompleted completionHandler) : base(completionHandler)
            {

            }
        }

        public class LeaveRequest : Request<OnLeaved>
        {
            internal LeaveRequest(OnLeaved completionHandler) : base(completionHandler)
            {

            }
        }

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

        public class SubscribeRequest : Request<OnSubscribed>
        {
            internal HandlerSubscription Handler{ get; private set; }

            internal string Topic{ get; private set; }

            internal SubscribeRequest(HandlerSubscription handler, OnSubscribed completionHandler, string topic) : base(completionHandler)
            {
                Handler = handler;
                Topic = topic;
            }
        }

        public class UnsubscribeRequest : Request<OnUnsubscribed>
        {
            internal UnsubscribeRequest(OnUnsubscribed completionHandler) : base(completionHandler)
            {
                
            }
        }

        #endregion

        #region Constructor

        public INetworkClient NetworkClient{ get; private set; }

        bool _stopped;
        bool _debug;
        bool _goodbyeSent;
        // WAMP session ID (if the session is joined to a realm).
        long _sessionId;
        // Last request ID of outgoing WAMP requests.
        long _requestId;

        WAMPRolePublisher _publisher;
        WAMPRoleCaller _caller;

        Dictionary<long, HandlerSubscription> _subscriptionHandlers;

        Dictionary<long, SubscribeRequest> _subscribeRequests;
        Dictionary<long, UnsubscribeRequest> _unsubscribeRequests;



        public WAMPConnection(INetworkClient networkClient)
        {
            NetworkClient = networkClient;
            _stopped = false;
            _debug = false;
            _goodbyeSent = false;
            _sessionId = 0;
            _requestId = 0;

            _publisher = new WAMPRolePublisher(this);
            _caller = new WAMPRoleCaller(this);

            _subscriptionHandlers = new Dictionary<long, HandlerSubscription>();

            _subscribeRequests = new Dictionary<long, SubscribeRequest>();
            _unsubscribeRequests = new Dictionary<long, UnsubscribeRequest>();

            NetworkClient.AddDelegate(this);
            NetworkClient.RegisterReceiver(this);
        }

        public void SetDebugMode(bool newValue)
        {
            _debug = newValue;
        }

        internal long GetAndIncrementRequestId()
        {
            var ret = _requestId;
            ++_requestId;
            return ret;
        }

        #endregion

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            if(_startRequest != null && _startRequest.CompletionHandler != null)
            {
                _startRequest.CompletionHandler();
            }
            _startRequest = null;
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            if(_stopRequest != null && _stopRequest.CompletionHandler != null)
            {
                _stopRequest.CompletionHandler();
            }
            _stopRequest = null;
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            
        }

        #endregion

        #region INetworkMessageReceiver implementation

        const string InvalidCodeMsg = "Received invalid WAMP message code: ";

        static string GetInvalidCodeMessage(int code)
        {
            var sb = StringUtils.StartBuilder();
            sb.Append(InvalidCodeMsg);
            sb.Append(code);
            return StringUtils.FinishBuilder(sb);
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            var parser = new JsonAttrParser();
            var attrData = parser.ParseString(reader.ReadString());
            if(attrData.AttrType != AttrType.LIST)
            {
                throw new Exception("Invalid WAMP message structure. It should be a AttrList.");
            }
            var msg = attrData.AsList;
            if(msg.Count == 0 || !msg.Get(0).IsValue)
            {
                throw new Exception(string.Concat("Missing message type field in message: ", msg));
            }
            var code = msg.Get(0).AsValue.ToInt();
            if(_stopped)
            {
                DebugMessage("Recieved message while stopped " + msg);
                return;
            }
            switch(code)
            {
            case MsgCode.HELLO:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.WELCOME:
                ProcessWelcome(msg);
                break;
            case MsgCode.ABORT:
                ProcessAbort(msg);
                break;
            case MsgCode.CHALLENGE:
            case MsgCode.AUTHENTICATE:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.GOODBYE:
                ProcessGoodbye(msg);
                break;
            case MsgCode.ERROR:
                ProcessError(msg);
                break;
            case MsgCode.PUBLISH:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.PUBLISHED:
                _publisher.ProcessPublished(msg);
                break;
            case MsgCode.SUBSCRIBE:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.SUBSCRIBED:
                ProcessSubscribed(msg);
                break;
            case MsgCode.UNSUBSCRIBE:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.UNSUBSCRIBED:
                ProcessUnsubscribed(msg);
                break;
            case MsgCode.EVENT:
                ProcessEvent(msg);
                break;
            case MsgCode.CALL:
            case MsgCode.CANCEL:
            case MsgCode.RESULT:
            case MsgCode.REGISTER:
            case MsgCode.REGISTERED:
            case MsgCode.UNREGISTER:
            case MsgCode.UNREGISTERED:
            case MsgCode.INVOCATION:
            case MsgCode.INTERRUPT:
            case MsgCode.YIELD:
                throw new Exception(GetInvalidCodeMessage(code));
            default:
                throw new ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Session

        internal bool HasActiveSession()
        {
            return _sessionId != 0;
        }

        StartRequest _startRequest;

        public StartRequest Start(Action completionHandler)
        {
            _startRequest = new StartRequest(completionHandler);
            NetworkClient.Connect();

            return _startRequest;
        }

        StopRequest _stopRequest;

        public StopRequest Stop(Action completionHandler)
        {
            _stopRequest = new StopRequest(completionHandler);
            _stopped = true;
            NetworkClient.Disconnect();

            return _stopRequest;
        }

        public delegate void OnJoinCompleted(Error error, long sessionId, AttrDic dict);

        JoinRequest _joinRequest;

        public JoinRequest Join(string realm, AttrDic additionalDetailsDic, OnJoinCompleted completionHandler)
        {
            //There is a JOIN process already in progress, call the handler with an error
            if(_joinRequest != null)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.JoinInProgress, "Another JOIN already in progress"), 0, null);
                }
                return null;
            }

            _joinRequest = new JoinRequest(completionHandler);

            DebugMessage(string.Concat("Joining realm ", realm));

            /* [HELLO, Realm|uri, Details|dict]
             * [1, "somerealm", {
             *      "roles": {
             *          "publisher": {},
             *          "subscriber": {}
             *      }
             * }]
             */

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.HELLO));
            data.AddValue(realm);
            var optionsDict = new AttrDic();
            var rolesDict = new AttrDic();

            optionsDict.Set("roles", rolesDict);
            rolesDict.Set("publisher", new AttrDic());
            rolesDict.Set("subscriber", new AttrDic());
            rolesDict.Set("caller", new AttrDic());

            // Move all the additionalDetailsDict children to optionsDict
            if(additionalDetailsDic != null)
            {
                var itr = additionalDetailsDic.GetEnumerator();
                while(itr.MoveNext())
                {
                    var element = itr.Current;
                    optionsDict.Set(element.Key, element.Value);
                }
            }
            data.Add(optionsDict);

            SendData(data);

            return _joinRequest;
        }

        public bool AbortJoining()
        {
            // Check if there is and active join in process
            if(_joinRequest == null)
            {
                return false;
            }

            DebugMessage("Aborting join");

            /* [ABORT, Details|dict, Reason|uri]
             * [3, {"message": "Joining aborted by client."}, "wamp.error.client_aborting"]
             */
            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.ABORT));
            var abortDetailsDic = new AttrDic();
            abortDetailsDic.SetValue("message", "Joining aborted by client.");
            data.Add(abortDetailsDic);
            data.AddValue("wamp.error.client_aborting");
            SendData(data);

            if(_joinRequest.CompletionHandler != null)
            {
                _joinRequest.CompletionHandler(new Error(ErrorCodes.SessionAborted, "Joining aborted by client"), 0, null);
            }
            _joinRequest = null;

            return true;
        }

        public delegate void OnLeaved(Error error, string reason);

        LeaveRequest _leaveRequest;

        public LeaveRequest Leave(OnLeaved completionHandler, string reason)
        {
            //There is a LEAVE process already in progress, call the handler with an error
            if(_leaveRequest != null)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.LeaveInProgress, "Another LEAVE already in progress"), reason);
                }
                return null;
            }

            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession, "Leaving an inexistent session"), reason);
                }
                return null;
            }

            _leaveRequest = new LeaveRequest(completionHandler);

            DebugMessage(string.Concat("Leaving realm with reason ", reason));

            _goodbyeSent = true;
            SendGoodbye(null, reason);

            return _leaveRequest;
        }

        #endregion

        #region Subscriber

        public delegate void OnSubscribed(Error error, Subscription subscription);

        public SubscribeRequest Subscribe(string topic, HandlerSubscription handler, OnSubscribed completionHandler)
        {
            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error(ErrorCodes.NoSession, "No current session"), null);
                }
                return null;
            }

            DebugMessage(string.Concat("Subscribe to event ", topic));

            /* [SUBSCRIBE, Request|id, Options|dict, Topic|uri]
             * [32, 713845233, {}, "com.myapp.mytopic1"]
             */
            _requestId++;
            DebugUtils.Assert(!_subscribeRequests.ContainsKey(_requestId), "This requestId was already in use");
            var request = new SubscribeRequest(handler, completionHandler, topic);
            _subscribeRequests.Add(_requestId, request);

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.SUBSCRIBE));
            data.AddValue(_requestId);
            data.Add(new AttrDic());
            data.AddValue(topic);

            SendData(data);

            return request;
        }

        public delegate void OnUnsubscribed(Error error);

        public UnsubscribeRequest Unsubscribe(Subscription subscription, OnUnsubscribed completionHandler)
        {
            if(_sessionId == 0)
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

            DebugMessage(string.Concat("Unsubscribe to subscription ", subscription.Id));

            _subscriptionHandlers.Remove(subscription.Id);

            /* [UNSUBSCRIBE, Request|id, SUBSCRIBED.Subscription|id]
             * [34, 85346237, 5512315355]
             */
            _requestId++;
            DebugUtils.Assert(!_unsubscribeRequests.ContainsKey(_requestId), "This requestId was already in use");

            var request = new UnsubscribeRequest(completionHandler);
            _unsubscribeRequests.Add(_requestId, request);

            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.UNSUBSCRIBE));
            data.AddValue(_requestId);
            data.AddValue(subscription.Id);

            SendData(data);

            return request;
        }

        public void Autosubscribe(Subscription subscription, HandlerSubscription handler)
        {
            if(_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                throw new Exception("This subscriptionId was already in use");
            }
            _subscriptionHandlers.Add(subscription.Id, handler);
        }

        #endregion

        #region Private methods

        void SendGoodbye(AttrDic detailsDict, string reason)
        {
            /* [GOODBYE, Details|dict, Reason|uri]
             * [6, {"message": "The host is shutting down now."}, "wamp.error.syste_shutdown"]
             */
            var data = new AttrList();
            data.Add(new AttrInt(MsgCode.GOODBYE));
            if(detailsDict != null)
            {
                data.Add(detailsDict);
            }
            else
            {
                data.Add(new AttrDic());
            }
            data.AddValue(reason);

            SendData(data);
        }

        void ProcessError(AttrList msg)
        {
            // [ERROR, REQUEST.Type|int, REQUEST.Request|id, Details|dict, Error|uri]
            // [ERROR, REQUEST.Type|int, REQUEST.Request|id, Details|dict, Error|uri, Arguments|list]
            // [ERROR, REQUEST.Type|int, REQUEST.Request|id, Details|dict, Error|uri, Arguments|list, ArgumentsKw|dict]

            if(msg.Count < 5 || msg.Count > 7)
            {
                throw new Exception("Invalid ERROR message structure - length must be 5, 6 or 7");
            }

            // REQUEST.Type|int
            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid ERROR message structure - REQUEST.Type must be an integer");
            }
            var requestType = msg.Get(1).AsValue.ToInt();

            // REQUEST.Request|id
            if(!msg.Get(2).IsValue)
            {
                throw new Exception("Invalid ERROR message structure - REQUEST.Request must be an integer");
            }
            long requestId = msg.Get(2).AsValue.ToLong();

            // Details
            string description = string.Empty;
            int code = 0;
            if(msg.Get(3).IsDic)
            {
                var errorDict = msg.Get(3).AsDic;
                description = errorDict.Get("message").AsValue.ToString();
                code = errorDict.Get("code").AsValue.ToInt();
            }

            // Arguments|list
            AttrList listArgs = null;
            if(msg.Count > 5)
            {
                if(!msg.Get(5).IsList)
                {
                    throw new Exception("Invalid ERROR message structure - Arguments must be a list");
                }
                listArgs = msg.Get(5).AsList;
            }

            // ArgumentsKw|list
            AttrDic dictArgs = null;
            if(msg.Count > 6)
            {
                if(!msg.Get(6).IsDic)
                {
                    throw new Exception("Invalid ERROR message structure - ArgumentsKw must be a dictionary");
                }
                dictArgs = msg.Get(6).AsDic;
            }

            switch(requestType)
            {
            case MsgCode.CALL:
                {
                    _caller.ProcessCallError(requestId, code, description, listArgs, dictArgs);
                    break;
                }
            case MsgCode.REGISTER:
            case MsgCode.UNREGISTER:
                throw new Exception("CALLEE role not implemented");
            case MsgCode.PUBLISH:
                {
                    _publisher.ProcessPublishError(requestId, description);
                    break;
                }
            case MsgCode.SUBSCRIBE:
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
                    break;
                }
            case MsgCode.UNSUBSCRIBE:
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
                    break;
                }
            default:
                throw new Exception("Invalid ERROR message - ERROR.Type must one of CALL, REGISTER, UNREGISTER, PUBLISH, SUBSCRIBE, UNSUBSCRIBE");
            }
        }

        void ProcessWelcome(AttrList msg)
        {
            //[WELCOME, Session|id, Details|dict]
            if(msg.Count != 3)
            {
                throw new Exception("Invalid WELCOME message structure - length must be 3");
            }
            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid WELCOME message structure - Session must be an integer");
            }
            if(!msg.Get(2).IsDic)
            {
                throw new Exception("Invalid WELCOME message structure - Details must be a dictionary");
            }

            // If there is no active JOIN it means that has been aborted, so do nothing
            if(_joinRequest == null)
            {
                return;
            }

            _sessionId = msg.Get(1).AsValue.ToLong();
            var detailsDict = msg.Get(2).AsDic;
            if(_joinRequest.CompletionHandler != null)
            {
                _joinRequest.CompletionHandler(null, _sessionId, detailsDict);
            }
            _joinRequest = null;
        }

        void ProcessAbort(AttrList msg)
        {
            //[ABORT, Details|dict, Reason|uri]
            if(msg.Count != 3)
            {
                throw new Exception("Invalid ABORT message structure - length must be 3");
            }
            if(!msg.Get(2).IsValue)
            {
                throw new Exception("Invalid ABORT message structure - Reason must be an string");
            }

            // If there is no active JOIN it means that has been aborted, so do nothing
            if(_joinRequest == null)
            {
                return;
            }

            if(_joinRequest.CompletionHandler != null)
            {
                var reason = msg.Get(2).AsValue.ToString();
                var error = new Error(ErrorCodes.SessionAborted, reason);
                _joinRequest.CompletionHandler(error, 0, null);
            }
            _joinRequest = null;
        }

        void ProcessGoodbye(AttrList msg)
        {
            //[GOODBYE, Details|dict, Reason|uri]
            if(_sessionId == 0)
            {
                throw new Exception("Invalid GOODBYE received when no session was established");
            }

            _sessionId = 0;

            if(!_goodbyeSent)
            {
                // if we did not initiate closing, reply ..
                SendGoodbye(null, "wamp.error.goodbye_and_out");
            }

            if(_leaveRequest != null && _leaveRequest.CompletionHandler != null)
            {
                var reason = msg.Get(2).AsValue.ToString();
                _leaveRequest.CompletionHandler(null, reason);
            }
            _leaveRequest = null;
        }

        void ProcessSubscribed(AttrList msg)
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
            Autosubscribe(subscription, request.Handler);

            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(null, subscription);
            }
            _subscribeRequests.Remove(requestId);
        }

        void ProcessUnsubscribed(AttrList msg)
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



        void ProcessEvent(AttrList msg)
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
                DebugMessage(string.Concat("Skipping EVENT for non-existent subscription ID ", subscriptionId));
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

        internal void SendData(Attr data)
        {
            var serializer = new JsonAttrSerializer();
            var serializedData = serializer.SerializeString(data);

            var networkMessage = NetworkClient.CreateMessage(new NetworkMessageData());
            networkMessage.Writer.Write(serializedData);
            networkMessage.Send();
        }

        internal void DebugMessage(string message)
        {
            if(_debug)
            {
                Log.d(message);
            }
        }

        #endregion
    }
}
