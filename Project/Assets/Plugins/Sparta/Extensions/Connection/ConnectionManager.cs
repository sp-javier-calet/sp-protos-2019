using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Utils;
using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.WAMP;
using SocialPoint.WAMP.Caller;
using SocialPoint.WAMP.Publisher;
using SocialPoint.WAMP.Subscriber;

namespace SocialPoint.Connection
{
    public static class NotificationType
    {
        public const int ChatWarning = 99;
        public const int TextMessage = 100;

        // Personal notifications
        public const int NotificationAllianceMemberAccept = 1;
        public const int NotificationAllianceMemberKickoff = 2;
        public const int NotificationAllianceMemberPromote = 3;
        public const int NotificationAllianceJoinRequest = 4;
        public const int NotificationAlliancePlayerAutoPromote = 109;
        public const int NotificationAlliancePlayerAutoDemote = 110;
        public const int NotificationUserChatBan = 307;

        // Alliance notifications
        public const int BroadcastAllianceMemberAccept = 101;
        public const int BroadcastAllianceJoin = 102;
        public const int BroadcastAllianceMemberKickoff = 103;
        public const int BroadcastAllianceMemberLeave = 104;
        public const int BroadcastAllianceEdit = 105;
        public const int BroadcastAllianceMemberPromote = 106;
        public const int BroadcastAllianceMemberRankChange = 111;
        public const int BroadcastAllianceOnlineMember = 308;

        //Matchmaking notifications
        public const int MatchmakingSuccessNotification = 502;
        public const int MatchmakingTimeoutNotification = 503;
        public const int MatchmakingWaitingTimeNotification = 504;
        public const int MatchmakingCanceled = 505;

        // Message System
        public const int MessagingSystemNewMessage = 600;

        // Donations System
        public const int BroadcastDonationRequest = 700;
        public const int BroadcastDonationContribute = 701;
        public const int BroadcastDonationRemove = 702;
        public const int BroadcastDonationUserRemove = 703;

        public const int MaxValue = 1000;
    }

    [Serializable]
    public class ConnectionManagerConfig
    {
        public float PingInterval = 10.0f;
        public float ReconnectInterval = 1.0f;

        public int RPCRetries = 1;
        public float RPCTimeout = 1.0f;
    }

    public class ConnectionManager : INetworkClientDelegate, IUpdateable, IDisposable
    {
        #region Attr keys

        //Alliances & Connection
        public const string ServicesKey = "services";

        //Chat & Connection
        public const string TopicsKey = "topics";
        public const string SubscriptionIdTopicKey = "subscription_id";
        public const string NotificationTypeKey = "type";

        //Connection
        public const string NotificationsServiceKey = "notification";
        public const string PendingKey = "pending";
        public const string NotificationPayloadKey = "payload";

        #endregion

        const string NotificationTopicType = "notification";
        const string NotificationTopicName = "notifications";

        const int WillGoBackgroundPriority = int.MaxValue;
        const int WasOnBackgroundPriority = int.MaxValue;

        /// <summary>
        /// User data to allow forcing user parameters
        /// </summary>
        public class UserData
        {
            public ulong UserId;
            public string SecurityToken;
            public string PrivilegeToken;

            public UserData(ulong userId, string securityToken)
            {
                UserId = userId;
                SecurityToken = securityToken;
                PrivilegeToken = string.Empty;
            }

            public UserData(ILoginData loginData)
            {
                UserId = loginData.UserId;
                SecurityToken = loginData.SecurityToken;
                PrivilegeToken = loginData.PrivilegeToken;
            }
        }

        class RequestWrapper
        {
            public enum RequestType
            {
                Unknown,
                Publish,
                Call,
            }

            public RequestType Type;
            public WAMPRequest Request;
        }

        public delegate void NotificationReceivedDelegate(int type, string topic, AttrDic dictParams);

        public event Action OnConnectionStablished;
        public event Action OnConnected;
        public event Action OnClosed;
        public event Action<AttrDic> OnProcessServices;
        public event Action<Error> OnError;
        public event Action<Error> OnRPCError;
        public event NotificationReceivedDelegate OnNotificationReceived;
        public event NotificationReceivedDelegate OnPendingNotification;

        ConnectionManagerConfig _config;

        public ConnectionManagerConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
                if(value == null)
                {
                    throw new InvalidOperationException("ConnectionManager Config cannot be null");
                }

                _config = value;

                //Re-schedule with updated values
                if(_active)
                {
                    UnschedulePing();
                    SchedulePing();
                }
            }
        }

        IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            set
            {
                if(_appEvents != null)
                {
                    _appEvents.GameWillRestart.Remove(Disconnect);
                    _appEvents.WillGoBackground.Remove(OnWillGoBackground);
                    _appEvents.WasOnBackground.Remove(OnWasOnBackground);
                    _appEvents.WasCovered -= OnWasOnBackground;
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.GameWillRestart.Add(0, Disconnect);
                    _appEvents.WillGoBackground.Add(WillGoBackgroundPriority, OnWillGoBackground);
                    _appEvents.WasOnBackground.Add(WasOnBackgroundPriority, OnWasOnBackground);
                    _appEvents.WasCovered += OnWasOnBackground;
                }
            }
        }

        IUpdateScheduler _scheduler;

        public IUpdateScheduler Scheduler
        {
            set
            {
                if(_scheduler != null)
                {
                    _scheduler.Remove(this);
                }
                _scheduler = value;
                _scheduler.Add(this);
            }
        }

        public bool IsConnected
        {
            get
            {
                return _joined;
            }
        }

        //Note: Social Framewoork's Admin Panel calls this by reflection. Update if renamed
        bool IsSocketConnected
        {
            get
            {
                return _socket.Connected;
            }
        }

        //Note: Social Framewoork's Admin Panel calls this by reflection. Update if renamed
        bool IsSocketConnecting
        {
            get
            {
                return _socket.Connecting;
            }
        }

        bool IsSocketInStandby
        {
            get
            {
                return _socket.InStandby;
            }
        }

        public float RPCTimeout
        {
            get
            {
                return (Config.RPCRetries + 1) * Config.RPCTimeout;
            }
        }

        public string[] Urls
        {
            get
            {
                return _socket.Urls;
            }
        }

        public string ConnectedUrl
        {
            get
            {
                return _socket.ConnectedUrl;
            }
        }

        bool _debugEnabled;

        public bool DebugEnabled
        {
            get
            {
                return _debugEnabled;
            }
            set
            {
                _debugEnabled = value;
                _connection.Debug = value;
            }
        }

        public ILoginData LoginData { get; set; }

        public SocialPoint.Social.IPlayerData PlayerData { get; set; }

        public Localization Localization { get; set; }

        public IDeviceInfo DeviceInfo { get; set; }

        public UserData ForcedUser { get; set; }

        readonly WAMPConnection _connection;
        readonly IWebSocketClient _socket;

        ScheduledAction _pingUpdate;
        ScheduledAction _reconnectUpdate;

        bool _active;
        bool _joined;
        Queue<RequestWrapper> _pendingRequests;

        public ConnectionManager(IWebSocketClient client, ConnectionManagerConfig config)
        {
            DebugUtils.Assert(client != null, "IWebsocketClient is required");
            _socket = client;
            _socket.AddDelegate(this);
            _connection = new WAMPConnection(client);
            _config = config ?? new ConnectionManagerConfig();

            _active = false;
            _joined = false;
            _pendingRequests = new Queue<RequestWrapper>();
        }

        public void Dispose()
        {
            Disconnect();
            if(_appEvents != null)
            {   
                _appEvents.GameWillRestart.Remove(Disconnect);
            }

            if(_pingUpdate != null)
            {
                _pingUpdate.Dispose();
            }

            if(_reconnectUpdate != null)
            {
                _reconnectUpdate.Dispose();
            }
        }

        public void AutosubscribeToTopic(string topic, Subscription subscription)
        {
            _connection.AutoSubscribe(subscription, (args, kwargs) => OnNotificationMessageReceived(topic, args, kwargs));
        }

        public WAMPConnection.StartRequest Connect()
        {
            DebugUtils.Assert(LoginData.Data != null && LoginData.Data.Social != null, "ConnectionManager: Trying to connect without the URL set");
            _socket.Urls = LoginData.Data.Social.WebSocketUrls;

            return Reconnect();
        }

        public WAMPConnection.StartRequest Reconnect()
        {
            if(IsSocketConnected || IsSocketConnecting)
            {
                return null;
            }

            _active = true;
            SchedulePing();
            return _connection.Start(() => SendHello());
        }

        void OnWillGoBackground()
        {
            _socket.OnWillGoBackground();
        }

        void OnWasOnBackground()
        {
            _socket.OnWasOnBackground();
        }

        void SendPendingRequests()
        {
            while(_pendingRequests.Count > 0)
            {
                var request = _pendingRequests.Dequeue();

                switch(request.Type)
                {
                case RequestWrapper.RequestType.Publish:
                    _connection.SendPublish((PublishRequest)request.Request);
                    break;
                case RequestWrapper.RequestType.Call:
                    _connection.SendCall((CallRequest)request.Request);
                    break;
                default:
                    throw new Exception("ConnectionManager cannot store requests of type: " + request.Type);
                }
            }
        }

        public void Update()
        {
            if(IsSocketConnected && !IsSocketInStandby && _joined)
            {
                SendPendingRequests();
            }
        }

        public void Disconnect()
        {
            if(IsSocketConnected)
            {
                _connection.Leave(null, "Disconnect requested");
            }
            else if(IsSocketConnecting)
            {
                _connection.AbortJoining();
            }

            _active = false;
            _joined = false;
            UnschedulePing();
        }

        void SetConnectedState()
        {
            _joined = true;
            if(OnConnected != null)
            {
                OnConnected();
            }
        }

        void SetClosedState()
        {
            _joined = false;
            if(OnClosed != null)
            {
                OnClosed();
            }
        }

        void SchedulePing()
        {
            if(_scheduler == null)
            {
                Log.e("Failed to schedule ping actions. Scheduler instance is not available.");
                return;
            }

            if(_pingUpdate == null)
            {
                _pingUpdate = new ScheduledAction(_scheduler, () => {
                    if(IsSocketConnected)
                    {
                        _socket.Ping();
                    }
                });
            }

            if(_reconnectUpdate == null)
            {
                _reconnectUpdate = new ScheduledAction(_scheduler, () => Reconnect());
            }

            _pingUpdate.Start(_config.PingInterval);
            _reconnectUpdate.Start(_config.ReconnectInterval);
        }

        void UnschedulePing()
        {
            if(_pingUpdate != null)
            {
                _pingUpdate.Stop();
            }
            if(_reconnectUpdate != null)
            {
                _reconnectUpdate.Stop();
            }
        }

        void ProcessNotificationServices(AttrDic dic)
        {
            ProcessNotificationTopics(dic);
            ProcessPendingNotifications(dic);
        }

        void ProcessNotificationTopics(AttrDic dic)
        {
            var topicsList = dic.Get(ConnectionManager.TopicsKey).AsList;
            for(int i = 0; i < topicsList.Count; ++i)
            {
                var topicDic = topicsList[i].AsDic;
                var subscriptionId = topicDic.GetValue(ConnectionManager.SubscriptionIdTopicKey).ToLong();

                _connection.AutoSubscribe(new Subscription(subscriptionId, NotificationTopicName), 
                    (args, kwargs) => OnNotificationMessageReceived(NotificationTopicType, args, kwargs));
            }
        }

        void ProcessPendingNotifications(AttrDic dic)
        {
            var pendingDic = dic.Get(ConnectionManager.PendingKey).AsDic;
            if(pendingDic.Count == 0)
            {
                return;
            }

            if(OnPendingNotification != null)
            {
                for(int i = 0; i < pendingDic.Count; ++i)
                {
                    var notif = pendingDic.ElementAt(i).Value.AsDic;
                    var codeType = notif.GetValue(ConnectionManager.NotificationTypeKey).ToInt();
                    var payloadDic = notif.Get(ConnectionManager.NotificationPayloadKey).AsDic;

                    OnPendingNotification(codeType, NotificationTopicType, payloadDic);
                }
            }
        }

        public PublishRequest Publish(string topic, AttrList args, AttrDic kwargs, OnPublished onComplete)
        {
            DebugUtils.Assert(_active, "Connect the ConnectionManager before attempting to send a publish");
            var request = _connection.CreatePublish(topic, args, kwargs, onComplete != null, onComplete);
            EnqueuePendingRequest(request, RequestWrapper.RequestType.Publish);
            return request;
        }

        public CallRequest Call(string procedure, AttrList args, AttrDic kwargs, HandlerCall onResult)
        {
            DebugUtils.Assert(_active, "Connect the ConnectionManager before attempting to send a call");
            var request = _connection.CreateCall(procedure, args, kwargs, (err, iargs, ikwargs) => OnRPCFinished(iargs, ikwargs, onResult, err));
            EnqueuePendingRequest(request, RequestWrapper.RequestType.Call);
            return request;
        }

        void EnqueuePendingRequest(WAMPRequest request, RequestWrapper.RequestType type)
        {
            if(request == null)
            {
                //Do not enqueue the request in _pendingRequests because it is not a valid request
                return;
            }
            _pendingRequests.Enqueue(new RequestWrapper {
                Type = type,
                Request = request,
            });
        }

        void OnConnectionError(Error err)
        {
            if(OnError != null)
            {
                OnError(err);
            }
        }

        void OnJoined(Error err, long sessionId, AttrDic dic)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                return;
            }

            var servicesDic = dic.Get(ServicesKey).AsDic;

            if(servicesDic.ContainsKey(NotificationsServiceKey))
            {
                ProcessNotificationServices(servicesDic.Get(NotificationsServiceKey).AsDic);
            }

            if(OnProcessServices != null)
            {
                OnProcessServices(servicesDic);
            }

            SetConnectedState();
        }

        void OnNotificationMessageReceived(string topic, AttrList listParams, AttrDic dicParams)
        {
            if(OnNotificationReceived != null)
            {
                int type = dicParams.GetValue(NotificationTypeKey).ToInt();
                OnNotificationReceived(type, topic, dicParams);
            }
        }

        void OnRPCFinished(AttrList iargs, AttrDic ikwargs, HandlerCall onResult, Error err)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                if(OnRPCError != null)
                {
                    OnRPCError(err);
                }
            }

            if(onResult != null)
            {
                onResult(err, iargs, ikwargs);
            }
        }

        WAMPConnection.JoinRequest SendHello()
        {
            // Use the ForcedUser if defined. Otherwise, collect info from current user.
            var data = ForcedUser ?? new UserData(LoginData);

            var dicDetails = new AttrDic();
            dicDetails.SetValue("user_id", (long)data.UserId);
            dicDetails.SetValue("security_token", data.SecurityToken);

            #if ADMIN_PANEL
            dicDetails.SetValue("privileged_token", data.PrivilegeToken);
            #endif

            dicDetails.SetValue("device_uid", DeviceInfo.Uid);
            dicDetails.SetValue("country", DeviceInfo.AppInfo.Country);
            dicDetails.SetValue("platform", DeviceInfo.Platform);
            dicDetails.SetValue("language", Localization.Language);

            return _connection.Join(string.Empty, dicDetails, OnJoined);
        }

        #region INetworkClientDelegate implementation

        public void OnClientConnected()
        {
            if(OnConnectionStablished != null)
            {
                OnConnectionStablished();
            }
        }

        public void OnClientDisconnected()
        {
            SetClosedState();
        }

        public void OnMessageReceived(NetworkMessageData data)
        {

        }

        public void OnNetworkError(Error err)
        {
            OnConnectionError(err);
        }

        #endregion
    }
}
