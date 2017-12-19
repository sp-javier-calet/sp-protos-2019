
namespace SocialPoint.Attributes
{
    public class FastJsonAttrParser : StreamReaderAttrParser
    {
        protected override IStreamReader CreateStreamReader(string data)
        {
            return new FastJsonStreamReader(data);
        }
    }
}
