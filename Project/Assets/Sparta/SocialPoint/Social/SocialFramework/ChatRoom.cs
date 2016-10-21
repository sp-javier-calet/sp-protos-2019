using System;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IChatRoom
    {
        string Type { get; }

        int Members { get; set; }

        bool Subscribed { get; set; }
        // TODO Accessible from ChatManager only

        bool IsAllianceChat { get; }

        void ParseInitialInfo(AttrDic dic);

        void AddNotificationMessage(AttrDic dic);
    }

    public class BaseChatRoom : IChatRoom
    {
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

        public bool Subscribed { get; set; }

        public bool IsAllianceChat
        { 
            get
            {
                return Type == "alliance"; 
            }
        }

        public BaseChatRoom(string name, ConnectionManager connection)
        {
            Name = name;
            _connection = connection;
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

        public virtual void AddNotificationMessage(AttrDic dic)
        {
        }

        public virtual void AddHistoricMessages(AttrList list)
        {
            
        }

        public virtual void SendDebugMessage(string text)
        {
        }
    }
}
