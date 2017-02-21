using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;
using System.Collections.Generic;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public abstract class StardustApplication : Application
    {
        protected override ClientConnection CreateClientConnection(string gameName, string lobbyName, int num)
        {
            return new StardustClientConnection(gameName, lobbyName, num, false, this);
        }

        public abstract object CreateGameClient(object factory, StardustClientConnection conn, Dictionary<string, string> config);
    }
}
