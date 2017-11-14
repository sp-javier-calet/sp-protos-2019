using System;

namespace SocialPoint.Social
{
    public enum RankPermission
    {
        EditAlliance,
        ManageCandidates
    }

    public interface IRankManager
    {
        /// <summary>
        /// Rank index for the founder member
        /// </summary>
        int FounderRank { get; }

        /// <summary>
        /// Default Rank index for a base member
        /// </summary>
        int DefaultRank { get; }

        /// <summary>
        /// Determines if a rank id exists
        /// </summary>
        bool Exists(int rank);

        bool CanChangeRank(int ownRank, int oldRank, int newRank);

        int GetPromotionRank(int rank);

        int GetDemotionRank(int rank);

        bool HasPermission(int rank, RankPermission permission);

        string GetRankChangeMessageTid(int oldRank, int newRank);

        string GetRankNameTid(int rank);
    }

    public sealed class DefaultRankManager : IRankManager
    {
        public enum Rank
        {
            Lead = 0,
            Colead,
            Member
        }

        #region IRankManager implementation

        public bool Exists(int rank)
        {
            return rank >= 0 && rank <= (int)Rank.Member;
        }

        public bool CanChangeRank(int ownRank, int oldRank, int newRank)
        {
            if(!Exists(ownRank))
            {
                throw new InvalidRankException(ownRank);
            }
            if(!Exists(oldRank))
            {
                throw new InvalidRankException(oldRank);
            }
            if(!Exists(newRank))
            {
                throw new InvalidRankException(newRank);
            }

            switch((Rank)ownRank)
            {
            case Rank.Lead:
                return true;
            case Rank.Colead:
                return oldRank != (int)Rank.Lead && newRank != (int)Rank.Lead;
            case Rank.Member:
                return false;
            default:
                return false;
            }
        }

        public int GetPromotionRank(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidRankException(rank);
            }

            switch((Rank)rank)
            {
            case Rank.Lead:
            case Rank.Colead: 
                return (int)Rank.Lead;
            case Rank.Member:
                return (int)Rank.Colead;
            default:
                return (int)Rank.Member;
            }
        }

        public int GetDemotionRank(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidAccessTypeException(rank);
            }

            switch((Rank)rank)
            {
            case Rank.Lead:
                return (int)Rank.Colead;
            case Rank.Colead: 
            case Rank.Member:
                return (int)Rank.Member;
            default:
                return (int)Rank.Member;
            }
        }

        public bool HasPermission(int rank, RankPermission permission)
        {
            switch(permission)
            {
            case RankPermission.EditAlliance:
                return HasAllianceManagementPermission(rank);
            case RankPermission.ManageCandidates:
                return HasManageCandidatesPermission(rank);
            }

            return false;
        }

        bool HasAllianceManagementPermission(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidRankException(rank);
            }

            var r = (Rank)rank;
            return (r == Rank.Lead || r == Rank.Colead);
        }

        bool HasManageCandidatesPermission(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidRankException(rank);
            }

            var r = (Rank)rank;
            return (r == Rank.Lead || r == Rank.Colead);
        }

        bool IsPromotion(int oldRank, int newRank)
        {
            if(!Exists(newRank))
            {
                throw new InvalidRankException(newRank);
            }
            if(!Exists(oldRank))
            {
                throw new InvalidRankException(oldRank);
            }

            return newRank < oldRank;
        }

        bool IsDemotion(int oldRank, int newRank)
        {
            if(!Exists(newRank))
            {
                throw new InvalidRankException(newRank);
            }
            if(!Exists(oldRank))
            {
                throw new InvalidRankException(oldRank);
            }

            return newRank > oldRank;
        }

        public string GetRankChangeMessageTid(int oldRank, int newRank)
        {
            if(!Exists(oldRank))
            {
                throw new InvalidRankException(oldRank);
            }

            if(!Exists(newRank))
            {
                throw new InvalidRankException(newRank);
            }

            if(IsPromotion(oldRank, newRank))
            {
                return SocialFrameworkStrings.ChatPlayerPromotedKey;
            }
            else if(IsDemotion(oldRank, newRank))
            {
                return SocialFrameworkStrings.ChatPlayerDemotedKey;
            }
            else
            {
                return string.Empty;
            }
        }

        public string GetRankNameTid(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidAccessTypeException(rank);
            }

            switch((Rank)rank)
            {
            case Rank.Lead:
                return SocialFrameworkStrings.AllianceLeadNameKey;
            case Rank.Colead: 
                return SocialFrameworkStrings.AllianceColeadNameKey;
            case Rank.Member:
                return SocialFrameworkStrings.AllianceMemberNameKey;
            default:
                return string.Empty;
            }
        }

        public int FounderRank
        {
            get
            {
                return (int)Rank.Lead;
            }
        }

        public int DefaultRank
        {
            get
            {
                return (int)Rank.Member;
            }
        }

        #endregion
    }

    public sealed class InvalidRankException : Exception
    {
        public int Rank { get; private set; }

        public InvalidRankException(int rank) : base(string.Format("Invalid member Rank {0}", rank))
        {
            Rank = rank;
        }

        public InvalidRankException(int rank, string message) : base(message)
        {
            Rank = rank;
        }
    }
}
