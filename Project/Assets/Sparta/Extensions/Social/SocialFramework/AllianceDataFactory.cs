using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    public class AllianceDataFactory
    {
        public AllianceMember CreateMember(AttrDic dic)
        {
            var member = CreateCustomMember();
            ParseMember(member, dic);
            ParseCustomMember(member, dic);
            return member;
        }

        void ParseMember(AllianceMember member, AttrDic dic)
        {
            member.Id = dic.GetValue("id").ToString();
            member.Name = dic.GetValue("name").ToString();
            member.Level = dic.GetValue("level").ToInt();
            member.Score = dic.GetValue("power").ToInt();
            member.Type = AllianceUtils.GetMemberTypeFromIndex(dic.GetValue("memberType").ToInt());

            if(dic.ContainsKey("allianceName"))
            {
                member.AllianceId = dic.GetValue("allianceId").ToString();
                member.AvatarId = dic.GetValue("allianceAvatarId").ToInt();
                member.AllianceName = dic.GetValue("allianceName").ToString();
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
            data.Id = dic.GetValue("id").ToString();
            data.Name = dic.GetValue("name").ToString();
            data.AvatarId = dic.GetValue("avatar").ToInt();
            data.AccessType = dic.GetValue("type").ToBool() ? AllianceAccessType.Private : AllianceAccessType.Open;
            data.MemberCount = dic.GetValue("member_count").ToInt();
            data.RequestCount = dic.GetValue("requests").ToInt();
            data.Score = dic.GetValue("score").ToInt();
            data.ScoreToJoin = dic.GetValue("minPowerToJoin").ToInt();
            data.ActivityIndicator = dic.GetValue("activityIndicator").ToInt();
            data.IsNewAlliance = dic.GetValue("newAlliance").ToBool();
        }

        void ParseBasicData(AllianceBasicData data, Alliance alliance)
        {
            data.Id = alliance.Id;
            data.Name = alliance.Name;
            data.AvatarId = alliance.AvatarId;
            data.AccessType = alliance.AccessType;
            data.MemberCount = alliance.Members;
            data.RequestCount = alliance.Candidates;
            data.Score = alliance.Score;
            data.ScoreToJoin = alliance.MinScoreToJoin;
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
            var data = dic.Get("alliance_info").AsDic;
            alliance.Name = data.GetValue("name").ToString();
            alliance.Description = data.GetValue("description").ToString();
            alliance.MinScoreToJoin = data.GetValue("minLevel").ToInt();
            alliance.AccessType = AllianceUtils.GetAllianceTypeFromIndex(data.GetValue("allianceType").ToInt());
            alliance.AvatarId = data.ContainsKey("avatarId") ? data.GetValue("avatarId").ToInt() : 1;
            alliance.ActivityIndicator = data.GetValue("activityIndicator").ToInt();
            alliance.IsNewAlliance = data.GetValue("newAlliance").ToBool();

            // Add candidates for private alliances
            if(alliance.AccessType == AllianceAccessType.Private)
            {
                var candidatesList = dic.Get("candidates").AsList;
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
            var membersList = dic.Get("members").AsList;
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
            if(dic.ContainsKey("id"))
            {
                info.Id = dic.GetValue("id").ToString();
                info.Name = dic.GetValue("name").ToString();
                info.AvatarId = dic.GetValue("avatar").ToInt();
                info.MemberType = AllianceUtils.GetMemberTypeFromIndex(dic.GetValue("role").ToInt());
                info.TotalMembers = dic.GetValue("total_members").ToInt();
                info.JoinTimestamp = dic.GetValue("join_ts").ToLong();
            }

            if(dic.ContainsKey("requests"))
            {
                var list = dic.Get("requests").AsList;
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

            var rankDir = dic.Get("ranking").AsDic;
            for(var i = 0; i < rankDir.Count; ++i)
            {
                var el = rankDir.ElementAt(i);
                var info = CreateBasicData(el.Value.AsDic);
                ranking.Add(info);
            }

            if(dic.ContainsKey("me"))
            {
                var meData = dic.Get("child").AsDic;
                ranking.PlayerAlliancePosition = meData.GetValue("rank").ToInt();
                ranking.PlayerAllianceData = CreateBasicData(meData);
            }

            ranking.PlayerScore = dic.GetValue("monster_power").ToInt(); // TODO MONSTER :( score?
            return ranking;
        }

        public AlliancesSearchData CreateSearchData(AttrDic dic, bool suggested)
        {
            var search = new AlliancesSearchData();
            var alliancesKey = suggested ? "suggested" : "search";
            var list = dic.Get(alliancesKey).AsList;
            for(var i = 0; i < list.Count; ++i)
            {
                var el = list[i];
                search.Add(CreateBasicData(el.AsDic));
            }
            search.PlayerScore = dic.GetValue("monster_power").ToInt();
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
            return new AllianceBasicData(); // TODO
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
