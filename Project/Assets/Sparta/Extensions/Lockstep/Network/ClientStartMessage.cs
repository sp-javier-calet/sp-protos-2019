using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientStartMessage : INetworkShareable
    {
        public int StartTime { get; private set; }

        public int ServerTimestamp { get; private set; }

        public byte PlayerNumber { get; private set; }

        public ClientStartMessage(int serverTimestamp = 0, int startTime = 0, byte playerId = 0)
        {
            ServerTimestamp = serverTimestamp;
            StartTime = startTime;
            PlayerNumber = playerId;
        }

        public void Deserialize(IReader reader)
        {
            ServerTimestamp = reader.ReadInt32();
            StartTime = reader.ReadInt32();
            PlayerNumber = reader.ReadByte();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ServerTimestamp);
            writer.Write(StartTime);
            writer.Write(PlayerNumber);
        }
    }
}