using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using System.Collections.Generic;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public class AuthoritativeApplication : StardustApplication
    {
        public override object CreateGameClient(object factory, StardustClientConnection conn, Dictionary<string, string> config)
        {
            var mpFactory = (INetworkClientGameFactory)factory;
            var sceneClient = new NetworkClientSceneController(conn.NetworkClient);
            return mpFactory.Create(sceneClient, config);
        }
    }
}
