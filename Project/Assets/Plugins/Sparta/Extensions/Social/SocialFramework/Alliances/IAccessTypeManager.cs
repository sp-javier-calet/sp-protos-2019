using System;

namespace SocialPoint.Social
{
    public interface IAccessTypeManager
    {
        /// <summary>
        /// Default Access Type index for an alliance
        /// </summary>
        int DefaultAccessType { get; }

        /// <summary>
        /// Determines if a alliance type is defined
        /// </summary>
        bool Exists(int accessType);

        /// <summary>
        /// Determines if a alliance type represents a public alliance
        /// </summary>
        bool IsPublic(int accessType);

        /// <summary>
        /// Determines if a alliance type accepts new candidates and join requests
        /// </summary>
        bool AcceptsCandidates(int accessType);
    }

    public sealed class DefaultAccessTypeManager : IAccessTypeManager
    {
        public enum AccessType
        {
            Open = 0,
            Private,
            Closed
        }

        #region IAccessTypeManager implementation

        public bool Exists(int accessType)
        {
            return accessType >= 0 && accessType <= (int)AccessType.Closed;
        }

        public bool IsPublic(int accessType)
        {
            if(!Exists(accessType))
            {
                throw new InvalidAccessTypeException(accessType);
            }

            var type = (AccessType)accessType;
            return type == AccessType.Open;
        }

        public bool AcceptsCandidates(int accessType)
        {
            if(!Exists(accessType))
            {
                throw new InvalidAccessTypeException(accessType);
            }

            var type = (AccessType)accessType;
            return type == AccessType.Private;
        }

        public int DefaultAccessType
        {
            get
            {
                return (int)AccessType.Open;
            }
        }

        #endregion
    }

    public sealed class InvalidAccessTypeException : Exception
    {
        public int AccessType { get; private set; }

        public InvalidAccessTypeException(int type) : base(string.Format("Invalid access type {0}", type))
        {
            AccessType = type;
        }

        public InvalidAccessTypeException(int type, string message) : base(message)
        {
            AccessType = type;
        }
    }
}
