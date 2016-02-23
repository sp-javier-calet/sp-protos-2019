using System;
using SocialPoint.Utils;
using SocialPoint.Attributes;

namespace SocialPoint.ServerMessaging
{

    public struct Destination
    {
        const string IdKey = "id";
        const string TypeKey = "type";

        public string type;
        public string id;

        public Destination(string id, string type)
        {
            this.id = id;
            this.type = type;
        }

        public Destination(AttrDic data)
        {
            id = data.GetValue(IdKey).ToString();
            type = data.GetValue(TypeKey).ToString();
        }

        public Attr ToAttr()
        {
            var attr = new AttrDic();
            attr.Set(IdKey, new AttrString(id));
            attr.Set(TypeKey, new AttrString(type));
            return attr;
        }
    }

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

        public Origin(AttrDic data)
        {
            id = data.GetValue(IdKey).ToString();
            type = data.GetValue(TypeKey).ToString();
            name = data.GetValue(NameKey).ToString();
            icon = data.GetValue(IconKey).ToString();
        }

        public Attr ToAttr()
        {
            return null;
        }
    }

    public class Message
    {
        const string MessageIdKey = "id";
        const string MessageTypeKey = "type";
        const string MessageOriginKey = "origin";
        const string MessageParamsKey = "params";
        const string MessageDestinationKey = "destination";

        public string Id { get; private set; }

        public string Type { get; private set; }

        public Origin Origin { get; private set; }

        public AttrDic Params { get; private set; }

        public Destination Destination { get; private set; }

        public Message(string type, AttrDic args, Origin origin, Destination destination)
        {
            Id = RandomUtils.GetUuid();
            Type = type;
            Params = args;
            Origin = origin;
            Destination = destination;
        }

        public Message(AttrDic data)
        {
            Id = data.GetValue(MessageIdKey).ToString();
            Type = data.GetValue(MessageTypeKey).ToString();
            Origin = new Origin(data.GetValue(MessageOriginKey).AsDic);
            Params = data.GetValue(MessageParamsKey).AsDic;
            if(data.ContainsKey(MessageDestinationKey))
            {
                Destination = new Destination(data.GetValue(MessageDestinationKey).AsDic);
            }
        }

        public Attr ToAttr()
        {
            var data = new AttrDic();
            data.Set(MessageIdKey, new AttrString(Id));
            data.Set(MessageTypeKey, new AttrString(Type));
            data.Set(MessageParamsKey, Params);
            data.Set(MessageOriginKey, Origin.ToAttr());
            data.Set(MessageDestinationKey, Destination.ToAttr());
            return data;
        }
    }
}

