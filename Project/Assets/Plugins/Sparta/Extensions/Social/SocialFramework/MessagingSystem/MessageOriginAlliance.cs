using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class MessageOriginAlliance : IMessageOrigin
    {
        public AllianceBasicData Alliance{ get; private set; }

        internal MessageOriginAlliance(AllianceBasicData alliance)
        {
            Alliance = alliance;
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
            return string.Format("[MessageOriginAlliance: Alliance={0}]", Alliance);
        }

        public const string IdentifierKey = "alliance";
    }

    public sealed class MessageOriginAllianceFactory : IMessageOriginFactory
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
