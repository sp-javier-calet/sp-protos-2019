using System;
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

        public class Subscription
        {
            public ulong Id{ get; }

            public string Topic{ get; }

            public Subscription(ulong id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        public delegate void HandlerSubscription(AttrList args, AttrDic kwargs);

        class SubscribeRequest
        {
            internal HandlerSubscription Handler{ get; }

            internal WeakReference<OnSubscribed> CompletionHandler{ get; }

            internal string Topic{ get; }

            internal SubscribeRequest(HandlerSubscription handler, WeakReference<OnSubscribed> completionHandler, string topic)
            {
                Handler = handler;
                CompletionHandler = completionHandler;
                Topic = topic;
            }
        }

        public class Publication
        {
            public ulong Id{ get; }

            public string Topic{ get; }

            public Publication(ulong id, string topic)
            {
                Id = id;
                Topic = topic;
            }
        }

        class PublishRequest
        {
            internal WeakReference<OnPublished> CompletionHandler{ get; }

            internal string Topic{ get; }

            public PublishRequest(WeakReference<OnPublished> completionHandler, string topic)
            {
                CompletionHandler = completionHandler;
                Topic = topic;
            }
        }

        public delegate void HandlerCall(Error error, AttrList args, AttrDic kwargs);

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

        INetworkClient NetworkClient{ get; }

        bool _stopped;
        bool _debug;
        bool _goodbyeSent;
        // WAMP session ID (if the session is joined to a realm).
        ulong _sessionId;
        // Last request ID of outgoing WAMP requests.
        ulong _requestId;

        Dictionary<ulong, SubscribeRequest> _subscribeRequests;
        Dictionary<ulong, List<WeakReference<HandlerSubscription>>> _subscriptionHandlers;
        Dictionary<ulong, WeakReference<OnUnsubscribed>> _unsubscribeRequests;
        Dictionary<ulong, PublishRequest> _publishRequests;
        Dictionary<ulong, WeakReference<HandlerCall>> _calls;

        WAMPConnection(INetworkClient networkClient)
        {
            NetworkClient = networkClient;
            _stopped = false;
            _debug = false;
            _goodbyeSent = false;
            _sessionId = 0;
            _requestId = 0;

            NetworkClient.AddDelegate(this);
            NetworkClient.RegisterReceiver(this);
        }

        public void setDebugMode(bool newValue)
        {
            _debug = newValue;
        }

        #endregion

        #region INetworkClientDelegate implementation

        void INetworkClientDelegate.OnClientConnected()
        {
            if(_startCompletionHandler != null)
            {
                _startCompletionHandler();
                _startCompletionHandler = null;
            }
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            if(_stopCompletionHandler != null)
            {
                _stopCompletionHandler();
                _stopCompletionHandler = null;
            }
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

        string getInvalidCodeMessage(MsgCode code)
        {
            var sb = StringUtils.StartBuilder();
            sb.Append(InvalidCodeMsg);
            sb.Append(code.ToString());
            return StringUtils.FinishBuilder(sb);
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, SocialPoint.IO.IReader reader)
        {
            JsonAttrParser parser;
            var attrData = parser.ParseString(reader.ReadString());
            if(attrData.AttrType != AttrType.LIST)
            {
                throw new System.Exception(InvalidDataTypeMsg);
                return;
            }
            var msg = attrData.AsList;
            if(msg.Count == 0 || !msg.Get(0).IsValue)
            {
                var sb = StringUtils.StartBuilder();
                sb.Append(MissingMessageTypeMsg);
                sb.Append(msg.ToString());
                throw new System.Exception(StringUtils.FinishBuilder(sb));
            }
            MsgCode code = (MsgCode)msg.Get(0).AsValue.ToInt();
            if(_stopped)
            {
                debugMessage("Recieved message while stopped " + msg);
                return;
            }
            switch(code)
            {
            case MsgCode.HELLO:
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            case MsgCode.WELCOME:
                processWelcome(msg);
                break;
            case MsgCode.ABORT:
                processAbort(msg);
                break;
            case MsgCode.CHALLENGE:
            case MsgCode.AUTHENTICATE:
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            case MsgCode.GOODBYE:
                processGoodbye(msg);
                break;
            case MsgCode.ERROR:
                processError(msg);
                break;
            case MsgCode.PUBLISH:
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            case MsgCode.PUBLISHED:
                processPublished(msg);
                break;
            case MsgCode.SUBSCRIBE:
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            case MsgCode.SUBSCRIBED:
                processSubscribed(msg);
                break;
            case MsgCode.UNSUBSCRIBE:
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            case MsgCode.UNSUBSCRIBED:
                processUnsubscribed(msg);
                break;
            case MsgCode.EVENT:
                processEvent(msg);
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
                throw new System.Exception(getInvalidCodeMessage(code));
                break;
            default:
                throw new System.ArgumentOutOfRangeException();
            }
        }

        #endregion

        #region Session

        Action _startCompletionHandler;

        public void start(Action completionHandler)
        {
            _startCompletionHandler = completionHandler;
            NetworkClient.Connect();
        }

        Action _stopCompletionHandler;

        public void stop(Action completionHandler)
        {
            _stopCompletionHandler = completionHandler;
            _stopped = true;
            NetworkClient.Disconnect();
        }

        public delegate void OnJoinCompleted(Error error, AttrDic dict);

        OnJoinCompleted _joinCompleteHandler;

        public void join(string realm, AttrDic additionalDetailsDic, OnJoinCompleted completionHandler)
        {
            //There is a JOIN process already in progress, call the handler with an error
            if(_joinCompleteHandler != null)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.JoinInProgress, "Another JOIN already in progress"), new AttrDic());
                }
                return;
            }

            if(completionHandler != null)
            {
                _joinCompleteHandler = completionHandler;
            }
            else
            {
                _joinCompleteHandler = delegate {
                };
            }

            debugMessage(string.Concat("Joining realm ", realm));

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

            sendData(data);
        }

        public bool abortJoining()
        {
            // Check if there is and active join in process
            if(_joinCompleteHandler == null)
            {
                return false;
            }

            debugMessage("Aborting join");

            /* [ABORT, Details|dict, Reason|uri]
             * [3, {"message": "Joining aborted by client."}, "wamp.error.client_aborting"]
             */
            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.ABORT));
            var abortDetailsDic = new AttrDic();
            abortDetailsDic.SetValue("message", "Joining aborted by client.");
            data.Add(abortDetailsDic);
            data.AddValue("wamp.error.client_aborting");
            sendData(data);

            _joinCompleteHandler(new Error((int)ErrorCodes.SessionAborted, "Joining aborted by client"), new AttrDic());
            _joinCompleteHandler = null;

            return true;
        }

        public delegate void OnLeaved(Error error, string reason);

        OnLeaved _leaveCompletedHandler;

        public void leave(OnLeaved completionHandler, string reason)
        {
            //There is a LEAVE process already in progress, call the handler with an error
            if(_leaveCompletedHandler != null)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.LeaveInProgress, "Another LEAVE already in progress"), reason);
                }
                return;
            }

            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.NoSession, "Leaving an inexistent session"), reason);
                }
                return;
            }

            if(completionHandler != null)
            {
                _leaveCompletedHandler = completionHandler;
            }
            else
            {
                _leaveCompletedHandler = delegate {
                };
            }

            debugMessage(string.Concat("Leaving realm with reason ", reason));

            _goodbyeSent = true;
            sendGoodbye(new AttrDic(), reason);
        }

        #endregion

        #region Subscriber

        public delegate void OnSubscribed(Error error, Subscription subscription);

        public void subscribe(string topic, HandlerSubscription handler, OnSubscribed completionHandler)
        {
            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.NoSession, "No current session"), null);
                }
                return;
            }

            debugMessage(string.Concat("Subscribe to event ", topic));

            /* [SUBSCRIBE, Request|id, Options|dict, Topic|uri]
             * [32, 713845233, {}, "com.myapp.mytopic1"]
             */
            _requestId++;
            DebugUtils.Assert(!_subscribeRequests.ContainsKey(_requestId), "This requestId was already in use");
            _subscribeRequests.Add(_requestId, new SubscribeRequest(handler, completionHandler, topic));

            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.SUBSCRIBE));
            data.AddValue(_requestId);
            data.Add(new AttrDic());
            data.AddValue(topic);

            sendData(data);
        }

        public delegate void OnUnsubscribed(Error error);

        public void unsubscribe(Subscription subscription, OnUnsubscribed completionHandler)
        {
            if(_sessionId == 0)
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.NoSession, "No current session"));
                }
                return;
            }

            if(!_subscriptionHandlers.ContainsKey(subscription.Id))
            {
                if(completionHandler != null)
                {
                    completionHandler(new Error((int)ErrorCodes.UnsubscribeError, string.Concat("Invalid subscription id: ", subscription.Id)));
                }
                return;
            }

            debugMessage(string.Concat("Unsubscribe to subscription ", subscription.Id));

            _subscriptionHandlers.Remove(subscription.Id);

            /* [UNSUBSCRIBE, Request|id, SUBSCRIBED.Subscription|id]
             * [34, 85346237, 5512315355]
             */
            _requestId++;
            DebugUtils.Assert(!_unsubscribeRequests.ContainsKey(_requestId), "This requestId was already in use");
            _unsubscribeRequests.Add(_requestId, completionHandler);

            var data = new AttrList();
            data.Add(new AttrInt((int)MsgCode.UNSUBSCRIBE));
            data.AddValue(_requestId);
            data.AddValue(subscription.Id);

            sendData(data);
        }

        public void autosubscribe(Subscription subscription, HandlerSubscription handler)
        {
            List<HandlerSubscription> list;
            if(!_subscriptionHandlers.TryGetValue(subscription.Id, out list))
            {
                list = new List<HandlerSubscription>();
                _subscriptionHandlers.Add(subscription.Id, list);
            }

            list.Add(handler);
        }

        #endregion

        #region Publisher

        public delegate void OnPublished(Error error, Publication pub);

        public void publish(string topic, AttrList args, AttrDic kwargs, bool acknowledged, OnPublished completionHandler)
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

            debugMessage(string.Concat("Publish event ", topic, " with args", args, " and ", kwargs));

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
            if(args)
            {
                data.Add(args);
            }
            else
            {
                data.Add(new AttrList());
            }

            if(kwargs)
            {
                data.Add(kwargs);
            }

            sendData(data);
        }

        #endregion

        #region Caller

        public void call(string procedure, HandlerCall resultHandler)
        {
            if(_sessionId == 0)
            {
                if(resultHandler != null)
                {
                    resultHandler(new Error((int)ErrorCodes.NoSession), null, null);
                }
                return;
            }

            debugMessage(string.Concat("Request call ", procedure));

            _requestId++;
            DebugUtils.Assert(!_calls.ContainsKey(_requestId), "This requestId was already in use");
            _calls.Add(_requestId, resultHandler);
        }

        #endregion


        void sendData(Attr data)
        {
            var serializer = new JsonAttrSerializer();
            var serializedData = serializer.SerializeString(data);

            var networkMessage = NetworkClient.CreateMessage(new NetworkMessageData());
            networkMessage.Writer.Write(serializedData);
            networkMessage.Send();
        }

        void debugMessage(string message)
        {
            if(_debug)
            {
                Log.d(message);
            }
        }
    }
}
