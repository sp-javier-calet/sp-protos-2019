﻿using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.WAMP
{
    public interface IWAMPConnectionDelegate
    {
        void OnClientConnected();

        void OnClientDisconnected();

        void OnNetworkError(Error err);
    }

    public class WAMPConnection : INetworkClientDelegate, INetworkMessageReceiver
    {
        #region Constants

        public enum ErrorCodes
        {
            NoSession = 1001,
            JoinInProgress = 1002,
            LeaveInProgress = 1003,
            SessionAborted = 1004,
            CallError = 1005,
            PublishError = 1006,
            SubscribeError = 1007,
            UnsubscribeError = 1008,
            ConnectionClosed = 1009,
        }

        #endregion

        #region Data structures

        public class IRequest<TCompletion> : IDisposable where TCompletion : class
        {
            public TCompletion CompletionHandler{ get; protected set; }

            protected IRequest(TCompletion completionHandler)
            {
                CompletionHandler = completionHandler;
            }

            void IDisposable.Dispose()
            {
                CompletionHandler = null;
            }

        }

        public class StartRequest : IRequest<Action>
        {
            internal StartRequest(Action action) : base(action)
            {

            }
        }

        public class StopRequest : IRequest<Action>
        {
            internal StopRequest(Action action) : base(action)
            {

            }
        }

        public class JoinRequest : IRequest<OnJoinCompleted>
        {
            internal JoinRequest(OnJoinCompleted completionHandler) : base(completionHandler)
            {

            }
        }

        public class LeaveRequest : IRequest<OnLeaved>
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

        public class SubscribeRequest : IRequest<OnSubscribed>
        {
            internal HandlerSubscription Handler{ get; private set; }

            internal string Topic{ get; private set; }

            internal SubscribeRequest(HandlerSubscription handler, OnSubscribed completionHandler, string topic) : base(completionHandler)
            {
                Handler = handler;
                Topic = topic;
            }
        }

        public class UnsubscribeRequest : IRequest<OnUnsubscribed>
        {
            internal UnsubscribeRequest(OnUnsubscribed completionHandler) : base(completionHandler)
            {
                
            }
        }

        public class Publication
        {
            public long Id{ get; private set; }

            public string Topic{ get; private set; }

            public Publication(long id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        public class PublishRequest : IRequest<OnPublished>
        {
            internal string Topic{ get; private set; }

            public PublishRequest(OnPublished completionHandler, string topic) : base(completionHandler)
            {
                Topic = topic;
            }
        }

        public delegate void HandlerCall(Error error, AttrList args, AttrDic kwargs);

        public class CallRequest : IRequest<HandlerCall>
        {
            internal CallRequest(HandlerCall handler) : base(handler)
            {
            }
        }

        enum MsgCode
        {
            HELLO = 1,
            WELCOME = 2,
            ABORT = 3,
            CHALLENGE = 4,
            AUTHENTICATE = 5,
            GOODBYE = 6,
            ERROR = 8,
            PUBLISH = 16,
            PUBLISHED = 17,
            SUBSCRIBE = 32,
            SUBSCRIBED = 33,
            UNSUBSCRIBE = 34,
            UNSUBSCRIBED = 35,
            EVENT = 36,
            CALL = 48,
            CANCEL = 49,
            RESULT = 50,
            REGISTER = 64,
            REGISTERED = 65,
            UNREGISTER = 66,
            UNREGISTERED = 67,
            INVOCATION = 68,
            INTERRUPT = 69,
            YIELD = 70,
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

        Dictionary<long, HandlerSubscription> _subscriptionHandlers;

        Dictionary<long, SubscribeRequest> _subscribeRequests;
        Dictionary<long, UnsubscribeRequest> _unsubscribeRequests;
        Dictionary<long, PublishRequest> _publishRequests;
        Dictionary<long, CallRequest> _calls;

        public WAMPConnection(INetworkClient networkClient)
        {
            NetworkClient = networkClient;
            _stopped = false;
            _debug = false;
            _goodbyeSent = false;
            _sessionId = 0;
            _requestId = 0;

            _subscriptionHandlers = new Dictionary<long, HandlerSubscription>();

            _subscribeRequests = new Dictionary<long, SubscribeRequest>();
            _unsubscribeRequests = new Dictionary<long, UnsubscribeRequest>();
            _publishRequests = new Dictionary<long, PublishRequest>();
            _calls = new Dictionary<long, CallRequest>();

            NetworkClient.AddDelegate(this);
            NetworkClient.RegisterReceiver(this);
        }

        public void SetDebugMode(bool newValue)
        {
            _debug = newValue;
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

        const string InvalidDataTypeMsg = "Invalid WAMP message structure. It should be a AttrList.";
        const string MissingMessageTypeMsg = "Missing message type field in message: ";
        const string InvalidCodeMsg = "Received invalid WAMP message code: ";

        static string GetInvalidCodeMessage(MsgCode code)
        {
            var sb = StringUtils.StartBuilder();
            sb.Append(InvalidCodeMsg);
            sb.Append(code.ToString());
            return StringUtils.FinishBuilder(sb);
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            var parser = new JsonAttrParser();
            var attrData = parser.ParseString(reader.ReadString());
            if(attrData.AttrType != AttrType.LIST)
            {
                throw new Exception(InvalidDataTypeMsg);
            }
            var msg = attrData.AsList;
            if(msg.Count == 0 || !msg.Get(0).IsValue)
            {
                var sb = StringUtils.StartBuilder();
                sb.Append(MissingMessageTypeMsg);
                sb.Append(msg.ToString());
                throw new Exception(StringUtils.FinishBuilder(sb));
            }
            var code = (MsgCode)msg.Get(0).AsValue.ToInt();
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
                ProcessPublished(msg);
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
                    completionHandler(new Error((int)ErrorCodes.JoinInProgress, "Another JOIN already in progress"), 0, null);
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
            data.Add(new AttrInt((int)MsgCode.HELLO));
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
            data.Add(new AttrInt((int)MsgCode.ABORT));
            var abortDetailsDic = new AttrDic();
            abortDetailsDic.SetValue("message", "Joining aborted by client.");
            data.Add(abortDetailsDic);
            data.AddValue("wamp.error.client_aborting");
            SendData(data);

            if(_joinRequest.CompletionHandler != null)
            {
                _joinRequest.CompletionHandler(new Error((int)ErrorCodes.SessionAborted, "Joining aborted by client"), 0, null);
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
                    completionHandler(new Error((int)ErrorCodes.LeaveInProgress, "Another LEAVE already in progress"), reason);
                }
                return null;
            }

            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.NoSession, "Leaving an inexistent session"), reason);
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
                    completionHandler(new Error((int)ErrorCodes.NoSession, "No current session"), null);
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
            data.Add(new AttrInt((int)MsgCode.SUBSCRIBE));
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
                    completionHandler(new Error((int)ErrorCodes.NoSession, "No current session"));
                }
                return null;
            }

            if(!_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.UnsubscribeError, string.Concat("Invalid subscription id: ", subscription.Id)));
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
            data.Add(new AttrInt((int)MsgCode.UNSUBSCRIBE));
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

        #region Publisher

        public delegate void OnPublished(Error error, Publication pub);

        public void Publish(string topic, AttrList args, AttrDic kwargs, bool acknowledged, OnPublished completionHandler)
        {
            DebugUtils.Assert((acknowledged && completionHandler != null) || !acknowledged, "Asked for acknowledge but without completionHandler");

            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.NoSession), null);
                }
                return;
            }

            DebugMessage(string.Concat("Publish event ", topic, " with args", args, " and ", kwargs));

            /* [PUBLISH, Request|id, Options|dict, Topic|uri, Arguments|list, ArgumentsKw|dict]
             * [16, 239714735, {}, "com.myapp.mytopic1", [], {"color": "orange", "sizes": [23, 42, 7]}]
             */
            _requestId++;
            if(acknowledged)
            {
                DebugUtils.Assert(!_publishRequests.ContainsKey(_requestId), "This requestId was already in use");
                _publishRequests.Add(_requestId, new PublishRequest(completionHandler, topic));
            }

            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.PUBLISH));
            data.AddValue(_requestId);
            var optionsDict = new AttrDic();
            if(acknowledged)
            {
                optionsDict.SetValue("acknowledge", true);
            }
            data.Add(optionsDict);
            data.AddValue(topic);
            if(args != null)
            {
                data.Add(args);
            }
            else
            {
                data.Add(new AttrList());
            }

            if(kwargs != null)
            {
                data.Add(kwargs);
            }

            SendData(data);
        }

        #endregion

        #region Caller

        public CallRequest Call(string procedure, AttrList args, AttrDic kwargs, HandlerCall resultHandler)
        {
            if(_sessionId == 0)
            {
                if(resultHandler != null)
                {
                    resultHandler(new Error((int)ErrorCodes.NoSession), null, null);
                }
                return null;
            }

            DebugMessage(string.Concat("Request call ", procedure));

            _requestId++;
            DebugUtils.Assert(!_calls.ContainsKey(_requestId), "This requestId was already in use");
            var request = new CallRequest(resultHandler);
            _calls.Add(_requestId, request);

            /* [CALL, Request|id, Options|dict, Procedure|uri]
             * [48, 7814135, {}, "com.myapp.user.new", ["johnny"], {"firstname": "John", "surname": "Doe"}]
             */
            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.CALL));
            data.AddValue(_requestId);
            data.Add(new AttrDic());
            data.AddValue(procedure);
            if(args != null)
            {
                data.Add(args);
            }
            else
            {
                data.Add(new AttrList());
            }

            if(kwargs != null)
            {
                data.Add(kwargs);
            }

            SendData(data);

            return request;
        }

        #endregion

        #region Private methods

        void SendGoodbye(AttrDic detailsDict, string reason)
        {
            /* [GOODBYE, Details|dict, Reason|uri]
             * [6, {"message": "The host is shutting down now."}, "wamp.error.syste_shutdown"]
             */
            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.GOODBYE));
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
            var requestType = (MsgCode)(msg.Get(1).AsValue.ToInt());

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
                    CallRequest request;
                    if(!_calls.TryGetValue(requestId, out request))
                    {
                        throw new Exception("Bogus ERROR message for non-pending CALL request ID");
                    }
                    if(request.CompletionHandler != null)
                    {
                        request.CompletionHandler(new Error(code, description), listArgs, dictArgs);
                    }
                    _calls.Remove(requestId);
                    break;
                }
            case MsgCode.REGISTER:
            case MsgCode.UNREGISTER:
                throw new Exception("CALLEE role not implemented");
            case MsgCode.PUBLISH:
                {
                    PublishRequest request;
                    if(!_publishRequests.TryGetValue(requestId, out request))
                    {
                        throw new Exception("Bogus ERROR message for non-pending PUBLISH request ID");
                    }
                    if(request.CompletionHandler != null)
                    {
                        request.CompletionHandler(new Error((int)ErrorCodes.PublishError, description), new Publication(requestId, request.Topic));
                    }
                    _publishRequests.Remove(requestId);
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
                        request.CompletionHandler(new Error((int)ErrorCodes.SubscribeError, description), new Subscription(requestId, request.Topic));
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
                        request.CompletionHandler(new Error((int)ErrorCodes.UnsubscribeError, description));
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
                var error = new Error((int)ErrorCodes.SessionAborted, reason);
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

        void ProcessCallResult(AttrList msg)
        {
            // [RESULT, CALL.Request|id, Details|dict]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list]
            // [RESULT, CALL.Request|id, Details|dict, YIELD.Arguments|list, YIELD.ArgumentsKw|dict]

            if(msg.Count < 3 || msg.Count > 5)
            {
                throw new Exception("Invalid RESULT message structure - length must be 3, 4 or 5");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid RESULT message structure - CALL.Request must be an integer");
            }
            long requestId = msg.Get(1).AsValue.ToLong();

            CallRequest request; 
            if(!_calls.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus RESULT message for non-pending request ID");
            }

            if(request.CompletionHandler != null)
            {
                AttrList listParams = null;
                AttrDic dictParams = null;
                if(msg.Count >= 4)
                {
                    if(!msg.Get(3).IsList)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.Arguments must be a list");
                    }
                    listParams = msg.Get(3).AsList;
                }
                if(msg.Count >= 5)
                {
                    if(!msg.Get(4).IsDic)
                    {
                        throw new Exception("Invalid RESULT message structure - YIELD.ArgumentsKw must be a dictionary");
                    }
                    dictParams = msg.Get(4).AsDic;
                }
                request.CompletionHandler(null, listParams, dictParams);
            }
            _calls.Remove(requestId);
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

        void ProcessPublished(AttrList msg)
        {
            // [PUBLISHED, PUBLISH.Request|id, Publication|id]
            if(msg.Count != 3)
            {
                throw new Exception("Invalid PUBLISHED message structure - length must be 3");
            }

            if(!msg.Get(1).IsValue)
            {
                throw new Exception("Invalid PUBLISHED message structure - PUBLISHED.Request must be an integer");
            }

            long requestId = msg.Get(1).AsValue.ToLong();

            PublishRequest request;
            if(!_publishRequests.TryGetValue(requestId, out request))
            {
                throw new Exception("Bogus PUBLISHED message for non-pending request ID");
            }

            if(!msg.Get(2).IsValue)
            {
                throw new Exception("Invalid PUBLISHED message structure - PUBLISHED.Subscription must be an integer");
            }
            long publicationId = msg.Get(2).AsValue.ToLong();

            if(request.CompletionHandler != null)
            {
                request.CompletionHandler(null, new Publication(publicationId, request.Topic));
            }
            _publishRequests.Remove(requestId);
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

        void SendData(Attr data)
        {
            var serializer = new JsonAttrSerializer();
            var serializedData = serializer.SerializeString(data);

            var networkMessage = NetworkClient.CreateMessage(new NetworkMessageData());
            networkMessage.Writer.Write(serializedData);
            networkMessage.Send();
        }

        void DebugMessage(string message)
        {
            if(_debug)
            {
                Log.d(message);
            }
        }

        #endregion
    }
}
