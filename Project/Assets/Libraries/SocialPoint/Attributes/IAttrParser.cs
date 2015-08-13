
using SocialPoint.Utils;

namespace SocialPoint.Attributes
{
    public interface IAttrParser
    {
        Attr Parse(Data data);
    }
}
