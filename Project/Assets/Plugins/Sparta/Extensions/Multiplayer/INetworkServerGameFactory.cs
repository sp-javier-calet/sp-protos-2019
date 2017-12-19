using SocialPoint.Network;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Matchmaking;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerGameFactory
    {
        object Create(NetworkServerSceneController ctrl, INetworkServer networkServer, IFileManager fileManager, IMatchmakingServer matchmakingServer, IUpdateScheduler updateScheduler, Dictionary<string, string> config);
    }
}
