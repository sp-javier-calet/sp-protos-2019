using SocialPoint.IO;
using System.Collections.Generic;

namespace SocialPoint.Lockstep
{
    public sealed class ClientStartMessage : INetworkShareable
    {
        public int StartTime { get; private set; }

        public int ServerTimestamp { get; private set; }

        public List<string> PlayerIds { get; private set; }

        public ClientStartMessage(int serverTimestamp = 0, int startTime = 0, List<string> playerIds = null)
        {
            ServerTimestamp = serverTimestamp;
            StartTime = startTime;
            if(playerIds == null)
            {
                playerIds = new List<string>();
            }
            PlayerIds = playerIds;
        }

        public void Deserialize(IReader reader)
        {
            ServerTimestamp = reader.ReadInt32();
            StartTime = reader.ReadInt32();
            var numPlayers = reader.ReadInt32();
            PlayerIds.Clear();
            for(var i = 0; i < numPlayers; i++)
            {
                PlayerIds.Add(reader.ReadString());
            }
        }

        public void Serialize(IWriter writer)
        {
            writer.Write(ServerTimestamp);
            writer.Write(StartTime);
            writer.Write(PlayerIds.Count);
            for(var i = 0; i < PlayerIds.Count; i++)
            {
                var playerId = PlayerIds[i];
                writer.Write(playerId == null ? string.Empty : playerId);
            }
        }
    }
}