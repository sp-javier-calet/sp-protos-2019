using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Connection;
using SocialPoint.WAMP;

namespace SocialPoint.Social
{
    public sealed class MessagingSystemManager : IDisposable
    {
        readonly ConnectionManager _connection;

        SocialManager _socialManager;

        internal SocialManager SocialManager {
            private get
            {
                return _socialManager; 
            } 
            set
            {
                _socialManager = value;
                _originFactories.Add(MessageOriginUser.Identifier, new MessageOriginUserFactory(SocialManager.PlayerFactory));
            }
        }

        AlliancesManager _alliancesManager;

        internal AlliancesManager AlliancesManager
        {
            private get
            {
                return _alliancesManager; 
            } 
            set
            {
                _alliancesManager = value;
                _originFactories.Add(MessageOriginAlliance.Identifier, new MessageOriginAllianceFactory(AlliancesManager.Factory));
            }
        }

        readonly List<Message> _listMessages;
        readonly Dictionary<string, IMessageOriginFactory> _originFactories;
        readonly Dictionary<string, IMessagePayloadFactory> _payloadFactories;

        public delegate void FinishCallback(Error error, AttrDic dic);

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

            AddDefaultFactories();
        }

        public void Dispose()
        {
            _connection.OnNotificationReceived -= OnNotificationReceived;
            _connection.OnProcessServices -= OnProcessServices;
        }

        void AddDefaultFactories()
        {
            _originFactories.Add(MessageOriginSystem.Identifier, new MessageOriginSystemFactory());

            _payloadFactories.Add(MessagePayloadPlainText.Identifier, new MessagePayloadPlainTextFactory());
        }

        public ReadOnlyCollection<Message> GetMessages()
        {
            return _listMessages.AsReadOnly();
        }

        public WAMPRequest SendMessage(string destinationType, AttrDic destinationData, IMessagePayload payload, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue("destination_type", destinationType);
            paramsDic.Set("destination_data", destinationData);
            paramsDic.SetValue("payload_type", payload.GetIdentifier());
            paramsDic.Set("payload_data", payload.Serialize());

            return _connection.Call("messaging_system.send", null, paramsDic, (error, AttrList, attrDic) => {
                if(callback != null)
                {
                    callback(error, attrDic);
                }
            });
        }

        public WAMPRequest DeleteMessage(Message msg, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue("msg_id", msg.Id);
            return _connection.Call("messaging_system.delete", null, paramsDic, (error, AttrList, attrDic) => {
                if(callback != null)
                {
                    if(Error.IsNullOrEmpty(error))
                    {
                        _listMessages.Remove(msg);
                    }
                    callback(error, attrDic);
                }
            });
        }

        public WAMPRequest AddMessageProperty(string property, Message msg, FinishCallback callback)
        {
            return AddMessageProperties(new List<string>{ property }, msg, callback);
        }

        public WAMPRequest AddMessageProperties(List<string> properties, Message msg, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue("msg_id", msg.Id);
            paramsDic.Set("properties", new AttrList(properties));
            return _connection.Call("messaging_system.add_properties", null, paramsDic, (error, AttrList, attrDic) => {
                if(callback != null)
                {
                    if(Error.IsNullOrEmpty(error))
                    {
                        using(var propertyItr = properties.GetEnumerator())
                        {
                            while(propertyItr.MoveNext())
                            {
                                msg.AddProperty(propertyItr.Current);
                            }
                        }
                    }
                    callback(error, attrDic);
                }
            });
        }

        public WAMPRequest RemoveMessageProperty(string property, Message msg, FinishCallback callback)
        {
            return RemoveMessageProperties(new List<string>{ property }, msg, callback);
        }

        public WAMPRequest RemoveMessageProperties(List<string> properties, Message msg, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue("msg_id", msg.Id);
            paramsDic.Set("properties", new AttrList(properties));
            return _connection.Call("messaging_system.remove_properties", null, paramsDic, (error, AttrList, attrDic) => {
                if(callback != null)
                {
                    if(Error.IsNullOrEmpty(error))
                    {
                        using(var propertyItr = properties.GetEnumerator())
                        {
                            while(propertyItr.MoveNext())
                            {
                                msg.RemoveProperty(propertyItr.Current);
                            }
                        }
                    }
                    callback(error, attrDic);
                }
            });
        }

        void ClearMessages()
        {
            _listMessages.Clear();
        }

        void OnProcessServices(AttrDic servicesDic)
        {
            ClearMessages();

            var messagingServiceData = servicesDic.Get("message_system").AsDic;
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
            if(OnHistoricReceived != null)
            {
                OnHistoricReceived();
            }
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
                    if(OnNewMessageEvent != null)
                    {
                        OnNewMessageEvent(message);
                    }
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

            var origin = ParseMessageOrigin(data);
            var payload = ParseMessagePayload(data);
            if(origin == null || payload == null)
            {
                return null;
            }

            var message = new Message(id, origin, payload);

            ParseMessageProperties(data, message);

            return message;
        }

        IMessageOrigin ParseMessageOrigin(AttrDic data)
        {
            var type = data.Get("origin_type").AsValue.ToString();
            IMessageOriginFactory factory;
            return !_originFactories.TryGetValue(type, out factory) ? null : factory.CreateOrigin(data.Get("origin_data").AsDic);
        }

        IMessagePayload ParseMessagePayload(AttrDic data)
        {
            var type = data.Get("payload_type").AsValue.ToString();
            IMessagePayloadFactory factory;
            return !_payloadFactories.TryGetValue(type, out factory) ? null : factory.CreatePayload(data.Get("payload_data").AsDic);
        }

        static void ParseMessageProperties(AttrDic data, Message message)
        {
            var propertiesList = data.Get("properties").AsList;
            using(var propertiesItr = propertiesList.GetEnumerator())
            {
                while(propertiesItr.MoveNext())
                {
                    message.AddProperty(propertiesItr.Current.AsValue.ToString());
                }
            }
        }

        static AttrList SerializeMessageProperties(Message message)
        {
            var list = new AttrList();
            using(var propertiesItr = message.GetProperties())
            {
                while(propertiesItr.MoveNext())
                {
                    list.AddValue(propertiesItr.Current);
                }
            }
            return list;
        }
    }
}