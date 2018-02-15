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
        #region Attr keys

        const string DestinationTypeKey = "destination_type";
        const string DestinationDataKey = "destination_data";
        const string OriginTypeKey = "origin_type";
        const string OriginDataKey = "origin_data";
        const string PayloadTypeKey = "payload_type";
        const string PayloadDataKey = "payload_data";
        const string MsgIdKey = "msg_id";
        const string PropertiesKey = "properties";
        const string MessageSystemKey = "message_system";
        const string MsgsKey = "msgs";
        const string MsgKey = "msg";
        const string IdKey = "id";
        const string TimestampKey = "timestamp";

        #endregion

        #region RPC methods

        const string MessagingSystemSendMethod = "messaging_system.send";
        const string MessagingSystemDeleteMethod = "messaging_system.delete";
        const string MessagingSystemAddPropertiesMethod = "messaging_system.add_properties";
        const string MessagingSystemRemovePropertiesMethod = "messaging_system.remove_properties";

        #endregion

        readonly ConnectionManager _connection;

        SocialManager _socialManager;

        internal SocialManager SocialManager
        {
            private get
            {
                return _socialManager; 
            } 
            set
            {
                _socialManager = value;
                _originFactories.Add(MessageOriginUser.IdentifierKey, new MessageOriginUserFactory(SocialManager.PlayerFactory));
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
                _originFactories.Add(MessageOriginAlliance.IdentifierKey, new MessageOriginAllianceFactory(AlliancesManager.Factory));
            }
        }

        readonly List<Message> _listMessages;
        readonly Dictionary<string, IMessageOriginFactory> _originFactories;
        readonly Dictionary<string, IMessagePayloadFactory> _payloadFactories;

        public delegate void FinishCallback(Error error, AttrDic dic);

        public delegate void OnNewMessage(Message msg);

        public event Action OnHistoricReceived;
        public event OnNewMessage OnNewMessageEvent;

        public bool IsReady{ get; private set; }

        public MessagingSystemManager(ConnectionManager connectionManager)
        {
            _connection = connectionManager;
            _connection.OnNotificationReceived += OnNotificationReceived;
            _connection.OnProcessServices += OnProcessServices;

            _listMessages = new List<Message>();
            _originFactories = new Dictionary<string, IMessageOriginFactory>();
            _payloadFactories = new Dictionary<string, IMessagePayloadFactory>();

            IsReady = false;

            AddDefaultFactories();
        }

        public void Dispose()
        {
            IsReady = false;
            _connection.OnNotificationReceived -= OnNotificationReceived;
            _connection.OnProcessServices -= OnProcessServices;
        }

        void AddDefaultFactories()
        {
            _originFactories.Add(MessageOriginSystem.IdentifierKey, new MessageOriginSystemFactory());

            _payloadFactories.Add(MessagePayloadPlainText.IdentifierKey, new MessagePayloadPlainTextFactory());
        }

        public void AddOriginFactory(string identifier, IMessageOriginFactory factory)
        {
            _originFactories.Add(identifier, factory);
        }

        public void AddPayloadFactory(string identifier, IMessagePayloadFactory factory)
        {
            _payloadFactories.Add(identifier, factory);
        }

        public ReadOnlyCollection<Message> GetMessages()
        {
            return _listMessages.AsReadOnly();
        }

        public Message GetMessageFromID(string id)
        {
            return _listMessages.Find(message => message.Id == id);

        }

        public WAMPRequest SendMessage(string destinationType, AttrDic destinationData, IMessagePayload payload, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue(DestinationTypeKey, destinationType);
            paramsDic.Set(DestinationDataKey, destinationData);
            paramsDic.SetValue(PayloadTypeKey, payload.Identifier);
            paramsDic.Set(PayloadDataKey, payload.Serialize());

            return _connection.Call(MessagingSystemSendMethod, null, paramsDic, (error, AttrList, attrDic) => {
                if(callback != null)
                {
                    callback(error, attrDic);
                }
            });
        }

        public WAMPRequest DeleteMessage(Message msg, FinishCallback callback)
        {
            var paramsDic = new AttrDic();
            paramsDic.SetValue(MsgIdKey, msg.Id);
            return _connection.Call(MessagingSystemDeleteMethod, null, paramsDic, (error, AttrList, attrDic) => {
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
            paramsDic.SetValue(MsgIdKey, msg.Id);
            paramsDic.Set(PropertiesKey, new AttrList(properties));
            return _connection.Call(MessagingSystemAddPropertiesMethod, null, paramsDic, (error, AttrList, attrDic) => {
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
            paramsDic.SetValue(MsgIdKey, msg.Id);
            paramsDic.Set(PropertiesKey, new AttrList(properties));
            return _connection.Call(MessagingSystemRemovePropertiesMethod, null, paramsDic, (error, AttrList, attrDic) => {
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

            var messagingServiceData = servicesDic.Get(MessageSystemKey).AsDic;
            var messagesList = messagingServiceData.Get(MsgsKey).AsList;
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

            IsReady = true;
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
                    var message = ParseMessage(dic.Get(MsgKey).AsDic);
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
            var id = data.Get(IdKey).AsValue.ToString();
            var timestamp = data.GetValue(TimestampKey).ToInt();

            var origin = ParseMessageOrigin(data);
            var payload = ParseMessagePayload(data);
            if(origin == null || payload == null)
            {
                return null;
            }

            var message = new Message(id, timestamp, origin, payload);

            ParseMessageProperties(data, message);

            return message;
        }

        IMessageOrigin ParseMessageOrigin(AttrDic data)
        {
            var type = data.Get(OriginTypeKey).AsValue.ToString();
            IMessageOriginFactory factory;
            return !_originFactories.TryGetValue(type, out factory) ? null : factory.CreateOrigin(data.Get(OriginDataKey).AsDic);
        }

        IMessagePayload ParseMessagePayload(AttrDic data)
        {
            var type = data.Get(PayloadTypeKey).AsValue.ToString();
            IMessagePayloadFactory factory;
            return !_payloadFactories.TryGetValue(type, out factory) ? null : factory.CreatePayload(data.Get(PayloadDataKey).AsDic);
        }

        static void ParseMessageProperties(AttrDic data, Message message)
        {
            var propertiesList = data.Get(PropertiesKey).AsList;
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