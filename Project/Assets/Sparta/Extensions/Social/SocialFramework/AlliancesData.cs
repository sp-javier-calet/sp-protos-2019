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
        Colead,
        Member,
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

    public class AllianceBasicData
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
        public int Score;

        public int PlayerAlliancePosition;

        public AllianceBasicData PlayerAllianceData;

        readonly List<AllianceBasicData> _rankingData;

        public AllianceRankingData()
        {
            _rankingData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            _rankingData.Add(data);
        }

        public IEnumerator<AllianceBasicData> GetRanking()
        {
            return _rankingData.GetEnumerator();
        }
    }

    public class AlliancesSearchData
    {
        public int Score;

        readonly List<AllianceBasicData> _searchData;

        public AlliancesSearchData()
        {
            _searchData = new List<AllianceBasicData>();
        }

        public void Add(AllianceBasicData data)
        {
            _searchData.Add(data);
        }

        public IEnumerator<AllianceBasicData> GetSearch()
        {
            return _searchData.GetEnumerator();
        }
    }

    public class AlliancesCreateData
    {
        public string Name;

        public string Description;

        public int Requirement;

        public int Avatar;

        public AllianceAccessType Type;

        public AlliancesCreateData()
        {
            Avatar = 1;
            Type = AllianceAccessType.Open;
        }
    }
}
