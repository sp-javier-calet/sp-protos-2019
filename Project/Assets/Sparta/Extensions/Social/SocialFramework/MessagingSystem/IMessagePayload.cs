using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public interface IMessagePayload
    {
        string Identifier { get; }

        AttrDic Serialize();
    }
}
