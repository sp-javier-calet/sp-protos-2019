using SocialPoint.Base;

namespace SocialPoint.Social
{
    public enum RanksComparison
    {
        Higher,
        Lower,
        Equal
    }

    public static class AllianceUtils
    {
        const int AllianceTypeOpenIndex = 0;
        const int AllianceTypePrivateIndex = 1;
        const int MemberTypeLeadIndex = 1;
        const int MemberTypeColeadIndex = 2;
        const int MemberTypeMemberIndex = 3;

        public static int GetIndexForMemberType(AllianceMemberType type)
        {
            switch(type)
            {
            case AllianceMemberType.Lead:
                return MemberTypeLeadIndex;
            case AllianceMemberType.Colead:
                return MemberTypeColeadIndex;
            case AllianceMemberType.Member:
                return MemberTypeMemberIndex;
            default:
                DebugUtils.Assert(false, "Invalid AllianceMemberType");
                return 0;
            }
        }

        public static AllianceMemberType GetMemberTypeFromIndex(int index)
        {
            switch(index)
            {
            case MemberTypeLeadIndex:
                return AllianceMemberType.Lead;
            case MemberTypeColeadIndex:
                return AllianceMemberType.Colead;
            default:
                return AllianceMemberType.Member;
            }
        }

        public static AllianceAccessType GetAllianceTypeFromIndex(int index)
        {
            switch(index)
            {
            case AllianceTypeOpenIndex:
                return AllianceAccessType.Open;
            case AllianceTypePrivateIndex:
                return AllianceAccessType.Private;
            default:
                DebugUtils.Assert(false, "Invalid Alliance Type index");
                return AllianceAccessType.Private;
            }
        }

        public static string GetAllianceMemberTypeString(AllianceMemberType type)
        {
            switch(type)
            {
            case AllianceMemberType.Lead:
                return SocialFrameworkStrings.AllianceLeadNameKey;
            case AllianceMemberType.Colead:
                return SocialFrameworkStrings.AllianceColeadNameKey;
            case AllianceMemberType.Member:
                return SocialFrameworkStrings.AllianceMemberNameKey;
            default:
                DebugUtils.Assert(false, "Invalid AllianceMemberType");
                return string.Empty;
            }
        }

        public static string GetAlliancePromotionTypeString(RanksComparison comparison)
        {
            switch(comparison)
            {
            case RanksComparison.Higher:
                return SocialFrameworkStrings.ChatPlayerPromotedKey;
            case RanksComparison.Lower:
                return SocialFrameworkStrings.ChatPlayerDemotedRankKey;
            }
            return string.Empty;
        }

        public static RanksComparison CompareRanks(AllianceMemberType typeFrom, AllianceMemberType typeTo)
        {
            var diff = GetIndexForMemberType(typeFrom) - GetIndexForMemberType(typeTo);
            return (diff > 0) ? RanksComparison.Higher : (diff < 0) ? RanksComparison.Lower : RanksComparison.Equal;
        }
    }
}
