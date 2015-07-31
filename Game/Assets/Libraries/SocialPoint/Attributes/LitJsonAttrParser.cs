using System;
using LitJson;
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public class LitJsonAttrParser : IAttrParser
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

        public Attr Parse(Data data)
        {
            var datastr = data.ToString().Trim();

            if(datastr.StartsWith(kQuoteString) && datastr.EndsWith(kQuoteString) && datastr.Length >= 2 * kQuoteString.Length)
            {
                datastr = datastr.Substring(kQuoteString.Length, datastr.Length - 2 * kQuoteString.Length);
                datastr = datastr.Replace(kEscapeString + kQuoteString, kQuoteString);
                return new AttrString(datastr);
            }
            try
            {
                var reader = new JsonReader(datastr);
                if(reader.Read())
                {
                    return Parse(reader);
                }
            }
            catch(JsonException)
            {
            }
        
            bool boolval;
            if(bool.TryParse(datastr, out boolval))
            {
                return new AttrBool(boolval);
            }
            int intval;
            if(int.TryParse(datastr, out intval))
            {
                return new AttrInt(intval);
            }
            long longval;
            if(long.TryParse(datastr, out longval))
            {
                return new AttrLong(longval);
            }
            float floatval;
            if(float.TryParse(datastr, out floatval))
            {
                return new AttrDouble(floatval);
            }
            double doubleval;
            if(double.TryParse(datastr, out doubleval))
            {
                return new AttrDouble(doubleval);
            }
            if(datastr.ToLower() == kNullString || datastr.Length == 0)
            {
                return new AttrEmpty();
            }
            throw new InvalidOperationException("Error reading data.");
        }
    }
}
