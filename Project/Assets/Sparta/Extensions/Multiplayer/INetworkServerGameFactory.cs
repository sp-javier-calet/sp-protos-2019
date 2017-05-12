using SocialPoint.Network;
using System.Collections.Generic;
using SocialPoint.IO;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerGameFactory
    {
        object Create(NetworkServerSceneController ctrl, INetworkServer networkServer, IFileManager fileManager, Dictionary<string, string> config);
    }
}
