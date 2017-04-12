using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IMessagePayload
    {
        string GetIdentifier();

        AttrDic Serialize();
    }
}
