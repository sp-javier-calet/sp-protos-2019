

namespace SocialPoint.Social
{
    public class AllianceBasicData // FIXME
    {
        public string Id;
        public string Name;
        public int AvatarId;
        public int MemberCount;
        public int Score;
        public int RequestCount;
        public int ScoreToJoin;
        public AllianceAccessType AccessType;
        public int ActivityIndicator;
        public bool IsNewAlliance;
    }

    public class AllianceRankingData
    {
        public AllianceBasicData PlayerAllianceData;
        public int PlayerAlliancePosition;
        public int PlayerScore;

        public void Add(AllianceBasicData data)
        {
        }
    }

    public class AlliancesSearchData
    {
        public int PlayerScore;

        public void Add(AllianceBasicData data)
        {
        }
    }

    public class AlliancesCreateData
    {
        public string Name;
        public string Description;
        public int RequirementValue;
        public int AvatarId = 1;
        public AllianceAccessType AccessType = AllianceAccessType.Open;
    }
}
