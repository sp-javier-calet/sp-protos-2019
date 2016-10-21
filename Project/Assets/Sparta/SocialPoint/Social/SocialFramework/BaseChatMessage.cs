namespace SocialPoint.Social
{
    public interface IChatMessage
    {
        string Uuid { get; set; }
        string PlayerId { get; set; }
        string PlayerName { get; set; }
        string Text { get; set; }
        long Timestamp { get; set; }

        bool IsWarning { get; set; }
        bool HasAlliance { get; set; }
        bool IsSending { get; set; }

        string AllianceName { get; set; }
        string AllianceId { get; set; }
        int RankInAlliance { get; set; }
        int PlayerLevel { get; set; }
        int AllianceAvatarId { get; set; }
    }
    /*
    public class ManagedChatMessage : IChatMessage
    {
        public string Uuid { get; set; }
        public string PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string Text { get;  set; }
        public long Timestamp { get;  set; }

        public bool IsWarning { get; set; }
        public bool HasAlliance { get; set; }
        public bool IsSending { get;  set; }

        public string AllianceName { get; set; }
        public string AllianceId { get; set; }
        public int RankInAlliance { get; set; }
        public int PlayerLevel { get; set; }
        public int AllianceAvatarId { get; set; }

    }
    */
}
