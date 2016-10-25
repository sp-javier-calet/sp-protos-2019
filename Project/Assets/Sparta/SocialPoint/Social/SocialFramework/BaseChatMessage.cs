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
        long PlayerLevel { get; set; }
        int AllianceAvatarId { get; set; }
    }
}
