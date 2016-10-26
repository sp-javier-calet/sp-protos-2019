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

        int Members { get; set; }

        bool Subscribed { get; set; }

        bool IsAllianceChat { get; }

        void ParseInitialInfo(AttrDic dic);

        void AddNotificationMessage(AttrDic dic);

        void SendDebugMessage(string text);
    }

    public class ChatRoom<MessageType> : IChatRoom 
        where MessageType : class, IChatMessage, new()
    {
        readonly FactoryChatMessages<MessageType> _factory;

        readonly ChatMessageList<MessageType> _messages;

        readonly ConnectionManager _connection;

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

        public bool Subscribed { get; set; }

        public bool IsAllianceChat
        { 
            get
            {
                return Type == "alliance"; 
            }
        }

        public ChatRoom(string type, ConnectionManager connection)
        {
            Type = type;
            _connection = connection;
            _factory = new FactoryChatMessages<MessageType>();
            _messages = new ChatMessageList<MessageType>();
        }

        public void ParseInitialInfo(AttrDic dic)
        {
            if(dic.ContainsKey(ConnectionManager.HistoryTopicKey))
            {
                var list = dic.Get(ConnectionManager.HistoryTopicKey).AsList;
                AddHistoricMessages(list);
            }

            Id = dic.GetValue(ConnectionManager.IdTopicKey).ToString();
            Name = dic.GetValue(ConnectionManager.NameTopicKey).ToString();
            Members = dic.GetValue(ConnectionManager.TopicMembersKey).ToInt();
        }

        public void AddNotificationMessage(AttrDic dic)
        {
            var messages = _factory.ParseMessage(dic);
            for(int i = 0; i < messages.Length; ++i)
            {
                _messages.Add(messages[i]);
            }
        }

        public void AddHistoricMessages(AttrList list)
        {
            var history = new List<MessageType>();
            for(int i = 0; i < list.Count; ++i)
            {
                var msgs = _factory.ParseMessage(list[i].AsDic);
                history.AddRange(msgs);
            }

            var message = _factory.CreateLocalizedWarning("socialFramework.ChatWarning");
            history.Add(message);

            _messages.SetHistory(history);
        }

        public void SendMessage(MessageType message)
        {
            SetupMessage(message);

            var messageInfo = _factory.SerializeMessage(message);

            var args = new AttrDic();
            args.SetValue(ConnectionManager.NotificationTypeKey, NotificationTypeCode.TextMessage);
            args.Set(ConnectionManager.ChatMessageInfoKey, messageInfo);

            var idx = _messages.Add(message);
            _connection.Publish(Id, null, args, (err, pub) => OnMessageSent(idx, message.Uuid));
        }

        void SetupMessage(MessageType message)
        {
            message.Uuid = RandomUtils.GetUuid();

            var player = _connection.PlayerData;
            message.PlayerId = player.Id;
            message.PlayerName = player.Name;
            message.PlayerLevel = player.Level;
            message.Timestamp = TimeUtils.Timestamp;

            if(_connection.AlliancesManager != null)
            {
                var member = _connection.AlliancesManager.Player;
                message.HasAlliance = member.IsInAlliance;
                message.AllianceName = member.Name;
                message.AllianceId = member.Id;
                message.AllianceAvatarId = member.AvatarId;
                message.RankInAlliance = member.Rank;
            }

            message.IsSending = true;
        }

        public void SendDebugMessage(string text)
        {
            SendMessage(_factory.Create(text));
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
