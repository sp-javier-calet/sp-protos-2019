using System;
using System.Text;
using System.Runtime.Serialization;
using SocialPoint.Base;
using LitJson;

namespace SocialPoint.Attributes
{
    public class FastJsonAttrParser : IAttrParser
    {
        public Attr Parse(byte[] data)
        {
            return ParseString(Encoding.UTF8.GetString(data));
        }

        static bool IsEmptyString(string data)
        {
            char c;
            for(int i = 0; i < data.Length; ++i)
            {
                c = data[i];

                if (c > ' ') return false;
                if (c != ' ' && c != '\t' && c != '\n' && c != '\r' && c != 0) return false;
            }
            return true;
        }

        public Attr ParseString(string data)
        {
            try
            {
                FastJsonStreamReader reader = new FastJsonStreamReader(data);
                if(data == null || IsEmptyString(data))
                {
                    return new AttrEmpty();
                }
                Attr result = null;
                reader.Read(); 
                result = reader.ParseElement();
                if (result == null)
                    result = new AttrEmpty();
                return result;
            }
            catch {}
            throw new SerializationException(string.Format("Error reading data: {0}", data));
        }
    }
}
