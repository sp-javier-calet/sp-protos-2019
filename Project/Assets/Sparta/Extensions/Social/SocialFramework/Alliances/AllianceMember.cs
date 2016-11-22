namespace SocialPoint.Social
{
    /// <summary>
    /// Common Alliance Member Data.
    /// This class is only intended to be inherited by library classes,
    /// containing the common data for all of them.
    /// </summary>
    public abstract class AllianceMemberData
    {
        public string Uid;

        public string Name;

        public int Level;

        public int Score;

        public string AllianceId;

        public string AllianceName;

        public int AllianceAvatar;

        public int Rank;
    }

    /// <summary>
    /// Common Alliance Member Data.
    /// This class represents the minimal Alliance Member Data, used in list like
    /// user search or Alliance member and candidates lists
    /// </summary>
    public class AllianceMemberBasicData : AllianceMemberData
    {
    }

    /// <summary>
    /// Alliance Member
    /// Complete Alliance Member Data 
    /// </summary>
    public class AllianceMember : AllianceMemberData
    {
    }
}