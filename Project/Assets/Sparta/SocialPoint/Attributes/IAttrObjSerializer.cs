
namespace SocialPoint.Attributes
{
    public interface IAttrObjSerializer<T>
    {
        Attr Serialize(T obj);
    }
}
