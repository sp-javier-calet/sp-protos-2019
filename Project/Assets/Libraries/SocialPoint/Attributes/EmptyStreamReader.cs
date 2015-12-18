
namespace SocialPoint.Attributes
{
    public class EmptyStreamReader : IStreamReader
    {
        public bool Read()
        {
            return false;
        }
        
        public StreamToken Token
        {
            get
            {
                return StreamToken.None;
            }
        }
        
        public object Value
        {
            get
            {
                return null;
            }
        }
    }

}