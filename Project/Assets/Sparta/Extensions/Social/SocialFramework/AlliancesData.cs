using System.Collections.Generic;

namespace SocialPoint.Social
{
    public enum AllianceAccessType
    {
        Open,
        Private
    }

    public enum AllianceMemberType
    {
        Lead = 1,
        CoLead,
        Soldier,
        Undefined
    }

    public class AllianceMember
    {
        public string Uid;

        public string Name;

        public int Level;

        public int Score;

        public string AllianceId;

        public string AllianceName;

        public int AllianceAvatar;

        public AllianceMemberType Type;
    }

    public class AllianceBasicData // FIXME
    {
        public string Id;

        public string Name;

        public int Avatar;

        public int Score;

        public int Members;

        public int Requests;

        public int Requirement;

        public AllianceAccessType AccessType;

        public int ActivityIndicator;

        public bool IsNewAlliance;
    }

    public class AllianceRankingData
    {
        public int PlayerScore;

        public int PlayerAlliancePosition;

        public AllianceBasicData PlayerAllianceData;

        readonly List<AllianceBasicData> RankingData;

        public AllianceRankingData()
        {
            RankingData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            RankingData.Add(data);
        }
    }

    public class AlliancesSearchData
    {
        public int PlayerScore;

        readonly List<AllianceBasicData> SearchData;

        public AlliancesSearchData()
        {
            SearchData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            SearchData.Add(data);
        }
    }

    public class AlliancesCreateData
    {
        public string Name;

        public string Description;

        public int RequirementValue;

        public int Avatar;

        public AllianceAccessType Type;

        public AlliancesCreateData()
        {
            Avatar = 1;
            Type = AllianceAccessType.Open;
        }
    }
}
