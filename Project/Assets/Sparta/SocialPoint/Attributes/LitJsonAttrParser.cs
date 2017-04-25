using System;
using System.Text;
using System.Runtime.Serialization;
using SocialPoint.Base;
using LitJson;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public sealed class LitJsonAttrParser : IAttrParser
    {
        public LitJsonAttrParser()
        {
        }

        private AttrDic ParseObject(JsonReader reader)
        {
            var attr = new AttrDic();
            while(reader.Read())
            {
                if(reader.Token == JsonToken.ObjectEnd)
                {
                    break;
                }
                if(reader.Token != JsonToken.PropertyName)
                {
                    throw new InvalidOperationException("Trying to parse object without property name.");
                }
                var key = (string)reader.Value;
                reader.Read();
                attr.Set(key, Parse(reader));
            }
            return attr;
        }

        private AttrList ParseArray(JsonReader reader)
        {
            var attr = new AttrList();
            while(reader.Read())
            {
                if(reader.Token == JsonToken.ArrayEnd)
                {
                    break;
                }
                attr.Add(Parse(reader));
            }
            return attr;
        }

        private Attr ParseValue(JsonReader reader)
        {
            if(reader.Token == JsonToken.Boolean)
            {
                return new AttrBool((bool)reader.Value);
            }
            else if(reader.Token == JsonToken.Int)
            {
                return new AttrInt((int)reader.Value);
            }
            else if(reader.Token == JsonToken.Long)
            {
                return new AttrLong((long)reader.Value);
            }
            else if(reader.Token == JsonToken.Double)
            {
                return new AttrDouble((double)reader.Value);
            }
            else if(reader.Token == JsonToken.String)
            {
                return new AttrString((string)reader.Value);
            }
            else if(reader.Token == JsonToken.Null)
            {
                return new AttrEmpty();
            }
            else
            {
                throw new InvalidOperationException("Unsupported attr value.");
            }
        }

        private Attr Parse(JsonReader reader)
        {
            if(reader.Token == JsonToken.ObjectStart)
            {
                return ParseObject(reader);
            }
            else if(reader.Token == JsonToken.ArrayStart)
            {
                return ParseArray(reader);
            }
            else
            {
                return ParseValue(reader);
            }
        }

        static readonly string kNullString = "null";
        static readonly string kQuoteString = "\"";
        static readonly string kEscapeString = "\\";

        public Attr Parse(byte[] data)
        {
            return ParseString(Encoding.UTF8.GetString(data));
        }

        public Attr ParseString(string data)
        {
            data = data.Trim();

            if(StringUtils.StartsWith(data, kQuoteString) && StringUtils.EndsWith(data, kQuoteString) && data.Length >= 2 * kQuoteString.Length)
            {
                data = data.Substring(kQuoteString.Length, data.Length - 2 * kQuoteString.Length);
                var i = 0;
                while(true && i < data.Length)
                {
                    i = data.IndexOf(kQuoteString, i + 1);
                    if(i == -1)
                    {
                        break;
                    }
                    if(data.Substring(i - kEscapeString.Length, kEscapeString.Length) != kEscapeString)
                    {
                        throw new SerializationException("Invalid string value.");
                    }
                }
                data = data.Replace(kEscapeString, string.Empty);
                return new AttrString(data);
            }
            string parseErrMsg = string.Empty;
            try
            {
                var reader = new JsonReader(data);
                if(reader.Read())
                {
                    return Parse(reader);
                }
            }
            catch(JsonException e)
            {
                parseErrMsg = e.Message;
            }
        
            bool boolval;
            if(bool.TryParse(data, out boolval))
            {
                return new AttrBool(boolval);
            }
            int intval;
            if(int.TryParse(data, out intval))
            {
                return new AttrInt(intval);
            }
            long longval;
            if(long.TryParse(data, out longval))
            {
                return new AttrLong(longval);
            }
            float floatval;
            if(float.TryParse(data, out floatval))
            {
                return new AttrDouble(floatval);
            }
            double doubleval;
            if(double.TryParse(data, out doubleval))
            {
                return new AttrDouble(doubleval);
            }
            if(string.Equals(data, kNullString, StringComparison.CurrentCultureIgnoreCase) || data.Length == 0)
            {
                return new AttrEmpty();
            }
            throw new SerializationException(string.Format("Error reading data: {0}", parseErrMsg));
        }
    }
}
