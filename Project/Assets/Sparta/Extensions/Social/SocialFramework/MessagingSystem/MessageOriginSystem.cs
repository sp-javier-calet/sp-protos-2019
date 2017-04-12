using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    class MessageOriginSystem : IMessageOrigin
    {
        public string GetIdentifier()
        {
            return Identifier;
        }

        public const string Identifier = "system";
    }

    class MessageOriginSystemFactory : IMessageOriginFactory
    {
        public IMessageOrigin CreateOrigin(AttrDic data)
        {
            return new MessageOriginSystem();
        }
    }
}