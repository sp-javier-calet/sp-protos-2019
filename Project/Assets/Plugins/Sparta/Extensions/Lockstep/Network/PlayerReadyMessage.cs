using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public string PlayerId { get; private set; }

        public int CurrentTurn { get; private set; }

        public string Version  { get; private set; }

        public PlayerReadyMessage(string playerId = null, int currentTurn = 0, string version = default(string))
        {
            PlayerId = playerId;
            CurrentTurn = currentTurn;
            Version = (version == null) ? string.Empty : version;
        }

        public void Deserialize(IReader reader)
        {
            PlayerId = reader.ReadString();
            CurrentTurn = reader.ReadInt32();
            Version = reader.ReadString();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(CurrentTurn);
            writer.Write(Version);
        }
    }
}