using SocialPoint.Attributes;

namespace SocialPoint.Attributes
{
    public interface IParser<T>
    {
        T Parse(Attr data);
    }
}
