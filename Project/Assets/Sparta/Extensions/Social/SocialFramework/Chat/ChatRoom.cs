using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public interface IChatRoom
    {
        IMessageList Messages { get; }

        string Type { get; }

        int Members { get; }

        bool Subscribed { get; }

        ChatManager ChatManager { set; }

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

        public event Action<int> OnMembersChanged;

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

        int _members;

        public int Members
        { 
            get
            {
                return _members;
            }
            set
            {
                _members = value;
                if(OnMembersChanged != null)
                {
                    OnMembersChanged(_members);
                }
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
        }

        public void ParseInitialInfo(AttrDic dic)
        {
            if(dic.ContainsKey(ConnectionManager.HistoryTopicKey))
            {
                var list = dic.Get(ConnectionManager.HistoryTopicKey).AsList;
                SetHistory(list);
            }

            Id = dic.GetValue(ConnectionManager.IdTopicKey).ToString();
            Name = dic.GetValue(ConnectionManager.NameTopicKey).ToString();
            Members = dic.GetValue(ConnectionManager.TopicMembersKey).ToInt();
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
                Members = dic.GetValue(ConnectionManager.TopicMembersKey).ToInt();
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
            history.Add(message);

            _messages.SetHistory(history);
        }

        public void SendMessage(MessageType message)
        {
            SetupMessage(message);

            var messageInfo = _factory.SerializeMessage(message);

            var args = new AttrDic();
            args.SetValue(ConnectionManager.NotificationTypeKey, NotificationType.TextMessage);
            args.Set(ConnectionManager.ChatMessageInfoKey, messageInfo);

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

            if(ChatManager.Connection.AlliancesManager != null)
            {
                var member = ChatManager.Connection.AlliancesManager.AlliancePlayerInfo;
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
            SendMessage(_factory.Create(NotificationType.TextMessage, text));
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
    }
}
