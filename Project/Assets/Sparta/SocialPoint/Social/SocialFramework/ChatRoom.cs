using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IChatRoom
    {
        IMessageList Messages { get; }

        string Type { get; }

        int Members { get; set; }

        bool Subscribed { get; set; }
        // TODO Accessible from ChatManager only

        bool IsAllianceChat { get; }

        void ParseInitialInfo(AttrDic dic);

        void AddNotificationMessage(AttrDic dic);
    }

    public class ChatRoom<MessageType> : IChatRoom 
        where MessageType : IChatMessage
    {
        FactoryChatMessages<MessageType> _factory;

        ChatMessageList<MessageType> _messages;

        readonly ConnectionManager _connection;

        public event Action<int> OnMembersChanged;

        public string Id { get; private set; }

        public string Name { get; private set; }

        public string Type { get; private set; }

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
                OnMembersChanged(_members);
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



        public ChatRoom(string name, ConnectionManager connection)
        {
            Name = name;
            _connection = connection;
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
            var messages = _factory.ParseMessages(dic);
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
                var msgs = _factory.ParseMessages(list[i].AsDic);
                history.AddRange(msgs);
            }

            var message = new MessageType(""); // TODO LocalizedString("socialFramework.ChatWarning")
            message.IsWarning = true;
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
            _connection.Publish(Id, null, args, () => OnMessageSent(idx, message.Uuid));
        }

        void SetupMessage(MessageType message)
        {
            /*
             message._uuid = RandomUtils::getUuid();
 +        message._playerId = GenericGameData::userId();
 +        message._playerName = GenericGameData::userName();
 +        message._timestamp = GenericGameData::currentTimestamp();
 +        message._playerLevel = GenericGameData::userLevel();
 +        message._isSending = true;
 +
 +        if(GameConfig::alliancesManager.enabled)
 +        {
 +            AlliancePlayerInfo* allyInfo = Services::get().getAlliancesManager().getAlliancePlayerInfo();
 +            message._hasAlliance = allyInfo->isInAlliance();
 +            message._allianceName = allyInfo->name;
 +            message._allianceId = allyInfo->id;
 +            message._allianceAvatarId = static_cast<int>(allyInfo->avatarId);
 +            message._rankInAlliance = static_cast<int>(AllianceUtils::getIndexForMemberType(allyInfo->memberType));
 +        }*/
        }

        public void SendDebugMessage(string text)
        {
            var message = new MessageType(text);
            SendMessage(message);
        }

        void OnMessageSent(int index, string originalUuid)
        {
            _messages.Edit(index, msg => {
                if(msg.Uuid == originalUuid)
                {
                    msg.IsSending = false;
                }
            });
        }
    }
}
