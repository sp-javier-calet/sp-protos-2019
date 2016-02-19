
namespace SocialPoint.Attributes
{
    public class JsonStreamReader : FastJsonStreamReader
    {
        public JsonStreamReader(byte[] data) : base(data)
        {
        }
        
        public JsonStreamReader(string data) : base(data)
        {
        }
    }
}