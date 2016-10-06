using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientStartMessage : INetworkShareable
    {
        public int StartDelay { get; private set; }

        public int ServerTimestamp { get; private set; }

        public byte PlayerId{ get; private set; }

        public ClientStartMessage(int serverTimestamp = 0, int startDelay = 0, byte playerId = 0)
        {
            ServerTimestamp = serverTimestamp;
            StartDelay = startDelay;
            PlayerId = playerId;
        }

        public void Deserialize(IReader reader)
        {
            ServerTimestamp = reader.ReadInt32();
            StartDelay = reader.ReadInt32();
            PlayerId = reader.ReadByte();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ServerTimestamp);
            writer.Write(StartDelay);
            writer.Write(PlayerId);
        }
    }
}