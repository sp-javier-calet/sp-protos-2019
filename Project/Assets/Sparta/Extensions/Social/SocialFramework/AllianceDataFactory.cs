using System.Collections.Generic;
using SocialPoint.Attributes;

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
        const string RankingScoreKey = "monster_power";  // TODO MONSTER :( score?

        const string SearchKey = "search";
        const string SearchSuggestedKey = "suggested";
        const string SearchScoreKey = "monster_power";

        const string AllianceInfoKey = "alliance_info";
        const string AllianceIdKey = "id";
        const string AllianceNameKey = "name";
        const string AllianceDescriptionKey = "description";
        const string AllianceAvatarKey = "avatar";
        const string AllianceAvatarIdKey = "avatarId"; // TODO duplicated
        const string AllianceTypeKey = "type";
        const string AllianceTypeAccessKey = "allianceType"; // TODO duplicated
        const string AllianceTotalMembersKey = "member_count";
        const string AllianceTotalRequestsKey = "requests";
        const string AllianceMembersKey = "members";
        const string AllianceCandidatesKey = "candidates";
        const string AllianceScoreKey = "score";
        const string AllianceRequirementKey = "minPowerToJoin";
        const string AllianceRequirementMinLevelKey = "minLevel"; // TODO duplicated
        const string AllianceActivityIndicatorKey = "activityIndicator";
        const string AllianceIsNewKey = "newAlliance";

        #endregion

        public AllianceMember CreateMember(AttrDic dic)
        {
            var member = CreateCustomMember();
            ParseMember(member, dic);
            ParseCustomMember(member, dic);
            return member;
        }

        void ParseMember(AllianceMember member, AttrDic dic)
        {
            member.Uid = dic.GetValue(MemberUidKey).ToString();
            member.Name = dic.GetValue(MemberNameKey).ToString();
            member.Level = dic.GetValue(MemberLevelKey).ToInt();
            member.Score = dic.GetValue(MemberScoreKey).ToInt();
            member.Type = AllianceUtils.GetMemberTypeFromIndex(dic.GetValue(MemberTypeKey).ToInt());

            if(dic.ContainsKey(MemberAllianceNameKey))
            {
                member.AllianceId = dic.GetValue(MemberAllianceIdKey).ToString();
                member.AllianceAvatar = dic.GetValue(MemberAllianceAvatarKey).ToInt();
                member.AllianceName = dic.GetValue(MemberAllianceNameKey).ToString();
            }
        }

        public AllianceBasicData CreateBasicData(Alliance alliance)
        {
            var data = CreateCustomBasicData();
            ParseBasicData(data, alliance);
            ParseCustomBasicData(data, alliance);
            return data;
        }

        public AllianceBasicData CreateBasicData(AttrDic dic)
        {
            var data = CreateCustomBasicData();
            ParseBasicData(data, dic);
            ParseCustomBasicData(data, dic);
            return data;
        }

        void ParseBasicData(AllianceBasicData data, AttrDic dic)
        {
            data.Id = dic.GetValue(AllianceIdKey).ToString();
            data.Name = dic.GetValue(AllianceNameKey).ToString();
            data.Avatar = dic.GetValue(AllianceAvatarKey).ToInt();
            data.AccessType = dic.GetValue(AllianceTypeKey).ToBool() ? AllianceAccessType.Private : AllianceAccessType.Open;
            data.Members = dic.GetValue(AllianceTotalMembersKey).ToInt();
            data.Requests = dic.GetValue(AllianceTotalRequestsKey).ToInt();
            data.Score = dic.GetValue(AllianceScoreKey).ToInt();
            data.Requirement = dic.GetValue(AllianceRequirementKey).ToInt();
            data.ActivityIndicator = dic.GetValue(AllianceActivityIndicatorKey).ToInt();
            data.IsNewAlliance = dic.GetValue(AllianceIsNewKey).ToBool();
        }

        void ParseBasicData(AllianceBasicData data, Alliance alliance)
        {
            data.Id = alliance.Id;
            data.Name = alliance.Name;
            data.Avatar = alliance.Avatar;
            data.AccessType = alliance.Type;
            data.Members = alliance.Members;
            data.Requests = alliance.Candidates;
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
            alliance.Requirement = data.GetValue(AllianceRequirementMinLevelKey).ToInt();
            alliance.Type = AllianceUtils.GetAllianceTypeFromIndex(data.GetValue(AllianceTypeAccessKey).ToInt());
            alliance.Avatar = data.ContainsKey(AllianceAvatarIdKey) ? data.GetValue(AllianceAvatarIdKey).ToInt() : 1;
            alliance.ActivityIndicator = data.GetValue(AllianceActivityIndicatorKey).ToInt();
            alliance.IsNewAlliance = data.GetValue(AllianceIsNewKey).ToBool();

            // Add candidates for private alliances
            if(alliance.Type == AllianceAccessType.Private)
            {
                var candidatesList = dic.Get(AllianceCandidatesKey).AsList;
                var candidates = new List<AllianceMember>();
                candidates.Capacity = candidatesList.Count;

                for(var i = 0; i < candidatesList.Count; ++i)
                {
                    var candidateDic = candidatesList[i].AsDic;
                    var candidate = CreateMember(candidateDic);
                    candidate.Type = AllianceMemberType.Soldier;

                    candidates.Add(candidate);
                }
                alliance.AddCandidates(candidates);
            }

            // Add alliance members
            var membersList = dic.Get(AllianceMembersKey).AsList;
            var members = new List<AllianceMember>();
            members.Capacity = membersList.Count;

            for(var i = 0; i < membersList.Count; ++i)
            {
                var memberDic = membersList[i].AsDic;
                var member = CreateMember(memberDic);
                members.Add(member);
            }
            alliance.AddMembers(members);

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
                info.MemberType = AllianceUtils.GetMemberTypeFromIndex(dic.GetValue(AlliancePlayerRoleKey).ToInt());
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

        public AllianceRankingData CreateRankingData(AttrDic dic)
        {
            var ranking = new AllianceRankingData();

            var rankDir = dic.Get(RankingKey).AsDic;
            for(var i = 0; i < rankDir.Count; ++i)
            {
                var el = rankDir.ElementAt(i);
                var info = CreateBasicData(el.Value.AsDic);
                ranking.Add(info);
            }

            if(dic.ContainsKey(RankingMeKey))
            {
                var meData = dic.Get(RankingMeKey).AsDic;
                ranking.PlayerAlliancePosition = meData.GetValue(RankingRankKey).ToInt();
                ranking.PlayerAllianceData = CreateBasicData(meData);
            }

            ranking.Score = dic.GetValue(RankingScoreKey).ToInt();
            return ranking;
        }

        public AlliancesSearchData CreateSearchData(AttrDic dic, bool suggested)
        {
            var search = new AlliancesSearchData();
            var alliancesKey = suggested ? SearchSuggestedKey : SearchKey;
            var list = dic.Get(alliancesKey).AsList;
            for(var i = 0; i < list.Count; ++i)
            {
                var el = list[i];
                search.Add(CreateBasicData(el.AsDic));
            }
            search.Score = dic.GetValue(SearchScoreKey).ToInt();
            return search;
        }

        public AlliancesSearchData CreateJoinData(AttrDic dic)
        {
            return CreateSearchData(dic, true);
        }

        #region Extensible Alliance data

        protected virtual AllianceMember CreateCustomMember()
        {
            return new AllianceMember();
        }

        protected virtual void ParseCustomMember(AllianceMember member, AttrDic dic)
        {
        }

        protected virtual AllianceBasicData CreateCustomBasicData()
        {
            return new AllianceBasicData();
        }

        protected virtual void ParseCustomBasicData(AllianceBasicData data, AttrDic dic)
        {
        }

        protected virtual void ParseCustomBasicData(AllianceBasicData data, Alliance alliance)
        {
        }

        protected virtual Alliance CreateCustomAlliance()
        {
            return new Alliance();
        }

        protected virtual void ParseCustomAlliance(Alliance alliance, AttrDic dic)
        {
        }

        protected virtual AlliancePlayerInfo CreateCustomPlayerInfo()
        {
            return new AlliancePlayerInfo();
        }

        protected virtual void ParseCustomPlayerInfo(AlliancePlayerInfo info, AttrDic dic)
        {
        }

        #endregion
    }
}
