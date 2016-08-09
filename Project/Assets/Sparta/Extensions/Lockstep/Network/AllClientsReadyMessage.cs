using System;
using System.Text;
using SocialPoint.Attributes;
using System.IO;
using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public class AllClientsReadyMessage : INetworkMessage
    {
        public int RemainingMillisecondsToStart { get; private set; }

        public AllClientsReadyMessage(int remainingMillisecondsToStart = 0)
        {
            RemainingMillisecondsToStart = remainingMillisecondsToStart;
        }

        public void Deserialize(IReader reader)
        {
            RemainingMillisecondsToStart = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
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