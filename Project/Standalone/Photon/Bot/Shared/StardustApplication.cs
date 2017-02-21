using Photon.Stardust.S2S.Server;
using Photon.Stardust.S2S.Server.ClientConnections;

namespace SocialPoint.Network
{
    public class StardustApplication : Application
    {
        protected override ClientConnection CreateClientConnection(string gameName, string lobbyName, int num)
        {
            return new StardustClientConnection(gameName, lobbyName, num, false, this);
        }

    }
}
