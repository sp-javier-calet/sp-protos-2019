
namespace SocialPoint.Attributes
{
    public sealed class LitJsonStreamReader : IStreamReader
    {
        LitJson.JsonReader _reader;
        
        public LitJsonStreamReader(byte[] data) : this(System.Text.Encoding.UTF8.GetString(data))
        {
        }
        
        public LitJsonStreamReader(string data)
        {
            _reader = new LitJson.JsonReader(data);
        }
        
        public bool Read ()
        {
            return _reader.Read();
        }
        
        public StreamToken Token
        {
            get
            {
                return (StreamToken)_reader.Token;
            }
        }
        
        public object Value
        {
            get
            {
                return _reader.Value;
            }
        }
    }

}