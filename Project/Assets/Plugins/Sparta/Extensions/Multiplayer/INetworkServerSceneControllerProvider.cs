using SocialPoint.Network;
using System.Collections.Generic;
using SocialPoint.IO;
using SocialPoint.Matchmaking;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public interface INetworkServerSceneControllerProvider
    {
        NetworkServerSceneController NetworkServerSceneController { get; }
    }
}
