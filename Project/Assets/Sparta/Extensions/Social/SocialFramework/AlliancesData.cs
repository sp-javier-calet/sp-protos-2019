﻿using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class AllianceMember
    {
        public string Uid;

        public string Name;

        public int Level;

        public int Score;

        public string AllianceId;

        public string AllianceName;

        public int AllianceAvatar;

        public int Rank;
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

        public int AccessType;

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
        public string Filter;
    }

    public class AlliancesSearchResultData
    {
        public int Score;

        readonly List<AllianceBasicData> _searchData;

        public AlliancesSearchResultData()
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
}
