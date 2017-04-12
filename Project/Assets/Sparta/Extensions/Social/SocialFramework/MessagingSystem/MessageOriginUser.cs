﻿using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    class MessageOriginUser : IMessageOrigin
    {
        public SocialPlayer Player{ get; private set; }

        internal MessageOriginUser(SocialPlayer player)
        {
            Player = player;
        }

        public string GetIdentifier()
        {
            return Identifier;
        }

        public const string Identifier = "user";
    }

    class MessageOriginUserFactory : IMessageOriginFactory
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