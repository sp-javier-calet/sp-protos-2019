using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    class MessageOriginAlliance : IMessageOrigin
    {
        public AllianceBasicData Alliance{ get; private set; }

        internal MessageOriginAlliance(AllianceBasicData alliance)
        {
            Alliance = alliance;
        }

        public string GetIdentifier()
        {
            return Identifier;
        }

        public const string Identifier = "alliance";
    }

    class MessageOriginAllianceFactory : IMessageOriginFactory
    {
        readonly AllianceDataFactory _dataFactory;

        public MessageOriginAllianceFactory(AllianceDataFactory dataFactory)
        {
            _dataFactory = dataFactory;
        }

        public IMessageOrigin CreateOrigin(AttrDic data)
        {
            var alliance = _dataFactory.CreateBasicData(data);
            return new MessageOriginAlliance(alliance);
        }
    }
}
