using System;
using System.Collections.Generic;
using System.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public interface INetworkMessage
    {
        void Deserialize(IReaderWrapper reader);

        void Serialize(IWriterWrapper writer);

        bool RequiresSync { get; }
    }

    public class EmptyMessage : INetworkMessage
    {
        public void Deserialize(IReaderWrapper reader)
        {
        }

        public void Serialize(IWriterWrapper writer)
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