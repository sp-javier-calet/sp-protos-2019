using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class MessageOriginSystem : IMessageOrigin
    {
        public string Identifier
        {
            get
            {
                return IdentifierKey;
            }
        }

        public const string IdentifierKey = "system";
    }

    public sealed class MessageOriginSystemFactory : IMessageOriginFactory
    {
        public IMessageOrigin CreateOrigin(AttrDic data)
        {
            return new MessageOriginSystem();
        }
    }
}