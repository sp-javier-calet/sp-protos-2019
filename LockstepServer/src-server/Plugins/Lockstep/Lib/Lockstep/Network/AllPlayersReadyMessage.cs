using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class AllPlayersReadyMessage : INetworkShareable
    {
        public int RemainingMillisecondsToStart { get; private set; }

        public int ServerTimestamp { get; private set; }

        public byte[] PlayerIds{ get; private set; }

        public AllPlayersReadyMessage(int serverTimestamp = 0, int remainingMillisecondsToStart = 0, byte[] playerIds = null)
        {
            ServerTimestamp = serverTimestamp;
            RemainingMillisecondsToStart = remainingMillisecondsToStart;
            PlayerIds = playerIds ?? new byte[0];
        }

        public void Deserialize(IReader reader)
        {
            ServerTimestamp = reader.ReadInt32();
            RemainingMillisecondsToStart = reader.ReadInt32();
            var size = reader.ReadInt32();
            PlayerIds = new byte[size];
            for(var i = 0; i < size; i++)
            {
                PlayerIds[i] = reader.ReadByte();
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ServerTimestamp);
            writer.Write(RemainingMillisecondsToStart);
            writer.Write(PlayerIds.Length);
            for(var i = 0; i < PlayerIds.Length; i++)
            {
                writer.Write(PlayerIds[i]);
            }
        }
    }
}