using System;
using System.Text;
using LitJson;

namespace SocialPoint.Attributes
{
    public class LitJsonAttrSerializer : IAttrSerializer
    {
        public bool PrettyPrint;

        public LitJsonAttrSerializer()
        {
        }

        public void Serialize(Attr attr, JsonWriter writer)
        {
            if(attr == null)
            {
                return;
            }
            switch(attr.AttrType)
            {
            case AttrType.DICTIONARY:
                SerializeDic((AttrDic)attr, writer);
                break;
            case AttrType.LIST:
                SerializeList((AttrList)attr, writer);
                break;
            case AttrType.VALUE:
                SerializeValue((AttrValue)attr, writer);
                break;
            default:
                throw new InvalidOperationException("Unsupported attr type.");
            }
        }

        public void SerializeDic(AttrDic attr, JsonWriter writer)
        {
            if(attr == null)
            {
                return;
            }
            writer.WriteObjectStart();
            foreach(var pair in attr)
            {
                writer.WritePropertyName(pair.Key);
                Serialize(pair.Value, writer);
            }
            writer.WriteObjectEnd();
        }

        public void SerializeList(AttrList attr, JsonWriter writer)
        {
            if(attr == null)
            {
                return;
            }
            writer.WriteArrayStart();
            foreach(var child in attr)
            {
                Serialize(child, writer);
            }
            writer.WriteArrayEnd();
        }

        public void SerializeValue(AttrValue attr, JsonWriter writer)
        {
            if(attr == (AttrValue)null)
            {
                return;
            }
            switch(attr.AttrValueType)
            {
            case AttrValueType.EMPTY:
                writer.Write(null);
                break;
            case AttrValueType.STRING:
                writer.Write(attr.ToString());
                break;
            case AttrValueType.BOOL:
                writer.Write(attr.ToBool());
                break;
            case AttrValueType.INT:
                writer.Write(attr.ToInt());
                break;
            case AttrValueType.LONG:
                writer.Write(attr.ToLong());
                break;
            case AttrValueType.FLOAT:
                writer.Write(attr.ToFloat());
                break;
            case AttrValueType.DOUBLE:
                writer.Write(attr.ToDouble());
                break;
            default:
                throw new InvalidOperationException("Unsupported attr value type.");
            }
        }

        static readonly string NullString = "null";
        static readonly string kQuoteString = "\"";
        static readonly string kEscapeString = "\\";

        public byte[] Serialize(Attr attr)
        {
            return Encoding.UTF8.GetBytes(SerializeString(attr));
        }

        public string SerializeString(Attr attr)
        {
            if(attr.AttrType == AttrType.VALUE)
            {
                var attrval = attr.AsValue;
                if(attrval.AttrValueType == AttrValueType.STRING)
                {
                    var str = attr.ToString().Replace(kQuoteString, kEscapeString + kQuoteString);
                    return kQuoteString + str + kQuoteString;
                }
                else if(attrval.AttrValueType == AttrValueType.EMPTY)
                {
                    return NullString;
                }
                return attr.ToString();
            }
            var writer = new JsonWriter();
            writer.PrettyPrint = PrettyPrint;
            Serialize(attr, writer);
            return writer.ToString();
        }
    }
}

