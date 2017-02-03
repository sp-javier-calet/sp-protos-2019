using SocialPoint.Attributes;
using SocialPoint.Utils;

namespace SocialPoint.ServerMessaging
{

    public struct Origin
    {
        const string TypeKey = "type";
        const string IdKey = "id";
        const string NameKey = "name";
        const string IconKey = "icon";

        public string type;
        public string id;
        public string name;
        public string icon;

        public Origin(string name, string icon)
        {
            this.name = name;
            this.icon = icon;
            id = null;
            type = null;
        }

        public Origin(AttrDic data)
        {
            id = data.GetValue(IdKey).ToString();
            type = data.GetValue(TypeKey).ToString();
            name = data.GetValue(NameKey).ToString();
            icon = data.GetValue(IconKey).ToString();
        }

        public Attr ToAttr()
        {
            var attr = new AttrDic();
            attr.Set(NameKey, new AttrString(name));
            attr.Set(IconKey, new AttrString(icon));
            return attr;
        }
    }

    public class Message
    {
        protected const string MessageIdKey = "id";
        const string MessageTypeKey = "type";
        const string MessageOriginKey = "origin";
        protected const string MessageParamsKey = "params" ;
        const string MessageDestinationKey = "destination";

        public string Id { get; protected set; }

        public string Type { get; protected set; }

        public Origin Origin { get; private set; }

        public AttrDic Params { get; protected set; }

        public string Destination { get; private set; }

        public Message()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.ServerMessaging.Message"/> class to be sended.
        /// </summary>
        /// <param name="type">Type.</param>
        /// <param name="args">Arguments.</param>
        /// <param name="origin">Origin.</param>
        /// <param name="destination">Destination.</param>
        public Message(string type, AttrDic args, Origin origin, string destination)
        {
            Id = RandomUtils.GetUuid();
            Type = type;
            Params = args;
            Origin = origin;
            Destination = destination;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialPoint.ServerMessaging.Message"/> class that has been received.
        /// </summary>
        /// <param name="data">Data.</param>
        public Message(AttrDic data)
        {
            Id = data.GetValue(MessageIdKey).ToString();
            Type = data.GetValue(MessageTypeKey).ToString();
            Origin = new Origin(data.Get(MessageOriginKey).AsDic);
            Params = data.Get(MessageParamsKey).AsDic;
        }

        public Attr ToAttr()
        {
            var data = new AttrDic();
            data.Set(MessageIdKey, new AttrString(Id));
            data.Set(MessageTypeKey, new AttrString(Type));
            data.Set(MessageParamsKey, Params);
            data.Set(MessageOriginKey, Origin.ToAttr());
            data.Set(MessageDestinationKey, new AttrString(Destination));
            return data;
        }

        override public string ToString()
        {
            return string.Format("[message: id:{0}, type:{1}, args{2}, origin: {3}]", Id, Type, Params, Origin);
        }
    }
}

