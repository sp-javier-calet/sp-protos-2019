using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public class AllianceDataFactory
    {
        #region Attr keys

        const string MemberUidKey = "id";
        const string MemberNameKey = "name";
        const string MemberLevelKey = "level";
        const string MemberScoreKey = "power";
        const string MemberTypeKey = "memberType";
        const string MemberAllianceIdKey = "allianceId";
        const string MemberAllianceNameKey = "allianceName";
        const string MemberAllianceAvatarKey = "allianceAvatarId";

        const string AlliancePlayerIdKey = "id";
        const string AlliancePlayerNameKey = "name";
        const string AlliancePlayerAvatarKey = "avatar";
        const string AlliancePlayerRoleKey = "role";
        const string AlliancePlayerTotalMembersKey = "total_members";
        const string AlliancePlayerJoinTimestampKey = "join_ts";
        const string AlliancePlayerRequestsKey = "requests";

        const string RankingKey = "ranking";
        const string RankingMeKey = "me";
        const string RankingRankKey = "rank";
        // TODO MONSTER :( score?
        const string RankingScoreKey = "monster_power";

        const string SearchKey = "search";
        const string SearchSuggestedKey = "suggested";
        const string SearchScoreKey = "monster_power";
        const string SearchFilterKey = "filter_name";

        const string AllianceInfoKey = "alliance_info";
        const string AllianceIdKey = "id";
        const string AllianceNameKey = "name";
        const string AllianceDescriptionKey = "description";
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

        const string AllianceRequestIdKey = "alliance_id";
        const string AllianceRequestNameKey = "alliance_name";
        const string AllianceRequestAvatarKey = "alliance_symbol";
        const string AllianceRequestTotalMembersKey = "total_members";
        const string AllianceRequestJoinTimestampKey = "join_ts";

        #endregion

        public IRankManager Ranks;

        public AllianceMemberBasicData CreateMemberBasicData(AttrDic dic)
        {
            var member = CreateCustomAllianceMemberBasicData();
            ParseMemberData(member, dic);
            ParseCustomAllianceMemberBasicData(member, dic);
            return member;
        }

        public AllianceMember CreateMember(AttrDic dic)
        {
            var member = CreateCustomAllianceMember();
            ParseMemberData(member, dic);
            ParseCustomAllianceMember(member, dic);
            return member;
        }

        void ParseMemberData(AllianceMemberData member, AttrDic dic)
        {
            member.Uid = dic.GetValue(MemberUidKey).ToString();
            member.Name = dic.GetValue(MemberNameKey).ToString();
            member.Level = dic.GetValue(MemberLevelKey).ToInt();
            member.Score = dic.GetValue(MemberScoreKey).ToInt();
            member.Rank = dic.GetValue(MemberTypeKey).ToInt();

            if(dic.ContainsKey(MemberAllianceNameKey))
            {
                member.AllianceId = dic.GetValue(MemberAllianceIdKey).ToString();
                member.AllianceAvatar = dic.GetValue(MemberAllianceAvatarKey).ToInt();
                member.AllianceName = dic.GetValue(MemberAllianceNameKey).ToString();
            }
        }

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

        void ParseAllianceBasicData(AllianceBasicData data, AttrDic dic)
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

        void ParseAllianceBasicData(AllianceBasicData data, Alliance alliance)
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

        public Alliance CreateAlliance(string allianceId, int defaultMemberRank, AttrDic dic)
        {
            var alliance = CreateCustomAlliance();
            alliance.Id = allianceId;

            ParseAlliance(alliance, defaultMemberRank, dic);
            ParseCustomAlliance(alliance, dic);
            return alliance;
        }

        void ParseAlliance(Alliance alliance, int defaultMemberRank, AttrDic dic)
        {
            var data = dic.Get(AllianceInfoKey).AsDic;
            alliance.Name = data.GetValue(AllianceNameKey).ToString();
            alliance.Description = data.GetValue(AllianceDescriptionKey).ToString();
            alliance.Requirement = data.GetValue(AllianceRequirementMinLevelKey).ToInt();
            alliance.AccessType = data.GetValue(AllianceTypeAccessKey).ToInt();
            alliance.Avatar = data.ContainsKey(AllianceAvatarIdKey) ? data.GetValue(AllianceAvatarIdKey).ToInt() : 1;
            alliance.ActivityIndicator = data.GetValue(AllianceActivityIndicatorKey).ToInt();
            alliance.IsNewAlliance = data.GetValue(AllianceIsNewKey).ToBool();

            // Add candidates 
            var candidatesList = data.Get(AllianceCandidatesKey).AsList;
            if(candidatesList.Count > 0)
            {
                var candidates = new List<AllianceMemberBasicData>();
                candidates.Capacity = candidatesList.Count;

                for(var i = 0; i < candidatesList.Count; ++i)
                {
                    var candidateDic = candidatesList[i].AsDic;
                    var candidate = CreateMemberBasicData(candidateDic);
                    candidate.Rank = defaultMemberRank;

                    candidates.Add(candidate);
                }
                alliance.AddCandidates(candidates);
            }

            // Add alliance members
            var membersList = data.Get(AllianceMembersKey).AsList;
            var members = new List<AllianceMemberBasicData>();
            members.Capacity = membersList.Count;

            for(var i = 0; i < membersList.Count; ++i)
            {
                var memberDic = membersList[i].AsDic;
                var member = CreateMemberBasicData(memberDic);
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
            AddDiff(dic, AllianceNameKey, baseAlliance != null ? baseAlliance.Name : null, modifiedAlliance.Name);
            AddDiff(dic, AllianceDescriptionKey, baseAlliance != null ? baseAlliance.Description : null, modifiedAlliance.Description);
            AddDiff(dic, AllianceRequirementScore, baseAlliance != null ? baseAlliance.Requirement : -1, modifiedAlliance.Requirement);
            AddDiff(dic, AllianceTypeKey, baseAlliance != null ? baseAlliance.AccessType : -1, modifiedAlliance.AccessType);
            AddDiff(dic, AllianceAvatarKey, baseAlliance != null ? baseAlliance.Avatar : -1, modifiedAlliance.Avatar);
        }

        protected void AddDiff<T>(AttrDic dic, string key, T currentData, T newData) where T : System.IComparable
        {
            if(newData.CompareTo(currentData) != 0)
            {
                dic.SetValue(key, newData);
            }
        }

        public AlliancePlayerInfo CreatePlayerInfo()
        {
            return CreateCustomPlayerInfo();
        }

        public AlliancePlayerInfo CreatePlayerInfo(uint maxPendingJoinRequests, AttrDic dic)
        {
            var data = CreateCustomPlayerInfo();
            ParsePlayerInfo(data, maxPendingJoinRequests, dic);
            ParseCustomPlayerInfo(data, dic);
            return data;
        }

        void ParsePlayerInfo(AlliancePlayerInfo info, uint maxRequests, AttrDic dic)
        {
            if(dic.ContainsKey(AlliancePlayerIdKey))
            {
                info.Id = dic.GetValue(AlliancePlayerIdKey).ToString();
                info.Name = dic.GetValue(AlliancePlayerNameKey).ToString();
                info.Avatar = dic.GetValue(AlliancePlayerAvatarKey).ToInt();
                info.Rank = dic.GetValue(AlliancePlayerRoleKey).ToInt();
                info.TotalMembers = dic.GetValue(AlliancePlayerTotalMembersKey).ToInt();
                info.JoinTimestamp = dic.GetValue(AlliancePlayerJoinTimestampKey).ToLong();
            }

            if(dic.ContainsKey(AlliancePlayerRequestsKey))
            {
                var list = dic.Get(AlliancePlayerRequestsKey).AsList;
                for(var i = 0; i < list.Count; ++i)
                {
                    var req = list[i].AsValue.ToString();
                    info.AddRequest(req, maxRequests);
                }
            }
        }

        public void OnAllianceCreated(AlliancePlayerInfo info, Alliance data, AttrDic result)
        {
            DebugUtils.Assert(result.Get(AllianceIdKey).IsValue);
            var id = result.GetValue(AllianceIdKey).ToString();

            info.Id = id;
            info.Avatar = data.Avatar;
            info.Name = data.Name;
            info.Rank = Ranks.FounderRank;
            info.TotalMembers = 1;
            info.JoinTimestamp = TimeUtils.Timestamp;
            info.ClearRequests();

            OnCustomAllianceCreated(info, data, result);
        }

        public void OnAllianceJoined(AlliancePlayerInfo info, AllianceBasicData data, JoinExtraData extra)
        {
            info.Id = data.Id;
            info.Name = data.Name;
            info.Avatar = data.Avatar;
            info.Rank = Ranks.DefaultRank;
            info.TotalMembers = data.Members;
            info.JoinTimestamp = extra.Timestamp;
            info.ClearRequests();

            OnCustomAllianceJoined(info, data, extra);
        }

        public void OnAllianceRequestAccepted(AlliancePlayerInfo info, AttrDic dic)
        {
            DebugUtils.Assert(dic.GetValue(AllianceRequestIdKey).IsValue);
            var allianceId = dic.GetValue(AllianceRequestIdKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceRequestNameKey).IsValue);
            var allianceName = dic.GetValue(AllianceRequestNameKey).ToString();

            DebugUtils.Assert(dic.GetValue(AllianceRequestAvatarKey).IsValue);
            var avatarId = dic.GetValue(AllianceRequestAvatarKey).ToInt();

            var totalMembers = dic.GetValue(AllianceRequestTotalMembersKey).ToInt();
            var joinTs = dic.GetValue(AllianceRequestJoinTimestampKey).ToInt();

            info.Id = allianceId;
            info.Name = allianceName;
            info.Avatar = avatarId;
            info.Rank = Ranks.DefaultRank;
            info.TotalMembers = totalMembers;
            info.JoinTimestamp = joinTs;
            info.ClearRequests();

            OnCustomAllianceRequestAccepted(info, dic);
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

        void SerializeSearchData(AlliancesSearch search, AttrDic dic)
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

        protected virtual AllianceMemberBasicData CreateCustomAllianceMemberBasicData()
        {
            return new AllianceMemberBasicData();
        }

        protected virtual void ParseCustomAllianceMemberBasicData(AllianceMemberBasicData member, AttrDic dic)
        {
        }

        protected virtual AllianceMember CreateCustomAllianceMember()
        {
            return new AllianceMember();
        }

        protected virtual void ParseCustomAllianceMember(AllianceMember member, AttrDic dic)
        {
        }

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

        protected virtual AlliancePlayerInfo CreateCustomPlayerInfo()
        {
            return new AlliancePlayerInfo();
        }

        protected virtual void ParseCustomPlayerInfo(AlliancePlayerInfo info, AttrDic dic)
        {
        }

        protected virtual void OnCustomAllianceCreated(AlliancePlayerInfo info, Alliance data, AttrDic result)
        {
        }

        protected virtual void OnCustomAllianceJoined(AlliancePlayerInfo info, AllianceBasicData data, JoinExtraData extra)
        {
        }

        protected virtual void OnCustomAllianceRequestAccepted(AlliancePlayerInfo info, AttrDic dic)
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

        #endregion
    }
}
