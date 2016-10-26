using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public sealed class ChatManager : IDisposable
    {
        const string AllianceRoomType = "alliance";

        public event Action<long> OnChatBanReceived;

        readonly ConnectionManager _connection;

        public ConnectionManager Connection
        {
            get
            {
                return _connection;
            }
        }

        readonly Dictionary<string, IChatRoom> _chatRooms;

        readonly  Dictionary<IChatRoom, WAMPConnection.Subscription> _chatSubscriptions;

        public IChatRoom AllianceRoom { get; private set; }

        public long ChatBanEndTimestamp { get; private set; }

        public ChatManager(ConnectionManager connection)
        {
            _chatRooms = new Dictionary<string, IChatRoom>();
            _chatSubscriptions = new Dictionary<IChatRoom, WAMPConnection.Subscription>();

            _connection = connection;
            _connection.ChatManager = this;
            _connection.OnNotificationReceived += ProcessNotificationMessage;
        }

        public void Dispose()
        {
            UnregisterAll();
            _connection.OnNotificationReceived -= ProcessNotificationMessage;
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
                ClearSubscription(room);

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

        public void ProcessChatServices(AttrDic dic)
        {
            var topicsList = dic.Get(ConnectionManager.TopicsKey).AsList;
            for(int i = 0; i < topicsList.Count; ++i)
            {
                var topicDic = topicsList[i].AsDic;
                var topic = topicDic.GetValue(ConnectionManager.TypeTopicKey).ToString();
                ProcessChatTopic(topic, topicDic);
            }

            ChatBanEndTimestamp = 0;
            if(dic.ContainsKey("banEndTimestamp"))
            {
                ChatBanEndTimestamp = dic.GetValue("banEndTimestamp").ToLong();
            }
        }

        public void ClearSubscription(IChatRoom room)
        {
            if(IsSubscribedToChat(room))
            {
                _chatSubscriptions.Remove(room);
            }
        }

        public void ClearAllSubscriptions()
        {
            _chatSubscriptions.Clear();
        }

        void ProcessNotificationMessage(int type, string topic, AttrDic dic)
        {
            // Global notifications
            switch(type)
            {
            case NotificationTypeCode.NotificationUserChatBan:
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
            ChatBanEndTimestamp = dic.GetValue("ban_end_time").ToLong();
            OnChatBanReceived(ChatBanEndTimestamp);
        }

        void ProcessChatTopic(string topic, AttrDic dic)
        {
            IChatRoom room;
            if(!_chatRooms.TryGetValue(topic, out room))
            {
                DebugUtils.Assert(false, "There is no registered room for topic " + topic);
                return;
            }

            var subscriptionId = dic.GetValue(ConnectionManager.SubscriptionIdTopicKey).ToLong();
            var topicName = dic.GetValue(ConnectionManager.IdTopicKey).ToString();
            var subscription = new WAMPConnection.Subscription(subscriptionId, topicName);
            _chatSubscriptions.Add(room, subscription);

            _connection.AutosubscribeToTopic(topic, subscription);
            room.ParseInitialInfo(dic);
        }
    }
}
