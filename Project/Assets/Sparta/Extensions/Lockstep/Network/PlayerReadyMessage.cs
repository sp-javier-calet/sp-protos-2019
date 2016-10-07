using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public uint PlayerId { get; private set; }

        public PlayerReadyMessage(uint playerId = 0)
        {
            PlayerId = playerId;
        }

        public void Deserialize(IReader reader)
        {
            PlayerId = reader.ReadUInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
        }
    }
}