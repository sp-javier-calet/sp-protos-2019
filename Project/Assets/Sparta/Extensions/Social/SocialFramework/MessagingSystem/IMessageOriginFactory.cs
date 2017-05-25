using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IMessageOriginFactory
    {
        IMessageOrigin CreateOrigin(AttrDic data);
    }
}