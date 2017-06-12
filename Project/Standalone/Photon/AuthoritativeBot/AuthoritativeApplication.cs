using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Multiplayer
{
    public class AuthoritativeApplication : StardustApplication
    {
        public override object CreateGameClient(object factory, StardustClientConnection conn, Dictionary<string, string> config)
        {
            var mpFactory = (INetworkClientGameFactory)factory;
            var sceneClient = new NetworkClientSceneController(conn.NetworkClient);
            var scheduler = new UpdateScheduler();
            return mpFactory.Create((INetworkClient)sceneClient, scheduler, config);
        }
    }
}
