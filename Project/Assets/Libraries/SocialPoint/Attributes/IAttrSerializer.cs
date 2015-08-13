

using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public interface IAttrSerializer
    {
        Data Serialize(Attr attr);  
    }
}
