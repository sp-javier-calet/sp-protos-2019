using System.Collections.Generic;

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
        public int PlayerAlliancePosition;
        public int PlayerScore;
    }

    public class AlliancesSearchData
    {
        public int PlayerScore;
    }

    public class AlliancesCreateData
    {
        public string Name;
        public string Description;
        public int RequirementValue;
        public int AvatarId = 1;
        public AllianceAccessType AccessType = AllianceAccessType.Open;
    }

    public class AlliancePlayerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public int AvatarId { get; set; }
        public AllianceMemberType MemberType = AllianceMemberType.Soldier;
        public int TotalMembers { get; set; }
        public long JoinTimestamp { get; set; }

        public bool IsInAlliance
        {
            get
            {
                // TODO
                return false;
            }
        }

        readonly Stack<string> _alliancesRequests;

        public AlliancePlayerInfo()
        {
        }

        public bool HasRequest(string allianceId)
        {
            // TODO
            return false;
        }

        public void AddRequest(string id, uint maxRequests)
        {
        }

        public void ClearInfo()
        {
            
        }

        public void ClearRequests()
        {
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
}
