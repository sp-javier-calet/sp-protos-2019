namespace SocialPoint.Social
{
    public interface IChatMessage
    {
        string Uuid { get; }
        string PlayerId { get; }
        string PlayerName { get; }
        string Text { get; }
        long Timestamp { get; }

        bool IsWarning { get; }
        bool HasAlliance { get; }
        bool IsSending { get; }

        string AllianceName { get; }
        string AllianceId { get; }
        int RankInAlliance { get; }
        int PlayerLevel { get; }
        int AllianceAvatarId { get; }
    }

    public class BaseChatMessage
    {
        public string Uuid { get; protected set; }
        public string PlayerId { get; protected set; }
        public string PlayerName { get; protected set; }
        public string Text { get; protected set; }
        public long Timestamp { get; protected set; }

        public bool IsWarning { get; protected set; }
        public bool HasAlliance { get; protected set; }
        public bool IsSending { get; protected set; }

        public string AllianceName { get; protected set; }
        public string AllianceId { get; protected set; }
        public int RankInAlliance { get; protected set; }
        public int PlayerLevel { get; protected set; }
        public int AllianceAvatarId { get; protected set; }

        public BaseChatMessage(string text)
        {
            Text = text;
        }
    }
}
