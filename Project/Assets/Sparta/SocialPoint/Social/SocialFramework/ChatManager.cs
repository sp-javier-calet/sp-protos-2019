using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public static class WAMP
    {
        //
        public struct Subscription
        {
            public Subscription(ulong id, string name)
            {
            }
        }
        //
    }

    public class ChatManager : IDisposable
    {
        public event Action<long> OnChatBanReceived;

        ConnectionManager _connection;

        Dictionary<string, IChatRoom> _chatRooms;
        Dictionary<IChatRoom, WAMP.Subscription> _chatSubscriptions;
        IChatRoom AllianceRoom;

        public long ChatBanEndTimestamp { get; protected set; }


        public ChatManager(ConnectionManager connection)
        {
            _connection = connection;
            _connection.OnNotificationReceived += ProcessNotificationMessage;
        }

        public void Dispose()
        {
            UnregisterAll();
            _connection.OnNotificationReceived -= ProcessNotificationMessage;
        }

        public void DeleteSubscription(IChatRoom room)
        {
            if(IsSubscribedToChat(room))
            {
                _chatSubscriptions.Remove(room);
                room.Subscribed = false;
            }
        }

        public void Register(IChatRoom room)
        {
            _chatRooms.Add(room.Type, room);
        }

        public void Unregister(string type)
        {
            _chatRooms.Remove(type);
        }

        public void UnregisterAll()
        {
            AllianceRoom = null;
            ClearAllSubscriptions();
            _chatRooms.Clear();
        }

        public bool IsSubscribedToChat(IChatRoom room)
        {
            return _chatSubscriptions.ContainsKey(room);
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

        public void ClearAllSubscriptions()
        {
            var itr = _chatRooms.GetEnumerator();
            while(itr.MoveNext())
            {
                var kvp = itr.Current;
                kvp.Value.Subscribed = false;

            }
            itr.Dispose();
            _chatSubscriptions.Clear();
        }

        void ProcessNotificationMessage(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationTypeCode.NotificationUserChatBan:
                SetChatBan(dic);
                break;
            case NotificationTypeCode.BroadcastAllianceOnlineMember:
                SetRoomMembersOnline(topic, dic);
                break;
            }

            IChatRoom room;
            if(_chatRooms.TryGetValue(topic, out room))
            {
                room.AddNotificationMessage(dic);
            }
        }

        void SetChatBan(AttrDic dic)
        {
            ChatBanEndTimestamp = dic.GetValue("ban_end_time").ToLong();
            OnChatBanReceived(ChatBanEndTimestamp);
        }

        void SetRoomMembersOnline(string topic, AttrDic dic)
        {
            IChatRoom room;
            if(_chatRooms.TryGetValue(topic, out room))
            {
                room.Members = dic.GetValue(ConnectionManager.TopicMembersKey).ToInt();
            }
        }

        void ProcessChatTopic(string topic, AttrDic dic)
        {
            IChatRoom room;
            if(_chatRooms.TryGetValue(topic, out room))
            {
                
            }
            else
            {
                throw new Exception("There is no registered room for topic " + topic);
            }

            ulong subscriptionId = dic.GetValue(ConnectionManager.SubscriptionIdTopicKey).ToValue<ulong>();
            var topicName = dic.GetValue(ConnectionManager.IdTopicKey).ToString();
            var subscription = new WAMP.Subscription(subscriptionId, topicName);
            _chatSubscriptions.Add(room, subscription);
            room.Subscribed = true;

            _connection.AutosubscribeToTopic(topic, subscription);
            room.ParseInitialInfo(dic);

            if(room.IsAllianceChat)
            {
                AllianceRoom = room;
            }
        }
    }
}
