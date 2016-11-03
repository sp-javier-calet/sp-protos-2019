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
        const int MemberTypeLeadIndex = 1;
        const int MemberTypeColeadIndex = 2;
        const int MemberTypeSoldierIndex = 3;

        public static int GetIndexForMemberType(AllianceMemberType type)
        {
            switch(type)
            {
            case AllianceMemberType.Lead:
                return MemberTypeLeadIndex;
            case AllianceMemberType.CoLead:
                return MemberTypeColeadIndex;
            case AllianceMemberType.Soldier:
                return MemberTypeSoldierIndex;
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
                return AllianceMemberType.CoLead;
            default:
                return AllianceMemberType.Soldier;
            }
        }

        public static AllianceAccessType GetAllianceTypeFromIndex(int index)
        {
            return AllianceAccessType.Open;
        }

        public static string GetAllianceMemberTypeString(AllianceMemberType type)
        {
            // TODO Localize strings
            switch(type)
            {
            case AllianceMemberType.Lead:
                return "socialFramework.AlliancesLead";
            case AllianceMemberType.CoLead:
                return "socialFramework.AlliancesColead";
            case AllianceMemberType.Soldier:
                return "socialFramework.AlliancesSoldier";
            default:
                DebugUtils.Assert(false, "Invalid AllianceMemberType");
                return string.Empty;
            }
        }

        public static RanksComparison CompareRanks(AllianceMemberType typeFrom, AllianceMemberType typeTo)
        {
            var diff = GetIndexForMemberType(typeFrom) - GetIndexForMemberType(typeTo);
            return (diff > 0) ? RanksComparison.Higher : (diff < 0) ? RanksComparison.Lower : RanksComparison.Equal;
        }
    }
}
