
namespace SocialPoint.Attributes
{
    public class JsonStreamReader : LitJsonStreamReader
    {
        public JsonStreamReader(byte[] data) : base(data)
        {
        }
        
        public JsonStreamReader(string data) : base(data)
        {
        }
    }
}