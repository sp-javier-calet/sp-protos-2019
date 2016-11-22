using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public string PlayerId { get; private set; }

        public PlayerReadyMessage(string playerId = null)
        {
            PlayerId = playerId;
        }

        public void Deserialize(IReader reader)
        {
            PlayerId = reader.ReadString();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
        }
    }
}