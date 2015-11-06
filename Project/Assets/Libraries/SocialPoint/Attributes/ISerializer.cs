using SocialPoint.Attributes;

namespace SocialPoint.Attributes
{
    public interface ISerializer<T>
    {
        Attr Serialize(T obj);
    }
}