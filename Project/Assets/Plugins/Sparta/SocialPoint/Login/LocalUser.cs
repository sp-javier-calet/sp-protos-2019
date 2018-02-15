using System;
using System.Collections.Generic;

namespace SocialPoint.Login
{
    public sealed class LocalUser : User
    {
        public string SessionId { get; set; }

        public LocalUser() : base()
        {
        }

        public LocalUser(UInt64 userId, string sessionId, List<UserMapping> links) : base(userId, links)
        {
            SessionId = sessionId;
        }

        public LocalUser(LocalUser other) : base(other)
        {
            SessionId = other.SessionId;
        }

        public override string ToString()
        {
            return string.Format("{0} // [LocalUser: SessionId={1}]", base.ToString(), SessionId);
        }
    }
}