using SocialPoint.Network;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public interface INetworkClientGameFactory
    {
        object Create(NetworkClientSceneController ctrl, Dictionary<string, string> config);
    }
}
