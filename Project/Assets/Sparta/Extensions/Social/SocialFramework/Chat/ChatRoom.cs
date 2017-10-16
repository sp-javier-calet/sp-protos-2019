using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Connection;
using SocialPoint.Locale;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public interface IChatRoom
    {
        IMessageList Messages { get; }

        HashSet<int> FilteredMessageTypes { get; }

        string Type { get; }

        int MemberCount { get; }

        List<string> Members { get; }

        bool Subscribed { get; }

        ChatManager ChatManager { set; }

        Localization Localization { set; }

        void ParseInitialInfo(AttrDic dic);

        void AddNotificationMessage(int type, AttrDic dic);

        void SendDebugMessage(string text);
    }

    public class ChatRoom<MessageType> : IChatRoom 
        where MessageType : class, IChatMessage, new()
    {
        readonly FactoryChatMessages<MessageType> _factory;

        readonly ChatMessageList<MessageType> _messages;

        public ChatManager ChatManager { private get; set; }

        public event Action<int> OnMembersCountChanged;
        public event Action<string> OnMemberConnected;
        public event Action<string> OnMemberDisconnected;

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

        #region Factory methods

        public Func<AttrDic, MessageType[]> ParseUnknownNotifications
        {
            set
            {
                _factory.ParseUnknownNotifications = value;
            }
        }

        public Action<MessageType, AttrDic> ParseExtraInfo
        {
            set
            {
                _factory.ParseExtraInfo = value;
            }
        }

        public Action<MessageType, AttrDic> SerializeExtraInfo
        {
            set
            {
                _factory.SerializeExtraInfo = value;
            }
        }

        public Localization Localization
        {
            set
            {
                _factory.Localization = value;
            }
        }

        public IRankManager RankManager
        {
            set
            {
                _factory.RankManager = value;
            }
        }

        #endregion

        int _memberCount;

        public int MemberCount
        { 
            get
            {
                return _memberCount;
            }
            set
            {
                _memberCount = value;
                if(OnMembersCountChanged != null)
                {
                    OnMembersCountChanged(_memberCount);
                }
            }
        }

        List<string> _members;

        public List<string> Members
        {
            get
            {
                return _members;
            }
        }

        public IMessageList Messages
        { 
            get
            {
                return CustomMessages;
            }
        }

        public ChatMessageList<MessageType> CustomMessages
        { 
            get
            {
                return _messages;
            }
        }

        public HashSet<int> FilteredMessageTypes
        {
            get
            {
                return _factory.FilteredMessageTypes;
            }
        }

        public bool Subscribed
        {
            get
            {
                return ChatManager.IsSubscribedToChat(this);
            }
        }

        public ChatRoom(string type)
        {
            Type = type;
            _factory = new FactoryChatMessages<MessageType>();
            _messages = new ChatMessageList<MessageType>();
            _members = new List<string>();
        }

        public void ParseInitialInfo(AttrDic dic)
        {
            if(dic.ContainsKey(ChatManager.HistoryTopicKey))
            {
                var list = dic.Get(ChatManager.HistoryTopicKey).AsList;
                SetHistory(list);
            }

            Id = dic.GetValue(ChatManager.IdTopicKey).ToString();
            Name = dic.GetValue(ChatManager.NameTopicKey).ToString();
            CheckMemberConnected(dic.Get(ChatManager.TopicMembersKey).AsList);
        }

        public void AddNotificationMessage(int type, AttrDic dic)
        {
            ProcessRoomNotifications(type, dic);

            var messages = _factory.ParseMessage(type, dic);
            for(int i = 0; i < messages.Length; ++i)
            {
                _messages.Add(messages[i]);
            }
        }

        void ProcessRoomNotifications(int type, AttrDic dic)
        {
            if(type == NotificationType.BroadcastAllianceOnlineMember)
            {
                AttrList list = dic.Get(ChatManager.TopicMembersKey).AsList;
                CheckMemberConnected(list);
            }
        }

        void SetHistory(AttrList list)
        {
            var history = new List<MessageType>();
            for(int i = 0; i < list.Count; ++i)
            {
                var dic = list[i].AsDic;
                var type = dic.GetValue(ConnectionManager.NotificationTypeKey).ToInt();
                var msgs = _factory.ParseMessage(type, dic);
                history.AddRange(msgs);
            }

            var message = _factory.CreateLocalizedWarning(NotificationType.ChatWarning, SocialFrameworkStrings.ChatWarningKey);
            if(message != null)
            {
                history.Add(message);
            }

            _messages.SetHistory(history);
        }

        public void SendMessage(MessageType message)
        {
            SetupMessage(message);

            var messageInfo = _factory.SerializeMessage(message);

            var args = new AttrDic();
            args.SetValue(ConnectionManager.NotificationTypeKey, NotificationType.TextMessage);
            args.Set(ChatManager.ChatMessageInfoKey, messageInfo);

            var idx = _messages.Add(message);
            ChatManager.Connection.Publish(Id, null, args, (err, pub) => OnMessageSent(idx, message.Uuid));
        }

        void SetupMessage(MessageType message)
        {
            message.Uuid = RandomUtils.GetUuid();

            var player = ChatManager.Connection.PlayerData;
            var data = new MessageData();
            data.PlayerId = player.Id;
            data.PlayerName = player.Name;
            data.PlayerLevel = player.Level;

            if(ChatManager.SocialManager.LocalPlayer.HasComponent<AlliancePlayerBasic>())
            {
                var member = ChatManager.SocialManager.LocalPlayer.GetComponent<AlliancePlayerBasic>();
                data.AllianceName = member.Name;
                data.AllianceId = member.Id;
                data.AllianceAvatarId = member.Avatar;
                data.RankInAlliance = member.Rank;
            }

            message.MessageData = data;
            message.Timestamp = TimeUtils.Timestamp;
            message.IsSending = true;
        }

        public void SendDebugMessage(string text)
        {
            var message = _factory.Create(NotificationType.TextMessage, text);
            if(message != null)
            {
                SendMessage(message);
            }
        }

        void OnMessageSent(int index, string originalUuid)
        {
            Action<IChatMessage> editCallback = msg => {
                if(msg.Uuid == originalUuid)
                {
                    msg.IsSending = false;
                }
            };
            _messages.Edit(index, editCallback);
        }

        void CheckMemberConnected(AttrList list)
        {
            List<string> newList = new List<string>();
            for(int i = 0; i < list.Count; i++)
            {
                newList.Add(list[i].AsValue.ToString());

                if(!_members.Contains(newList[i])) //Player connected
                {
                    MemberConnected(newList[i]);
                }
            }

            for(int i = 0; i < _members.Count; i++)
            {
                if(!newList.Contains(_members[i])) //Player disconnected
                {
                    MemberDisonnected(_members[i]);
                }
            }

            _members = newList;
            MemberCount = list.Count;
        }

        void MemberConnected(string id)
        {
            if(OnMemberConnected != null)
            {
                OnMemberConnected(id);
            }
        }

        void MemberDisonnected(string id)
        {
            if(OnMemberDisconnected != null)
            {
                OnMemberDisconnected(id);
            }
        }
    }
}
