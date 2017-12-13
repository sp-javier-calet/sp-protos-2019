using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class ClientChangedConnectionStatusMessage : INetworkShareable
    {
        public byte CliendId { get; private set; }
        public bool Connected { get; private set; }

        public ClientChangedConnectionStatusMessage(byte clientId = 0, bool connected = false)
        {
            CliendId = clientId;
            Connected = connected;
        }

        public void Deserialize(IReader reader)
        {
            CliendId = reader.ReadByte();
            Connected = reader.ReadBoolean();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write((byte)CliendId);
            writer.Write((bool)Connected);
        }
    }
}