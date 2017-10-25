using System;
using System.Text;
using System.Runtime.Serialization;
using SocialPoint.Base;

namespace SocialPoint.Attributes
{
    public abstract class StreamReaderAttrParser : IAttrParser
    {
        protected abstract IStreamReader CreateStreamReader(string data);

        public Attr Parse(byte[] data)
        {
            var dataString = Encoding.UTF8.GetString(data);
            return ParseString(dataString);
        }

        static bool IsEmptyString(string data)
        {
            char c;
            for(int i = 0; i < data.Length; ++i)
            {
                c = data[i];

                if(c > ' ')
                {
                    return false;
                }
                if(c != ' ' && c != '\t' && c != '\n' && c != '\r' && c != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public Attr ParseString(string data)
        {
            try
            {
                var reader = CreateStreamReader(data);
                if(data == null || IsEmptyString(data))
                {
                    return new AttrEmpty();
                }
                Attr result = null;
                reader.Read(); 
                result = reader.ParseElement();
                if(result == null)
                {
                    result = new AttrEmpty();
                }
                //Check if all the tokens were read
                if(!reader.Read())
                {
                    return result;
                }
            }
            catch(Exception e)
            {
                throw new SerializationException("Error reading data", e);
            }
            return null;
        }
    }
}
