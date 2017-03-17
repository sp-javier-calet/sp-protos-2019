using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public string PlayerId { get; private set; }
        public int CurrentTurn { get; private set; }

        public PlayerReadyMessage(string playerId = null, int currentTurn = 0)
        {
            PlayerId = playerId;
            CurrentTurn = currentTurn;
        }

        public void Deserialize(IReader reader)
        {
            PlayerId = reader.ReadString();
            CurrentTurn = reader.ReadInt32();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(CurrentTurn);
        }
    }
}