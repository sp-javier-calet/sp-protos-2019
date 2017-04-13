using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class MessageOriginSystem : IMessageOrigin
    {
        public string GetIdentifier()
        {
            return Identifier;
        }

        public const string Identifier = "system";
    }

    public sealed class MessageOriginSystemFactory : IMessageOriginFactory
    {
        public IMessageOrigin CreateOrigin(AttrDic data)
        {
            return new MessageOriginSystem();
        }
    }
}