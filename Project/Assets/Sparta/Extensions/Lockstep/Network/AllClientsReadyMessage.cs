using System;
using System.Text;
using SocialPoint.Attributes;
using System.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public class AllClientsReadyMessage : INetworkMessage
    {
        public int RemainingMillisecondsToStart { get; private set; }

        public AllClientsReadyMessage(int remainingMillisecondsToStart = 0)
        {
            RemainingMillisecondsToStart = remainingMillisecondsToStart;
        }

        public void Deserialize(IReaderWrapper reader)
        {
            RemainingMillisecondsToStart = reader.ReadInt32();
        }

        public void Serialize(IWriterWrapper writer)
        {
            writer.Write(RemainingMillisecondsToStart);
        }

        public bool RequiresSync
        {
            get
            {
                return true;
            }
        }
    }
}