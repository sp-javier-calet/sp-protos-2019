#define SPARTA_LOG_VERBOSE

using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    public class SocketServer
    {
        public enum Protocol
        {
            TCP,
            UDP
        }

        const string ServerAddress = "127.0.0.1";
        const int PeerLimit = 100;
        const string ConnectionKey = "TestConnectionKey";
        const int UpdateTime = 10;

        INetworkServer _netServer;
        NetworkMatchDelegateFactory _matchDelegateFactory;
        MultiMatchController _multiMatch;

        public SocketServer(Protocol protocol, int port, IUpdateScheduler updateScheduler)
        {
            switch(protocol)
            {
            case Protocol.TCP:
                _netServer = new TcpSocketNetworkServer(updateScheduler, ServerAddress, port);
                break;
            case Protocol.UDP:
                _netServer = new UdpSocketNetworkServer(updateScheduler, PeerLimit, ConnectionKey, UpdateTime);
                (_netServer as UdpSocketNetworkServer).Port = port;
                break;
            }
            _matchDelegateFactory = new NetworkMatchDelegateFactory();
            _multiMatch = new MultiMatchController(_netServer, _matchDelegateFactory, TypeMessages.ConnectMessageType);
        }

        public void Start()
        {
            _netServer.Start();
        }
    }
}
