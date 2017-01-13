using System.Collections.Generic;
using SocialPoint.Attributes;

namespace SocialPoint.Social
{
    /// <summary>
    /// Common Alliance Member Data.
    /// This class represents the minimal Alliance Member Data, used in list like
    /// user search or Alliance member and candidates lists
    /// </summary>
    public class AlliancePlayerBasic : SocialPlayer.IComponent
    {
        public string Id;
        public string Name;
        public int Avatar;
        public int Rank;

        public bool IsInAlliance()
        {
            return !string.IsNullOrEmpty(Id);
        }

        public void ClearInfo()
        {
            Id = string.Empty;
            Name = string.Empty;
            Avatar = 0;
            Rank = 0;
        }
    }

    public class AlliancePlayerBasicFactory : SocialPlayerFactory.IFactory
    {
        const string MemberTypeKey = "memberType";
        const string MemberAllianceIdKey = "allianceId";
        const string MemberAllianceNameKey = "allianceName";
        const string MemberAllianceAvatarKey = "allianceAvatarId";

        // TODO: Duplicated ones used for parsing the old AlliancePlayerInfo
        const string AlliancePlayerIdKey = "id";
        const string AlliancePlayerNameKey = "name";
        const string AlliancePlayerAvatarKey = "avatar";
        const string AlliancePlayerRoleKey = "role";

        public SocialPlayer.IComponent CreateElement(AttrDic dic)
        {
            var component = new AlliancePlayerBasic();
            component.Rank = dic.GetValue(MemberTypeKey).ToInt();
            component.Id = dic.GetValue(MemberAllianceIdKey).ToString();
            component.Avatar = dic.GetValue(MemberAllianceAvatarKey).ToInt();
            component.Name = dic.GetValue(MemberAllianceNameKey).ToString();

            //TODO Temporal code to parse the old values
            if(dic.ContainsKey(AlliancePlayerIdKey))
            {
                component.Id = dic.GetValue(AlliancePlayerIdKey).ToString();
            }
            if(dic.ContainsKey(AlliancePlayerNameKey))
            {
                component.Name = dic.GetValue(AlliancePlayerNameKey).ToString();
            }
            if(dic.ContainsKey(AlliancePlayerAvatarKey))
            {
                component.Avatar = dic.GetValue(AlliancePlayerAvatarKey).ToInt();
            }
            if(dic.ContainsKey(AlliancePlayerRoleKey))
            {
                component.Rank = dic.GetValue(AlliancePlayerRoleKey).ToInt();
            }
            return component;
        }
    }

    /// <summary>
    /// Alliance data summary for the current user
    /// </summary>
    public class AlliancePlayerPrivate : SocialPlayer.IComponent
    {
        public int TotalMembers { get; set; }
        public long JoinTimestamp { get; set; }
        public int MaxRequests { get; set; }

        readonly Queue<string> _alliancesRequests;

        public AlliancePlayerPrivate()
        {
            _alliancesRequests = new Queue<string>();
            MaxRequests = 20;
        }

        public bool HasRequest(string id)
        {
            return _alliancesRequests.Contains(id);
        }

        public void AddRequest(string id)
        {
            if(_alliancesRequests.Count > MaxRequests)
            {
                _alliancesRequests.Dequeue();
            }
            _alliancesRequests.Enqueue(id);
        }

        public void ClearInfo()
        {
            JoinTimestamp = 0;
            TotalMembers = 0;
        }

        public void ClearRequests()
        {
            _alliancesRequests.Clear();
        }

        public void IncreaseTotalMembers()
        {
            TotalMembers++;
        }

        public void DecreaseTotalMembers()
        {
            TotalMembers--;
        }
    }

    public class AlliancePlayerPrivateFactory : SocialPlayerFactory.IFactory
    {
        const string AlliancePlayerTotalMembersKey = "total_members";
        const string AlliancePlayerJoinTimestampKey = "join_ts";
        const string AlliancePlayerRequestsKey = "requests";

        public SocialPlayer.IComponent CreateElement(AttrDic dic)
        {
            var component = new AlliancePlayerPrivate();

            component.TotalMembers = dic.GetValue(AlliancePlayerTotalMembersKey).ToInt();
            component.JoinTimestamp = dic.GetValue(AlliancePlayerJoinTimestampKey).ToLong();

            if(dic.ContainsKey(AlliancePlayerRequestsKey))
            {
                var list = dic.Get(AlliancePlayerRequestsKey).AsList;
                for(var i = 0; i < list.Count; ++i)
                {
                    var req = list[i].AsValue.ToString();
                    component.AddRequest(req);
                }
            }
            return component;
        }
    }
}