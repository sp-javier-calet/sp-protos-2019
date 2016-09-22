using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class AllPlayersReadyMessage : INetworkShareable
    {
        public int RemainingMillisecondsToStart { get; private set; }

        public int ServerTimestamp { get; private set; }

        public byte PlayerId{ get; private set; }

        public AllPlayersReadyMessage(int serverTimestamp = 0, int remainingMillisecondsToStart = 0, byte playerId = 0)
        {
            ServerTimestamp = serverTimestamp;
            RemainingMillisecondsToStart = remainingMillisecondsToStart;
            PlayerId = playerId;
        }

        public void Deserialize(IReader reader)
        {
            ServerTimestamp = reader.ReadInt32();
            RemainingMillisecondsToStart = reader.ReadInt32();
            PlayerId = reader.ReadByte();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ServerTimestamp);
            writer.Write(RemainingMillisecondsToStart);
            writer.Write(PlayerId);
        }
    }
}