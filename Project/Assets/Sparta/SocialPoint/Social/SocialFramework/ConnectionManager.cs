using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.Locale;
using SocialPoint.Utils;
using SocialPoint.Login;

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


    public class ConnectionManager : IDisposable, IUpdateable
    {
        const string NotificationTopicType = "notification";

        public float PingInterval = 10.0f;

        // Client-only error code. It is set on timeout errors.
        const int TimeoutErrorCode = 9000;

        const string TimeoutErrorTag = "timeout_error";

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

        public delegate void NotificationReceivedDelegate(int type, string topic, AttrDic dictParams);

        enum ConnectionState
        {
            Disconnected,
            Connecting,
            Connected
        }

        ConnectionState _state = ConnectionState.Disconnected;

        public event Action OnConnected;
        public event Action OnClosed;
        public event Action<AttrDic> OnProcessServices;
        public event Action<bool> OnUpdatedConnectivity;
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

        public AlliancesManager AllianceManager;

        public ILoginData LoginData;
        public IDeviceInfo DeviceInfo;

        ConnectionManagerConfig _config;

        public ConnectionManagerConfig Config
        {
            get
            {
                return _config;
            }
            set
            {
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
                    _appEvents.GameWillRestart.Remove(Disconnect);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    
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

                // TODO Set connection debug mode
            }
        }

        public ConnectionManager()
        {
            // TODO Create and configure websocket and wamp connection
        }

        public void Dispose()
        {
            Disconnect();
            if(_appEvents != null)
            {   
                _appEvents.GameWillRestart.Remove(Disconnect);
            }
        }

        public void AutosubscribeToTopic(string topic, WAMP.Subscription subscription)
        {
            // TODO Wamp autosubscribe
        }

        public void Connect(string[] socketUrls)
        {
            // TODO Configure websocket
            Reconnect();
        }

        public void Reconnect()
        {
            if(_state != ConnectionState.Disconnected /*|| !_connection*/)
            {
                return;
            }

            _state = ConnectionState.Connecting;

            // TODO 
            /*
            _connection.Start () => {
                SendHello();
                SchedulePing();
            }
            */
        }

        void RestartConnection()
        {
            UnschedulePing();
            ResetState();

            Reconnect();
        }

        public void Disconnect()
        {
            // TODO Disconnect wamp

            _chatManager.UnregisterAll();
            UnschedulePing();
        }

        void ResetState()
        {
            _state = ConnectionState.Disconnected;
            _chatManager.ClearAllSubscriptions();
        }

        void SchedulePing()
        {
            _scheduler.AddFixed(this, PingInterval);
        }

        void UnschedulePing()
        {
            _scheduler.Remove(this);
            // TODO ScheduledActions
        }

        public void Update()
        {
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
                //var topicDic = topicsList[i].AsDic;
                //var subscriptionId = topicDic.GetValue(ConnectionManager.SubscriptionIdTopicKey).ToLong();

                // TODO WAMP Autosubscribe to subscriptionId, "notifications"
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

                OnPendingNotification(codeType, NotificationTopicType, payloadDic);
            }
        }

        public void Publish(string topic, AttrList args, AttrDic kwargs, Action onComplete)
        {
            //bool askForConfirmation = onComplete != null;
            // WAMP Publish
        }

        public void Call(string procedure, AttrList args, AttrDic kwargs, Action onResult)
        {
            // TODO WAMP Call
        }

        void OnConnectionStateChanged(/*TODO websocket */ConnectionState state)
        {
            switch(state)
            {
            case ConnectionState.Disconnected:
                ResetState();
                OnClosed();
                OnUpdatedConnectivity(false);
                break;
            case ConnectionState.Connecting: // Closing
                ResetState();
                break;
            case ConnectionState.Connected:
                OnUpdatedConnectivity(true);
                break;
            }
        }

        void OnConnectionError()
        {
            
        }

        void OnJoined(long sessionId, AttrDic dic, Error err)
        {
            if(Error.IsNullOrEmpty(err))
            {
                _state = ConnectionState.Disconnected;
                return;
            }

            var servicesDic = dic.Get(ServicesKey).AsDic;

            if(servicesDic.ContainsKey(ChatServiceKey))
            {
                _chatManager.ProcessChatServices(servicesDic.Get(ChatServiceKey).AsDic);
            }

            if(servicesDic.ContainsKey(NotificationsServiceKey))
            {
                ProcessNotificationServices(servicesDic.Get(NotificationsServiceKey).AsDic);
            }

            OnProcessServices(servicesDic);
            _state = ConnectionState.Connected;
            OnConnected();
        }

        void OnNotificationMessageReceived(string topic, AttrList listParams, AttrDic dicParams)
        {
            int type = dicParams.GetValue(NotificationTypeKey).ToInt();
            if(type == 0)
            {
                type = dicParams.GetValue(NotificationCapitalTypeKey).ToInt();
            }

            OnNotificationReceived(type, topic, dicParams);
        }

        void OnRPCFinished(AttrList iargs, AttrDic ikwargs, Action onResult, int rpcId, Error err)
        {
            if(!Error.IsNullOrEmpty(err))
            {
                OnRPCError(err);

                if(err.Code == /*TODO*/1)
                {
                    if(onResult != null)
                    {
                        onResult();//TODO(err, iargs, ikwargs);
                    }
                }
                RestartConnection();
                return;
            }

            if(onResult != null)
            {
                onResult();//TODO(err, iargs, ikwargs);
            }
        }

        void SendHello()
        {
            var dicDetails = new AttrDic();
            dicDetails.SetValue("user_id", LoginData.UserId);
            //dicDetails.SetValue("security_token", LoginData.s); //security token?
            #if ADMIN_PANEL
            //dicDetails.SetValue("privileged_token", LoginData.
            #endif

            // TODO Add more children

            // TODO _connection.Join
        }
    }
}
