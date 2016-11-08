using System.Collections.Generic;

namespace SocialPoint.Social
{
    public class AlliancePlayerInfo
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public int Avatar { get; set; }

        public AllianceMemberType MemberType = AllianceMemberType.Soldier;

        public int TotalMembers { get; set; }

        public long JoinTimestamp { get; set; }

        public bool IsInAlliance
        {
            get
            {
                return !string.IsNullOrEmpty(Id);
            }
        }

        readonly Queue<string> _alliancesRequests;

        public AlliancePlayerInfo()
        {
            _alliancesRequests = new Queue<string>();
        }

        public bool HasRequest(string id)
        {
            return _alliancesRequests.Contains(id);
        }

        public void AddRequest(string id, uint maxRequests)
        {
            if(_alliancesRequests.Count > maxRequests)
            {
                _alliancesRequests.Dequeue();
            }
            _alliancesRequests.Enqueue(id);
        }

        public void ClearInfo()
        {
            Id = string.Empty;
            Name = string.Empty;
            JoinTimestamp = 0;
            Avatar = 0;
            TotalMembers = 0;
            MemberType = AllianceMemberType.Soldier;
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
}