
namespace SocialPoint.Attributes
{
    public sealed class JsonStreamReader : FastJsonStreamReader
    {
        public JsonStreamReader(byte[] data) : base(data)
        {
        }
        
        public JsonStreamReader(string data) : base(data)
        {
        }
    }
}