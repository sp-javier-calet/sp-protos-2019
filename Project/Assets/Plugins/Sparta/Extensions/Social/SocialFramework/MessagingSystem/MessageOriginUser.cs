using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class MessageOriginUser : IMessageOrigin
    {
        public SocialPlayer Player{ get; private set; }

        internal MessageOriginUser(SocialPlayer player)
        {
            Player = player;
        }

        public string Identifier
        {
            get
            {
                return IdentifierKey;
            }
        }

        public override string ToString()
        {
            return string.Format("[MessageOriginUser: Player={0}]", Player);
        }

        public const string IdentifierKey = "user";
    }

    public sealed class MessageOriginUserFactory : IMessageOriginFactory
    {
        readonly SocialPlayerFactory _playerFactory;

        public MessageOriginUserFactory(SocialPlayerFactory playerFactory)
        {
            _playerFactory = playerFactory;
        }

        public IMessageOrigin CreateOrigin(AttrDic data)
        {
            var socialPlayer = _playerFactory.CreateSocialPlayer(data);
            return new MessageOriginUser(socialPlayer);
        }
    }
}