﻿using System;

namespace SocialPoint.Social
{
    public enum RankPermission
    {
        EditAlliance,
        Members
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

        /// <summary>
        /// Compare two ranks
        /// </summary>
        int Compare(int rank1, int rank2);

        /// <summary>
        /// Try a promotion to another Rank.
        /// Game may have restriction for when a promotions is available
        /// </summary>
        /// <returns>Final promotion rank</returns>
        int GetPromotedTo(int toRank);

        int GetPromoted(int rank);

        int GetDemoted(int rank);

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

        public int Compare(int rank1, int rank2)
        {
            if(!Exists(rank1))
            {
                throw new InvalidRankException(rank1);
            }

            if(!Exists(rank2))
            {
                throw new InvalidRankException(rank2);
            }

            return rank2 - rank1;
        }

        public int GetPromotedTo(int toRank)
        {
            if(!Exists(toRank))
            {
                throw new InvalidRankException(toRank);
            }

            var rank = (Rank)toRank;
            return rank == Rank.Lead ? (int)Rank.Colead : toRank;
        }

        public int GetPromoted(int rank)
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

        public int GetDemoted(int rank)
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
            case RankPermission.Members:
                return HasMemberManagementPermission(rank);
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

        bool HasMemberManagementPermission(int rank)
        {
            if(!Exists(rank))
            {
                throw new InvalidRankException(rank);
            }

            var r = (Rank)rank;
            return (r == Rank.Lead || r == Rank.Colead);
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

            var comp = Compare(oldRank, newRank);
            if(comp > 0)
            {
                return SocialFrameworkStrings.ChatPlayerPromotedKey;
            }
            return comp < 0 ? SocialFrameworkStrings.ChatPlayerDemotedKey : string.Empty;
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
