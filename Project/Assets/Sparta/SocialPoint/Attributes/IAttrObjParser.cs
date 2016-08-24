
namespace SocialPoint.Attributes
{
    public interface IAttrObjParser<T>
    {
        T Parse(Attr data);
    }
}
