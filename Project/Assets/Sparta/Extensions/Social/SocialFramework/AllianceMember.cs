namespace SocialPoint.Social
{
    public enum AllianceMemberType
    {
        Lead = 1,
        CoLead,
        Soldier,
        Undefined
    }

    public class AllianceMember
    {
        public string Id;

        public string Name;

        public int Level;

        public int AvatarId;

        public AllianceMemberType Type;

        public int Rank;

        public int Score;

        public bool IsInAlliance;

        public string AllianceId;
       
        public string AllianceName;

        public int AllianceAvatarId;

    }
}
