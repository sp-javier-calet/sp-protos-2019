
namespace SocialPoint.Attributes
{
    public interface IAttrSerializer
    {
        byte[] Serialize(Attr attr);  
        string SerializeString(Attr attr);
    }
}
