using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IMessagePayloadFactory
    {
        IMessagePayload CreatePayload(AttrDic data);
    }
}