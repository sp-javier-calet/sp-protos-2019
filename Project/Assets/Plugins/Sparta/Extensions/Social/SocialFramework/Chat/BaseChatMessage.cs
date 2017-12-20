namespace SocialPoint.Social
{
    /// <summary>
    /// Stores common chat messages data
    /// </summary>
    public class MessageData
    {
        public string PlayerId { get; set; }

        public string PlayerName { get; set; }

        public string AllianceName { get; set; }

        public string AllianceId { get; set; }

        public int RankInAlliance { get; set; }

        public int PlayerLevel { get; set; }

        public int AllianceAvatarId { get; set; }

        public bool HasAlliance
        {
            get
            {
                return !string.IsNullOrEmpty(AllianceId);
            }
        }
    }

    /// <summary>
    /// Stores Member Promotion and Demotion messages information
    /// </summary>
    public class MemberPromotionData
    {
        public string PlayerName { get; set; }

        public int OldRank { get; set; }

        public int NewRank { get; set; }
    }

    // <summary>
    // Stores information for requested joins
    // </summary>
    public class RequestJoinData
    {
        public string PlayerId { get; set; }
    }

    public interface IChatMessage
    {
        string Uuid { get; set; }

        int Type { get; set; }

        bool IsWarning { get; set; }

        bool IsSending { get; set; }

        string Text { get; set; }

        long Timestamp { get; set; }

        MessageData MessageData { get; set; }

        MemberPromotionData MemberPromotionData { get; set; }

        RequestJoinData RequestJoinData { get; set; }
    }

    public class BaseChatMessage : IChatMessage
    {
        public string Uuid { get; set; }

        public int Type { get; set; }

        public bool IsWarning { get; set; }

        public bool IsSending { get; set; }

        public string Text { get; set; }

        public long Timestamp { get; set; }

        public MessageData MessageData { get; set; }

        public MemberPromotionData MemberPromotionData { get; set; }

        public RequestJoinData RequestJoinData { get; set; }
    }
}
