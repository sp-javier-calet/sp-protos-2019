using System;
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

namespace SocialPoint.Social
{
    public class ConnectionManagerConfig
    {
        public float PingInterval = 10.0f;
        public float ReconnectInterval = 10.0f;

        public int RPCRetries = 1;
        public float RPCTimeout = 1.0f;
    }

    public static class NotificationTypeCode
    {
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
    }

    public class ConnectionManager : INetworkClientDelegate, IDisposable
    {
        #region Attr keys

        public const string ResultKey = "result";
        public const string ServicesKey = "services";
        public const string ChatServiceKey = "chat";
        public const string NotificationsServiceKey = "notification";
        public const string MatchmakingServicesKey = "matchmaking";
        public const string TeamWarKey = "team_war";
        public const string TopicsKey = "topics";
        public const string PendingKey = "pending";

        public const string IdTopicKey = "id";
        public const string SubscriptionIdTopicKey = "subscription_id";
        public const string TypeTopicKey = "type";
        public const string NameTopicKey = "name";
        public const string HistoryTopicKey = "history";
        public const string TopicMembersKey = "topic_members";
        public const string TopicTotalMembersKey = "total_members";

        public const string NotificationTypeKey = "type";
        public const string NotificationCapitalTypeKey = "Type";
        public const string NotificationPayloadKey = "payload";
        public const string ChatMessageInfoKey = "message_info";
        public const string NotificationIdKey = "notification_id";

        public const string ChatMessageUuidKey = "id";
        public const string ChatMessageUserIdKey = "uid";
        public const string ChatMessageUserNameKey = "uname";
        public const string ChatMessageTsKey = "ts";
        public const string ChatMessageTextKey = "msg";
        public const string ChatMessageLevelKey = "lvl";
        public const string ChatMessageAllyNameKey = "ally_name";
        public const string ChatMessageAllyIdKey = "ally_id";
        public const string ChatMessageAllyAvatarKey = "ally_avatar";
        public const string ChatMessageAllyRoleKey = "ally_role";

        #endregion

        const string NotificationTopicType = "notification";
        const string NotificationTopicName = "notifications";

        enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

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

        public delegate void NotificationReceivedDelegate(int type, string topic, AttrDic dictParams);

        public event Action OnConnected;
        public event Action OnClosed;
        public event Action<AttrDic> OnProcessServices;
        public event Action<bool> OnUpdatedConnectivity;
        public event Action<Error> OnError;
        public event Action<Error> OnRPCError;
        public event NotificationReceivedDelegate OnNotificationReceived;
        public event NotificationReceivedDelegate OnPendingNotification;

        ChatManager _chatManager;

        public ChatManager ChatManager
        {
            get
            {
                return _chatManager;
            }
            set
            {
                if(_chatManager != null && _chatManager != null && _chatManager != value)
                {
                    throw new Exception("ConnectionManager is already binded to a different instance of Chat Manager");

                }
                _chatManager = value;
            }
        }

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
                if(IsConnected)
                {
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

                    _appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
                    _appEvents.GameWillRestart.Remove(Disconnect);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    _appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
                    _appEvents.GameWillRestart.Add(0, Disconnect);
                }
            }
        }

        IUpdateScheduler _scheduler;

        public IUpdateScheduler Scheduler
        {
            set
            {
                _scheduler = value;
            }
        }

        public bool IsConnected
        {
            get
            {
                return _state == ConnectionState.Connected;
            }
        }

        public bool IsConnecting
        {
            get
            {
                return _state == ConnectionState.Connecting;
            }
        }

        public float RPCTimeout
        {
            get
            {
                return (Config.RPCRetries + 1) * Config.RPCTimeout;
            }
        }

        public string Url
        {
            get
            {
                // TODO Work with multiple Urls
                return _socket.Url;
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

        public AlliancesManager AlliancesManager { get; set; }

        public ILoginData LoginData { get; set; }

        public IPlayerData PlayerData { get; set; }

        public Localization Localization { get; set; }

        public IDeviceInfo DeviceInfo { get; set; }

        public UserData ForcedUser { get; set; }

        readonly WAMPConnection _connection;
        readonly IWebSocketClient _socket;

        ScheduledAction _pingUpdate;
        ScheduledAction _reconnectUpdate;
        ConnectionState _state;

        public ConnectionManager(IWebSocketClient client)
        {
            _socket = client;
            _socket.AddDelegate(this);
            _connection = new WAMPConnection(client);
            _config = new ConnectionManagerConfig();
            _state = ConnectionState.Disconnected;
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

        public void Connect()
        {
            Reconnect();
        }

        public void Reconnect()
        {
            if(_state != ConnectionState.Disconnected)
            {
                return;
            }

            _state = ConnectionState.Connecting;

            _connection.Start(() => {
                SendHello();
                SchedulePing();
            });
        }

        void RestartConnection()
        {
            UnschedulePing();
            ResetState();
            Reconnect();
        }

        void OnGameWasLoaded()
        {
            if(LoginData != null && LoginData.Data.Social != null)
            {
                var urls = LoginData.Data.Social.WebSocketUrls;
                _socket.Url = urls[0]; // TODO Set urls
            }
        }

        public void Disconnect()
        {
            if(IsConnected)
            {
                _connection.Leave(null, "Disconnect requested");
            }
            else if(IsConnecting)
            {
                _connection.AbortJoining();
            }

            if(_chatManager != null)
            {
                _chatManager.ClearAllSubscriptions();
            }

            UnschedulePing();
            _state = ConnectionState.Disconnected;
        }

        void ResetState()
        {
            _state = ConnectionState.Disconnected;
            if(_chatManager != null)
            {
                _chatManager.ClearAllSubscriptions();
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
                    if(IsConnected)
                    {
                        _socket.Ping();
                    }
                });
            }

            if(_reconnectUpdate == null)
            {
                _reconnectUpdate = new ScheduledAction(_scheduler, () => {
                    if(!IsConnected && !IsConnecting)
                    {
                        Reconnect();
                    }
                });
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

            for(int i = 0; i < pendingDic.Count; ++i)
            {
                var notif = pendingDic.ElementAt(i).Value.AsDic;
                var codeType = notif.GetValue(ConnectionManager.NotificationTypeKey).ToInt();

                var payloadDic = notif.Get(ConnectionManager.NotificationPayloadKey).AsDic;

                if(OnPendingNotification != null)
                {
                    OnPendingNotification(codeType, NotificationTopicType, payloadDic);
                }
            }
        }

        public void Publish(string topic, AttrList args, AttrDic kwargs, OnPublished onComplete)
        {
            _connection.Publish(topic, args, kwargs, onComplete != null, onComplete);
        }

        public void Call(string procedure, AttrList args, AttrDic kwargs, HandlerCall onResult)
        {
            _connection.Call(procedure, args, kwargs, (err, iargs, ikwargs) => OnRPCFinished(iargs, ikwargs, onResult, err));
        }

        void OnConnectionStateChanged(ConnectionState state)
        {
            switch(state)
            {
            case ConnectionState.Disconnected:
                ResetState();
                if(OnClosed != null)
                {
                    OnClosed();
                }
                if(OnUpdatedConnectivity != null)
                {
                    OnUpdatedConnectivity(false);
                }
                break;
            case ConnectionState.Connected:
                if(OnUpdatedConnectivity != null)
                {
                    OnUpdatedConnectivity(true);
                }
                break;
            }
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
                _state = ConnectionState.Disconnected;
                return;
            }

            var servicesDic = dic.Get(ServicesKey).AsDic;

            if(_chatManager != null && servicesDic.ContainsKey(ChatServiceKey))
            {
                _chatManager.ProcessChatServices(servicesDic.Get(ChatServiceKey).AsDic);
            }

            if(servicesDic.ContainsKey(NotificationsServiceKey))
            {
                ProcessNotificationServices(servicesDic.Get(NotificationsServiceKey).AsDic);
            }

            if(OnProcessServices != null)
            {
                OnProcessServices(servicesDic);
            }
            _state = ConnectionState.Connected;
            if(OnConnected != null)
            {
                OnConnected();
            }
        }

        void OnNotificationMessageReceived(string topic, AttrList listParams, AttrDic dicParams)
        {
            int type = dicParams.GetValue(NotificationTypeKey).ToInt();
            if(type == 0)
            {
                type = dicParams.GetValue(NotificationCapitalTypeKey).ToInt();
            }

            if(OnNotificationReceived != null)
            {
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

                if(err.Code == ErrorCodes.ConnectionClosed)
                {
                    if(onResult != null)
                    {
                        onResult(err, iargs, ikwargs);
                    }
                }
                RestartConnection();
                return;
            }

            if(onResult != null)
            {
                onResult(err, iargs, ikwargs);
            }
        }

        void SendHello()
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

            _connection.Join(string.Empty, dicDetails, OnJoined);
        }

        #region INetworkClientDelegate implementation

        public void OnClientConnected()
        {
            OnConnectionStateChanged(ConnectionState.Connected);
        }

        public void OnClientDisconnected()
        {
            OnConnectionStateChanged(ConnectionState.Disconnected);
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
