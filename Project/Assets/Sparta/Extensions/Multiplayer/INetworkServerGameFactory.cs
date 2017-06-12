using SocialPoint.Network;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Matchmaking;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerGameFactory
    {
        object Create(NetworkServerSceneController ctrl, INetworkServer networkServer, IFileManager fileManager, IMatchmakingServer matchmakingServer, Dictionary<string, string> config);
    }
}
