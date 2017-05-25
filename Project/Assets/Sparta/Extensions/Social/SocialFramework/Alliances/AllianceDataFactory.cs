using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class AllianceDataFactory
    {
        #region Attr keys

        const string RankingKey = "ranking";
        const string RankingMeKey = "me";
        const string RankingRankKey = "rank";
        // TODO MONSTER :( score?
        const string RankingScoreKey = "monster_power";

        const string SearchKey = "search";
        const string SearchSuggestedKey = "suggested";
        const string SearchScoreKey = "monster_power";
        const string SearchFilterKey = "filter_name";

        protected const string AllianceInfoKey = "alliance_info";
        const string AllianceIdKey = "id";
        const string AllianceNameKey = "name";
        const string AllianceDescriptionKey = "description";
        const string AllianceMessageKey = "welcome_message";
        // TODO duplicated
        const string AllianceAvatarKey = "avatar";
        const string AllianceAvatarIdKey = "avatarId";
        // TODO duplicated
        const string AllianceTypeKey = "type";
        const string AllianceTypeAccessKey = "allianceType";
        const string AllianceTotalMembersKey = "member_count";
        const string AllianceTotalRequestsKey = "requests";
        const string AllianceMembersKey = "members";
        const string AllianceCandidatesKey = "candidates";
        const string AllianceScoreKey = "score";
        // TODO duplicated
        const string AllianceRequirementKey = "minPowerToJoin";
        const string AllianceRequirementMinLevelKey = "minLevel";
        const string AllianceRequirementScore = "minimum_score";
        const string AllianceActivityIndicatorKey = "activityIndicator";
        const string AllianceIsNewKey = "newAlliance";

        #endregion

        public IRankManager Ranks;

        public SocialPlayerFactory PlayerFactory{ protected get; set; }

        public AllianceBasicData CreateBasicData(Alliance alliance)
        {
            var data = CreateCustomAllianceBasicData();
            ParseAllianceBasicData(data, alliance);
            ParseCustomAllianceBasicData(data, alliance);
            return data;
        }

        public AllianceBasicData CreateBasicData(AttrDic dic)
        {
            var data = CreateCustomAllianceBasicData();
            ParseAllianceBasicData(data, dic);
            ParseCustomAllianceBasicData(data, dic);
            return data;
        }

        static void ParseAllianceBasicData(AllianceBasicData data, AttrDic dic)
        {
            data.Id = dic.GetValue(AllianceIdKey).ToString();
            data.Name = dic.GetValue(AllianceNameKey).ToString();
            data.Avatar = dic.GetValue(AllianceAvatarKey).ToInt();
            data.AccessType = dic.GetValue(AllianceTypeKey).ToInt();
            data.Members = dic.GetValue(AllianceTotalMembersKey).ToInt();
            data.Candidates = dic.GetValue(AllianceTotalRequestsKey).ToInt();
            data.Score = dic.GetValue(AllianceScoreKey).ToInt();
            data.Requirement = dic.GetValue(AllianceRequirementKey).ToInt();
            data.ActivityIndicator = dic.GetValue(AllianceActivityIndicatorKey).ToInt();
            data.IsNewAlliance = dic.GetValue(AllianceIsNewKey).ToBool();
        }

        static void ParseAllianceBasicData(AllianceBasicData data, Alliance alliance)
        {
            data.Id = alliance.Id;
            data.Name = alliance.Name;
            data.Avatar = alliance.Avatar;
            data.AccessType = alliance.AccessType;
            data.Members = alliance.Members;
            data.Candidates = alliance.Candidates;
            data.Score = alliance.Score;
            data.Requirement = alliance.Requirement;
            data.ActivityIndicator = alliance.ActivityIndicator;
            data.IsNewAlliance = alliance.IsNewAlliance;
        }

        public Alliance CreateAlliance(string allianceId, AttrDic dic)
        {
            var alliance = CreateCustomAlliance();
            alliance.Id = allianceId;

            ParseAlliance(alliance, dic);
            ParseCustomAlliance(alliance, dic);
            return alliance;
        }

        void ParseAlliance(Alliance alliance, AttrDic dic)
        {
            var data = dic.Get(AllianceInfoKey).AsDic;
            alliance.Name = data.GetValue(AllianceNameKey).ToString();
            alliance.Description = data.GetValue(AllianceDescriptionKey).ToString();
            alliance.Message = data.GetValue(AllianceMessageKey).ToString();
            alliance.Requirement = data.GetValue(AllianceRequirementMinLevelKey).ToInt();
            alliance.AccessType = data.GetValue(AllianceTypeAccessKey).ToInt();
            alliance.Avatar = data.ContainsKey(AllianceAvatarIdKey) ? data.GetValue(AllianceAvatarIdKey).ToInt() : 1;
            alliance.ActivityIndicator = data.GetValue(AllianceActivityIndicatorKey).ToInt();
            alliance.IsNewAlliance = data.GetValue(AllianceIsNewKey).ToBool();

            // Add candidates 
            var candidatesList = data.Get(AllianceCandidatesKey).AsList;
            if(candidatesList.Count > 0)
            {
                var candidates = new List<SocialPlayer>();
                candidates.Capacity = candidatesList.Count;

                for(var i = 0; i < candidatesList.Count; ++i)
                {
                    var candidate = PlayerFactory.CreateSocialPlayer(candidatesList[i].AsDic);
                    var component = candidate.GetComponent<AlliancePlayerBasic>();
                    if(component != null)
                    {
                        component.Rank = Ranks.DefaultRank;
                        component.Id = alliance.Id;
                        component.Name = alliance.Name;
                        component.Avatar = alliance.Avatar;
                    }

                    candidates.Add(candidate);
                }
                alliance.AddCandidates(candidates);
            }

            // Add alliance members
            var membersList = data.Get(AllianceMembersKey).AsList;
            var members = new List<SocialPlayer>();
            members.Capacity = membersList.Count;

            for(var i = 0; i < membersList.Count; ++i)
            {
                var memberDic = membersList[i].AsDic;
                var member = PlayerFactory.CreateSocialPlayer(memberDic);
                var component = member.GetComponent<AlliancePlayerBasic>();
                if(component != null)
                {
                    component.Id = alliance.Id;
                    component.Name = alliance.Name;
                    component.Avatar = alliance.Avatar;
                }
                members.Add(member);
            }
            alliance.AddMembers(members);
        }

        public AttrDic SerializeAlliance(Alliance alliance)
        {
            return SerializeAlliance(null, alliance);
        }

        public AttrDic SerializeAlliance(Alliance baseAlliance, Alliance modifiedAlliance)
        {
            var dic = new AttrDic();
            SerializeAllianceDiff(baseAlliance, modifiedAlliance, dic);
            SerializeCustomAllianceDiff(baseAlliance, modifiedAlliance, dic);
            return dic;
        }

        void SerializeAllianceDiff(Alliance baseAlliance, Alliance modifiedAlliance, AttrDic dic)
        {
            AddStringDiff(dic, AllianceNameKey, baseAlliance != null ? baseAlliance.Name : null, modifiedAlliance.Name);
            AddStringDiff(dic, AllianceDescriptionKey, baseAlliance != null ? baseAlliance.Description : null, modifiedAlliance.Description);
            AddStringDiff(dic, AllianceMessageKey, baseAlliance != null ? baseAlliance.Message : null, modifiedAlliance.Message);
            AddIntDiff(dic, AllianceRequirementScore, baseAlliance != null ? baseAlliance.Requirement : -1, modifiedAlliance.Requirement);
            AddIntDiff(dic, AllianceTypeKey, baseAlliance != null ? baseAlliance.AccessType : -1, modifiedAlliance.AccessType);
            AddIntDiff(dic, AllianceAvatarKey, baseAlliance != null ? baseAlliance.Avatar : -1, modifiedAlliance.Avatar);
        }

        protected void AddStringDiff(AttrDic dic, string key, string currentData, string newData)
        {
            if(currentData != newData)
            {
                dic.SetValue(key, newData);
            }
        }

        protected void AddIntDiff(AttrDic dic, string key, int currentData, int newData)
        {
            if(currentData != newData)
            {
                dic.SetValue(key, newData);
            }
        }

        public void UpdateAllianceData(Alliance baseAlliance, Alliance modifiedAlliance)
        {
            baseAlliance.Name = modifiedAlliance.Name;
            baseAlliance.Description = modifiedAlliance.Description;
            baseAlliance.Avatar = modifiedAlliance.Avatar;
            baseAlliance.AccessType = modifiedAlliance.AccessType;
            baseAlliance.Requirement = modifiedAlliance.Requirement;

            UpdateCustomAllianceData(baseAlliance, modifiedAlliance);
        }

        public AlliancesRanking CreateRankingData(AttrDic dic)
        {
            var ranking = CreateCustomRanking();
            ParseRankingData(ranking, dic);
            ParseCustomRanking(ranking, dic);
            return ranking;
        }

        public void ParseRankingData(AlliancesRanking ranking, AttrDic dic)
        {
            var rankDir = dic.Get(RankingKey).AsDic;
            var itr = rankDir.GetEnumerator();
            while(itr.MoveNext())
            {
                var el = itr.Current;
                var info = CreateBasicData(el.Value.AsDic);
                ranking.Add(info);
            }
            itr.Dispose();

            if(dic.ContainsKey(RankingMeKey))
            {
                var meData = dic.Get(RankingMeKey).AsDic;
                ranking.PlayerAlliancePosition = meData.GetValue(RankingRankKey).ToInt();
                ranking.PlayerAllianceData = CreateBasicData(meData);
            }

            ranking.Score = dic.GetValue(RankingScoreKey).ToInt();
        }

        public AttrDic SerializeSearchData(AlliancesSearch search)
        {
            var dic = new AttrDic();
            SerializeSearchData(search, dic);
            SerializeCustomSearch(search, dic);
            return dic;
        }

        static void SerializeSearchData(AlliancesSearch search, AttrDic dic)
        {
            dic.SetValue(SearchFilterKey, search.Filter);
        }

        public AlliancesSearchResult CreateSearchResultData(AttrDic dic)
        {
            var search = CreateCustomSearchResult();
            ParseSearchResultData(search, dic);
            ParseCustomSearchResult(search, dic);
            return search;
        }

        public void ParseSearchResultData(AlliancesSearchResult search, AttrDic dic)
        {
            var list = dic.Get(SearchKey).AsList;
            for(var i = 0; i < list.Count; ++i)
            {
                var el = list[i];
                search.Add(CreateBasicData(el.AsDic));
            }
            search.Score = dic.GetValue(SearchScoreKey).ToInt();
        }

        #region Extensible Alliance data

        protected virtual AllianceBasicData CreateCustomAllianceBasicData()
        {
            return new AllianceBasicData();
        }

        protected virtual void ParseCustomAllianceBasicData(AllianceBasicData data, AttrDic dic)
        {
        }

        protected virtual void ParseCustomAllianceBasicData(AllianceBasicData data, Alliance alliance)
        {
        }

        protected virtual Alliance CreateCustomAlliance()
        {
            return new Alliance();
        }

        protected virtual void ParseCustomAlliance(Alliance alliance, AttrDic dic)
        {
        }

        protected virtual void SerializeCustomAllianceDiff(Alliance baseAlliance, Alliance modifiedAlliance, AttrDic dic)
        {
        }

        protected virtual AlliancesRanking CreateCustomRanking()
        {
            return new AlliancesRanking();
        }

        protected virtual void ParseCustomRanking(AlliancesRanking ranking, AttrDic dic)
        {
        }

        protected virtual void SerializeCustomSearch(AlliancesSearch search, AttrDic dic)
        {
        }

        protected virtual AlliancesSearchResult CreateCustomSearchResult()
        {
            return new AlliancesSearchResult();
        }

        protected virtual void ParseCustomSearchResult(AlliancesSearchResult search, AttrDic dic)
        {
        }

        protected virtual void UpdateCustomAllianceData(Alliance baseAlliance, Alliance modifiedAlliance)
        {
        }

        #endregion
    }
}
