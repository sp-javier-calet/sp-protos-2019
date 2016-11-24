using SocialPoint.Network;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerGameFactory
    {
        object Create(INetworkServer server, NetworkServerSceneController ctrl, Dictionary<string, string> config);
    }
}
