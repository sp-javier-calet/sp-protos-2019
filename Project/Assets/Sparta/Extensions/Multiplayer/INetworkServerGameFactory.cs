using SocialPoint.Network;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerGameFactory
    {
        object Create(NetworkServerSceneController ctrl, Dictionary<string, string> config);
    }
}
