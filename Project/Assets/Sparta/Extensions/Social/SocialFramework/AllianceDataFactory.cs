using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class AllianceDataFactory
    {
        public AllianceBasicData CreateBasicData(Alliance alliance)
        {
            return new AllianceBasicData(); // TODO
        }

        public Alliance CreateAlliance(string allianceId, AttrDic dic)
        {
            // TODO
            return null;
        }

        public AllianceMember CreateMember(AttrDic dic)
        {
            // TODO
            return null;
        }

        public AllianceRankingData CreateRankingData(AttrDic dic)
        {
            // TODO
            return null;
        }

        public AlliancesSearchData CreateSearchData(AttrDic dic, bool suggested)
        {
            // TODO
            return null;
        }

        public AlliancesSearchData CreateJoinData(AttrDic dic)
        {
            // TODO
            return null;
        }

        public AlliancePlayerInfo CreatePlayerInfo(uint maxPendingJoinRequests, AttrDic dic)
        {
            // TODO
            return null;
        }
    }
}
