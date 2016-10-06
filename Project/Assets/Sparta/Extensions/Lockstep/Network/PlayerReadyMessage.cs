using SocialPoint.IO;

namespace SocialPoint.Lockstep.Network
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public uint PlayerHash { get; private set; }

        public PlayerReadyMessage(uint playerHash=0)
        {
            PlayerHash = playerHash;
        }

        public void Deserialize(IReader reader)
        {
            PlayerHash = reader.ReadUInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerHash);
        }
    }
}