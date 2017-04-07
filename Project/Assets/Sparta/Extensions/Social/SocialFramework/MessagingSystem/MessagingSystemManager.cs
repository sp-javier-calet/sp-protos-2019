using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Attributes;
using SocialPoint.Connection;

namespace SocialPoint.Social
{
    public sealed class MessagingSystemManager : IDisposable
    {
        readonly ConnectionManager _connection;

        readonly List<Message> _listMessages;
        readonly Dictionary<string, IMessageOriginFactory> _originFactories;
        readonly Dictionary<string, IMessagePayloadFactory> _payloadFactories;

        public delegate void FinishCallback(bool success, AttrDic dic);

        public delegate void OnNewMessage(Message msg);

        public event Action OnHistoricReceived;
        public event OnNewMessage OnNewMessageEvent;

        public MessagingSystemManager(ConnectionManager connectionManager)
        {
            _connection = connectionManager;
            _connection.OnNotificationReceived += OnNotificationReceived;
            _connection.OnProcessServices += OnProcessServices;

            _listMessages = new List<Message>();
            _originFactories = new Dictionary<string, IMessageOriginFactory>();
            _payloadFactories = new Dictionary<string, IMessagePayloadFactory>();
        }

        public void Dispose()
        {
            _connection.OnNotificationReceived -= OnNotificationReceived;
        }

        ReadOnlyCollection<Message> GetMessages()
        {
            return _listMessages.AsReadOnly();
        }

        void SendMessage(string destinationType, AttrDic destinationData, FinishCallback callback)
        {

        }

        void DeleteMessage(Message msg, FinishCallback callback)
        {

        }

        void AddMessageProperty(string property, Message msg, FinishCallback callback)
        {

        }

        void RemoveMessageProperty(string property, Message msg, FinishCallback callback)
        {

        }

        void ClearMessages()
        {
            _listMessages.Clear();
        }

        void OnProcessServices(AttrDic servicesDic)
        {
            ClearMessages();

            var messagingServiceData = servicesDic.Get("messaging_system").AsDic;
            var messagesList = messagingServiceData.Get("msgs").AsList;
            using(var messageDataItr = messagesList.GetEnumerator())
            {
                while(messageDataItr.MoveNext())
                {
                    var message = ParseMessage(messageDataItr.Current.AsDic);
                    if(message == null)
                    {
                        continue;
                    }
                    _listMessages.Add(message);
                }
            }
            OnHistoricReceived();
        }

        void OnNotificationReceived(int type, string topic, AttrDic dic)
        {
            switch(type)
            {
            case NotificationType.MessagingSystemNewMessage:
                {
                    var message = ParseMessage(dic.Get("msg").AsDic);
                    if(message == null)
                    {
                        break;
                    }
                    _listMessages.Add(message);
                    OnNewMessageEvent(message);
                    break;
                }
            default:
                {
                    break;
                }
            }
        }

        Message ParseMessage(AttrDic data)
        {
            var id = data.Get("id").AsValue.ToString();

        }
    }
}