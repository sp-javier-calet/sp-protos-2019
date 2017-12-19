using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.WAMP.Subscriber;
using SocialPoint.Utils;
using SocialPoint.Connection;

namespace SocialPoint.Social
{
    public sealed class ChatManager : IDisposable
    {
        #region Attr keys

        const string ChatServiceKey = "chat";
        const string AllianceRoomType = "alliance";

        public const string IdTopicKey = "id";
        public const string TypeTopicKey = "type";
        public const string NameTopicKey = "name";
        public const string HistoryTopicKey = "history";
        public const string TopicMembersKey = "topic_members";
        public const string ChatMessageInfoKey = "message_info";

        const string ChatBanEndTimestampKey = "banEndTimestamp";
        const string ChatBanEndTimeKey = "ban_end_time";
        const string ReportsKey = "reports";
        const string UserIdKey = "user_id";

        #endregion

        #region RPC methods

        const string ChatReportUserMethod = "chat.report.user";

        #endregion

        public event Action<long> OnChatBanReceived;

        readonly ConnectionManager _connection;

        public ConnectionManager Connection
        {
            get
            {
                return _connection;
            }
        }

        readonly SocialManager _socialManager;

        public SocialManager SocialManager
        {
            get
            {
                return _socialManager;
            }
        }

        readonly List<ChatReport> _reports;

        public ReadOnlyCollection<ChatReport> Reports{ get { return _reports.AsReadOnly(); } }


        public const long DefaultReportUserCooldown = 60 * 60 * 24;

        public long ReportUserCooldown{ get; set; }

        readonly Dictionary<string, IChatRoom> _chatRooms;

        readonly  Dictionary<IChatRoom, Subscription> _chatSubscriptions;

        public IChatRoom AllianceRoom { get; private set; }

        public long ChatBanEndTimestamp { get; private set; }

        public ChatManager(ConnectionManager connection, SocialManager socialManager)
        {
            _chatRooms = new Dictionary<string, IChatRoom>();
            _chatSubscriptions = new Dictionary<IChatRoom, Subscription>();

            _socialManager = socialManager;
            _connection = connection;
            _connection.OnClosed += OnConnectionClosed;
            _connection.OnNotificationReceived += ProcessNotificationMessage;
            _connection.OnProcessServices += ProcessChatServices;

            _reports = new List<ChatReport>();

            ReportUserCooldown = DefaultReportUserCooldown;
        }

        public void Dispose()
        {
            UnregisterAll();
            _connection.OnClosed -= OnConnectionClosed;
            _connection.OnNotificationReceived -= ProcessNotificationMessage;
            _connection.OnProcessServices -= ProcessChatServices;
        }

        public IEnumerator<IChatRoom> GetRooms()
        {
            return _chatRooms.Values.GetEnumerator();
        }

        public void DeleteSubscription(IChatRoom room)
        {
            if(IsSubscribedToChat(room))
            {
                _chatSubscriptions.Remove(room);
            }
        }

        public void Register(List<IChatRoom> rooms)
        {
            for(var i = 0; i < rooms.Count; ++i)
            {
                Register(rooms[i]);
            }
        }

        public void Register(IChatRoom room)
        {
            IChatRoom existing;
            if(_chatRooms.TryGetValue(room.Type, out existing) && existing != room)
            {
                throw new Exception("A Chat Room is already registered for the topic type " + room.Type);
            }

            _chatRooms.Add(room.Type, room);

            if(room.Type == AllianceRoomType)
            {
                AllianceRoom = room;
            }
        }

        public void Unregister(string type)
        {
            IChatRoom room;
            if(_chatRooms.TryGetValue(type, out room))
            {
                _chatRooms.Remove(type);
                DeleteSubscription(room);

                if(room == AllianceRoom)
                {
                    AllianceRoom = null;
                }
            }
        }

        public void UnregisterAll()
        {
            ClearAllSubscriptions();
            AllianceRoom = null;
            _chatRooms.Clear();
        }

        public bool IsSubscribedToChat(IChatRoom room)
        {
            return _chatSubscriptions.ContainsKey(room);
        }

        public bool IsAllianceChat(IChatRoom room)
        {
            return AllianceRoom == room; 
        }

        public void ProcessChatServices(AttrDic servicesDic)
        {
            var chatServiceDic = servicesDic.Get(ChatServiceKey).AsDic;

            var topicsList = chatServiceDic.Get(ConnectionManager.TopicsKey).AsList;
            for(int i = 0; i < topicsList.Count; ++i)
            {
                var topicDic = topicsList[i].AsDic;
                ProcessChatTopic(topicDic);
            }

            ChatBanEndTimestamp = 0;
            if(chatServiceDic.ContainsKey(ChatBanEndTimestampKey))
            {
                ChatBanEndTimestamp = chatServiceDic.GetValue(ChatBanEndTimestampKey).ToLong();
            }

            if(chatServiceDic.ContainsKey(ReportsKey))
            {
                ProcessReports(chatServiceDic.GetValue(ReportsKey).AsList);
            }
        }

        public void ClearAllSubscriptions()
        {
            _chatSubscriptions.Clear();
            _reports.Clear();
        }

        void ProcessNotificationMessage(int type, string topic, AttrDic dic)
        {
            // Global notifications
            switch(type)
            {
            case NotificationType.NotificationUserChatBan:
                SetChatBan(dic);
                break;
            }

            // Room notifications
            IChatRoom room;
            if(_chatRooms.TryGetValue(topic, out room))
            {
                room.AddNotificationMessage(type, dic);
            }
        }

        void SetChatBan(AttrDic dic)
        {
            ChatBanEndTimestamp = dic.GetValue(ChatBanEndTimeKey).ToLong();
            OnChatBanReceived(ChatBanEndTimestamp);
        }

        public void ReportChatMessage(BaseChatMessage message, AttrDic extraData)
        {
            var report = new ChatReport(message, extraData);
            _reports.Add(report);

            var dicData = report.Serialize();
            dicData.SetValue(UserIdKey, _socialManager.LocalPlayer.Uid);

            _connection.Call(ChatReportUserMethod, null, dicData, null);
        }

        void ProcessReports(AttrList listReports)
        {
            var itr = listReports.GetEnumerator();
            while(itr.MoveNext())
            {
                _reports.Add(ChatReport.Parse(itr.Current.AsDic));
            }
            itr.Dispose();
        }

        public bool CanReportUser(string userId)
        {
            var vigentReport = _reports.Find(report => (report.ReportedUid == userId) && (TimeUtils.Timestamp < report.Ts + ReportUserCooldown));
            return (vigentReport == null);
        }

        void ProcessChatTopic(AttrDic dic)
        {
            var topic = dic.GetValue(TypeTopicKey).ToString();

            IChatRoom room;
            if(!_chatRooms.TryGetValue(topic, out room))
            {
                DebugUtils.Assert(false, "There is no registered room for topic " + topic);
                return;
            }

            var subscriptionId = dic.GetValue(ConnectionManager.SubscriptionIdTopicKey).ToLong();
            var topicName = dic.GetValue(IdTopicKey).ToString();
            var subscription = new Subscription(subscriptionId, topicName);
            _chatSubscriptions.Add(room, subscription);

            _connection.AutosubscribeToTopic(topic, subscription);
            room.ParseInitialInfo(dic);
        }

        void OnConnectionClosed()
        {
            ClearAllSubscriptions();
        }
    }
}
