using System;
using SocialPoint.Base;

namespace SocialPoint.Attributes
{
    public enum StreamToken
    {
        None,

        ObjectStart,
        PropertyName,
        ObjectEnd,

        ArrayStart,
        ArrayEnd,

        Int,
        Long,
        Double,

        String,

        Boolean,
        Null
    }

    public interface IStreamReader
    {
        StreamToken Token { get; }

        bool Read();

        object Value { get; }

    }

    public static class StreamReaderExtensions
    {       
        public static void SkipElement(this IStreamReader reader)
        {
            int count = 0;
            do
            {
                var t = reader.Token;
                if(t == StreamToken.ArrayStart || t == StreamToken.ObjectStart)
                {
                    count++;
                }
                else if(t == StreamToken.ArrayEnd || t == StreamToken.ObjectEnd)
                {
                    count--;
                }
                if(count <= 0)
                {
                    break;
                }
            }
            while(reader.Read());
        }

        public static void SkipToObjectEnd(this IStreamReader reader)
        {
            while(reader.Read() && reader.Token != StreamToken.ObjectEnd)
            {
                reader.SkipElement();
            }
        }

        public static int GetIntValue(this IStreamReader reader, int defaultValue = 0)
        {
            var v = reader.Value;
            if(reader.Token == StreamToken.Int)
            {
                return (int)v;
            }
            else
            {
                if(v == null || reader.Token == StreamToken.Null)
                {
                    return defaultValue;
                }
                int result;
                if(int.TryParse(v.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        public static double GetDoubleValue(this IStreamReader reader, double defaultValue = 0)
        {
            var v = reader.Value;
            if(reader.Token == StreamToken.Double)
            {
                return (double)v;
            }
            else
            {
                if(v == null || reader.Token == StreamToken.Null)
                {
                    return defaultValue;
                }
                double result;
                if(double.TryParse(v.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        public static long GetLongValue(this IStreamReader reader, long defaultValue = 0)
        {
            var v = reader.Value;
            if(reader.Token == StreamToken.Long)
            {
                return (long)v;
            }
            else
            {
                if(v == null || reader.Token == StreamToken.Null)
                {
                    return defaultValue;
                }
                long result;
                if(long.TryParse(v.ToString(), out result))
                {
                    return result;
                }
            }
            return defaultValue;
        }

        public static float GetFloatValue(this IStreamReader reader, float defaultValue = 0.0f)
        {
            var v = reader.Value;
            if(reader.Token == StreamToken.Long)
            {
                return (long)v;
            }
            else
            {
                if(v == null || reader.Token == StreamToken.Null)
                {
                    return defaultValue;
                }
                float result;
                if(float.TryParse(v.ToString(), out result))
                {
                    return result;
                }
                return defaultValue;
            }
        }

        public static bool GetBoolValue(this IStreamReader reader)
        {
            if(reader.Token == StreamToken.Boolean)
            {
                return (bool)reader.Value;
            }
            else
            {
                return false;
            }
        }

        public static string GetStringValue(this IStreamReader reader)
        {
            var v = reader.Value;
            if(v != null)
            {
                return v.ToString();
            }
            else
            {
                return string.Empty;
            }
        }

        private static AttrDic ParseObject(this IStreamReader reader)
        {
            var attr = new AttrDic();
            while(reader.Read())
            {
                if(reader.Token == StreamToken.ObjectEnd)
                {
                    break;
                }
                if(reader.Token != StreamToken.PropertyName)
                {
                    throw new InvalidOperationException("Trying to parse object without property name.");
                }
                var key = (string)reader.Value;
                reader.Read();
                attr.Set(key, ParseElement(reader));
            }
            return attr;
        }
        
        private static AttrList ParseArray(this IStreamReader reader)
        {
            var attr = new AttrList();
            while(reader.Read())
            {
                if(reader.Token == StreamToken.ArrayEnd)
                {
                    break;
                }
                attr.Add(ParseElement(reader));
            }
            return attr;
        }
        
        private static Attr ParseValue(this IStreamReader reader)
        {
            if(reader.Token == StreamToken.Boolean)
            {
                return new AttrBool((bool)reader.Value);
            }
            else if(reader.Token == StreamToken.Int)
            {
                return new AttrInt((int)reader.Value);
            }
            else if(reader.Token == StreamToken.Long)
            {
                return new AttrLong((long)reader.Value);
            }
            else if(reader.Token == StreamToken.Double)
            {
                return new AttrDouble((double)reader.Value);
            }
            else if(reader.Token == StreamToken.String)
            {
                return new AttrString((string)reader.Value);
            }
            else if(reader.Token == StreamToken.Null)
            {
                return new AttrEmpty();
            }
            else
            {
                throw new InvalidOperationException("Unsupported attr value.");
            }
        }
        
        public static Attr ParseElement(this IStreamReader reader)
        {
            if(reader.Token == StreamToken.ObjectStart)
            {
                return ParseObject(reader);
            }
            else if(reader.Token == StreamToken.ArrayStart)
            {
                return ParseArray(reader);
            }
            else if(reader.Token == StreamToken.None)
            {
                return null;
            }
            else
            {
                return ParseValue(reader);
            }
        }
    }
}