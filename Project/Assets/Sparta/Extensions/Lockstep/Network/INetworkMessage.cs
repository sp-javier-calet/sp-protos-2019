using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public interface INetworkMessage
    {
        void Deserialize(IReader reader);

        void Serialize(IWriter writer);

        bool RequiresSync { get; }
    }

    public class EmptyMessage : INetworkMessage
    {
        public void Deserialize(IReader reader)
        {
        }

        public void Serialize(IWriter writer)
        {
        }

        public bool RequiresSync
        {
            get
            {
                return false;
            }
        }
    }
}