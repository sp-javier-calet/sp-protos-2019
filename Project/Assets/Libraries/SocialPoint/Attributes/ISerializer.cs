
namespace SocialPoint.Attributes
{
    public interface ISerializer<T>
    {
        Attr Serialize(T obj);
    }
}
