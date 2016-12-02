using System.Collections.Generic;

namespace SocialPoint.Social
{
    /// <summary>
    /// Alliance data summary for the current user
    /// </summary>
    public class AlliancePlayerInfo
    {
        /// <summary>
        /// Current player's alliance Id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Current player's alliance name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Current player's avatar
        /// </summary>
        public int Avatar { get; set; }

        /// <summary>
        /// Current player's rank in the alliance
        /// </summary>
        public int Rank;

        /// <summary>
        /// Number of member of the current player's alliance
        /// </summary>
        public int TotalMembers { get; set; }

        /// <summary>
        /// Player join timestamp
        /// </summary>
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
            Rank = 0;
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