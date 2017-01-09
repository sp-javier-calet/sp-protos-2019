using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public sealed class PlayerReadyMessage : INetworkShareable
    {
        public string PlayerId { get; private set; }
        public string BackendEnv { get; private set; }

        public PlayerReadyMessage(string playerId = null, string environmentId = null)
        {
            PlayerId = playerId;
            BackendEnv = environmentId;
        }

        public void Deserialize(IReader reader)
        {
            PlayerId = reader.ReadString();
            BackendEnv = reader.ReadString();
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(BackendEnv);
        }
    }
}