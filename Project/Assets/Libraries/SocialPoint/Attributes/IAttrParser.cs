
namespace SocialPoint.Attributes
{
    public interface IAttrParser
    {
        Attr Parse(byte[] data);
        Attr ParseString(string data);
    }
}
