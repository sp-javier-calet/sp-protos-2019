using System.Collections.Generic;
using SocialPoint.Network;

namespace SocialPoint.Multiplayer
{
    public class AuthoritativeApplication : StardustApplication
    {
        public override object CreateGameClient(object factory, StardustClientConnection connection, Dictionary<string, string> config)
        {
            var clientFactory = (INetworkClientGameFactory)factory;
            var clientSceneController = new NetworkClientSceneController(connection.NetworkClient);
            config.Add("GameName", connection.GameName);
            return clientFactory.Create(clientSceneController, config);
        }
    }
}
