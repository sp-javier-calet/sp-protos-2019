using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.WAMP.Caller;
using SocialPoint.WAMP.Publisher;
using SocialPoint.WAMP.Subscriber;

namespace SocialPoint.WAMP
{
    public static class MsgCode
    {
        public const int HELLO = 1;
        public const int WELCOME = 2;
        public const int ABORT = 3;
        public const int CHALLENGE = 4;
        public const int AUTHENTICATE = 5;
        public const int GOODBYE = 6;
        public const int ERROR = 8;
        public const int PUBLISH = 16;
        public const int PUBLISHED = 17;
        public const int SUBSCRIBE = 32;
        public const int SUBSCRIBED = 33;
        public const int UNSUBSCRIBE = 34;
        public const int UNSUBSCRIBED = 35;
        public const int EVENT = 36;
        public const int CALL = 48;
        public const int CANCEL = 49;
        public const int RESULT = 50;
        public const int REGISTER = 64;
        public const int REGISTERED = 65;
        public const int UNREGISTER = 66;
        public const int UNREGISTERED = 67;
        public const int INVOCATION = 68;
        public const int INTERRUPT = 69;
        public const int YIELD = 70;
    }

    static class ErrorCodes
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

    public interface WAMPRequest : IDisposable
    {
    }

    public class WAMPConnection : INetworkClientDelegate, INetworkMessageReceiver
    {
        #region Data structures

        public class Request<TCompletion> : WAMPRequest where TCompletion : class
        {
            public TCompletion CompletionHandler{ get; protected set; }

            protected Request(TCompletion completionHandler)
            {
                CompletionHandler = completionHandler;
            }

            public void Dispose()
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

        public delegate void OnJoinCompleted(Error error, long sessionId, AttrDic dict);

        public class JoinRequest : Request<OnJoinCompleted>
        {
            internal JoinRequest(OnJoinCompleted completionHandler) : base(completionHandler)
            {

            }
        }

        public delegate void OnLeft(Error error, string reason);

        public class LeaveRequest : Request<OnLeft>
        {
            internal LeaveRequest(OnLeft completionHandler) : base(completionHandler)
            {

            }
        }

        #endregion

        #region Constructor

        public INetworkClient NetworkClient{ get; private set; }

        public bool Debug{ get; set; }

        bool _stopped;
        bool _goodbyeSent;
        // WAMP session ID (if the session is joined to a realm).
        long _sessionId;
        // Last request ID of outgoing WAMP requests.
        long _requestId;

        readonly JsonAttrParser _jsonParser;

        WAMPRolePublisher _publisher;
        WAMPRoleCaller _caller;
        WAMPRoleSubscriber _subscriber;

        readonly List<WAMPRole> _roles;

        StartRequest _startRequest;
        StopRequest _stopRequest;

        JoinRequest _joinRequest;
        LeaveRequest _leaveRequest;

        public WAMPConnection(INetworkClient networkClient)
        {
            NetworkClient = networkClient;
            _stopped = false;
            Debug = false;
            _goodbyeSent = false;
            _sessionId = 0;
            _requestId = 0;

            _jsonParser = new JsonAttrParser();

            _roles = new List<WAMPRole>();

            _publisher = new WAMPRolePublisher(this);
            _roles.Add(_publisher);

            _caller = new WAMPRoleCaller(this);
            _roles.Add(_caller);

            _subscriber = new WAMPRoleSubscriber(this);
            _roles.Add(_subscriber);

            NetworkClient.AddDelegate(this);
            NetworkClient.RegisterReceiver(this);
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
            DebugMessage("Connection established");

            if(_startRequest != null && _startRequest.CompletionHandler != null)
            {
                _startRequest.CompletionHandler();
            }
            _startRequest = null;
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            DebugMessage("Connection closed");

            if(_stopRequest != null && _stopRequest.CompletionHandler != null)
            {
                _stopRequest.CompletionHandler();
            }
            _stopRequest = null;


            ResetToInitialState();
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
            
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            DebugMessage("Connection error. Closing connection");
            Stop(null);
            ResetToInitialState();
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
            var attrData = _jsonParser.ParseString(reader.ReadString());
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
                _subscriber.ProcessSubscribed(msg);
                break;
            case MsgCode.UNSUBSCRIBE:
                throw new Exception(GetInvalidCodeMessage(code));
            case MsgCode.UNSUBSCRIBED:
                _subscriber.ProcessUnsubscribed(msg);
                break;
            case MsgCode.EVENT:
                _subscriber.ProcessEvent(msg);
                break;
            case MsgCode.RESULT:
                _caller.ProcessCallResult(msg);
                break;
            case MsgCode.CALL:
            case MsgCode.CANCEL:
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

        public StartRequest Start(Action completionHandler)
        {
            _startRequest = new StartRequest(completionHandler);
            NetworkClient.Connect();

            return _startRequest;
        }

        public StopRequest Stop(Action completionHandler)
        {
            _stopRequest = new StopRequest(completionHandler);
            _stopped = true;
            NetworkClient.Disconnect();

            return _stopRequest;
        }

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

            for(var i = 0; i < _roles.Count; i++)
            {
                _roles[i].AddRoleDetails(rolesDict);
            }

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

        public LeaveRequest Leave(OnLeft completionHandler, string reason)
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

        #region Public methods

        public SubscribeRequest CreateSubscribe(string topic, HandlerSubscription handler, OnSubscribed completionHandler)
        {
            return _subscriber.CreateSubscribe(topic, handler, completionHandler);
        }

        public void SendSubscribe(SubscribeRequest request)
        {
            _subscriber.SendSubscribe(request);
        }

        public UnsubscribeRequest CreateUnsubscribe(Subscription subscription, OnUnsubscribed completionHandler)
        {
            return _subscriber.CreateUnsubscribe(subscription, completionHandler);
        }

        public void SendUnsubscribe(UnsubscribeRequest request)
        {
            _subscriber.SendUnsubscribe(request);
        }

        public void AutoSubscribe(Subscription subscription, HandlerSubscription handler)
        {
            _subscriber.AutoSubscribe(subscription, handler);
        }

        public PublishRequest CreatePublish(string topic, AttrList args, AttrDic kwargs, bool acknowledged, OnPublished completionHandler)
        {
            return _publisher.CreatePublish(topic, args, kwargs, acknowledged, completionHandler);
        }

        public void SendPublish(PublishRequest request)
        {
            _publisher.SendPublish(request);
        }

        public CallRequest CreateCall(string procedure, AttrList args, AttrDic kwargs, HandlerCall resultHandler)
        {
            return _caller.CreateCall(procedure, args, kwargs, resultHandler);
        }

        public void SendCall(CallRequest request)
        {
            _caller.SendCall(request);
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

            // Error initialization
            string errorDescription = msg.Get(4).AsValue.ToString();
            int code = 0;

            // Details
            if(msg.Get(3).IsDic)
            {
                var errorDict = msg.Get(3).AsDic;
                // TODO Hides actual WAMP Error. Move to AttrList.
                errorDescription = errorDict.Get("message").AsValue.ToString();
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
                    var errorCode = code == 0 ? ErrorCodes.CallError : code;
                    _caller.ProcessCallError(requestId, errorCode, errorDescription, listArgs, dictArgs);
                    break;
                }
            case MsgCode.REGISTER:
            case MsgCode.UNREGISTER:
                throw new Exception("CALLEE role not implemented");
            case MsgCode.PUBLISH:
                {
                    _publisher.ProcessPublishError(requestId, errorDescription);
                    break;
                }
            case MsgCode.SUBSCRIBE:
                {
                    _subscriber.ProcessSubscribeError(requestId, errorDescription);
                    break;
                }
            case MsgCode.UNSUBSCRIBE:
                {
                    _subscriber.ProcessUnsubscribeError(requestId, errorDescription);
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
            if(Debug)
            {
                Log.d(string.Concat("WAMPConnection: " + message));
            }
        }

        internal void ResetToInitialState()
        {
            _stopped = false;
            _goodbyeSent = false;
            _requestId = 0;

            if(_joinRequest != null && _joinRequest.CompletionHandler != null)
            {
                _joinRequest.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), 0, null);
            }
            _joinRequest = null;

            if(_leaveRequest != null && _leaveRequest.CompletionHandler != null)
            {
                _leaveRequest.CompletionHandler(new Error(ErrorCodes.ConnectionClosed, "Connection reset"), "wamp.error.connection_reset");
            }
            _leaveRequest = null;

            for(var i = 0; i < _roles.Count; i++)
            {
                _roles[i].ResetToInitialState();
            }
        }

        #endregion
    }
}
