#define SPARTA_LOG_VERBOSE

using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Sockets
{
    public class SocketClients
    {
        public enum Protocol
        {
            TCP,
            UDP
        }

        const string ConnectionKey = "TestConnectionKey";
        const int UpdateTime = 10;


        List<INetworkClient> _netClientsList;

        public SocketClients(int numClients, Protocol protocol, string ipAdress,int port, IUpdateScheduler updateScheduler)
        {
            _netClientsList = new List<INetworkClient>();
            switch (protocol)
            {
                case Protocol.TCP:
                    Log.d("INSTANTIATE TCP CLIENTS NumClients: "+ numClients+ " PORT: " + port + " IPAdress: " + ipAdress);
                    for (int i = 0; i < numClients;i++)
                    {
                        INetworkClient clientTcp = new TcpSocketNetworkClient(updateScheduler, ipAdress, port);
                        _netClientsList.Add((clientTcp));
                    }
                    break;
                case Protocol.UDP:
                    Log.d("INSTANTIATE UDP CLIENTS NumClients: " + numClients + " PORT: " + port + " IPAdress: " + ipAdress);

                    for (int i = 0; i < numClients; i++)
                    {
                        INetworkClient clientUdp = new UdpSocketNetworkClient(updateScheduler, ConnectionKey, UpdateTime);
                        (clientUdp as UdpSocketNetworkClient).ServerPort = port;
                        (clientUdp as UdpSocketNetworkClient).ServerAddress = ipAdress;
                        _netClientsList.Add((clientUdp));
                    }
                    break;
            }
        }

        internal void Connect()
        {
            for (int i = 0; i < _netClientsList.Count; i++)
            {
                Log.d("CONNECT CLIENT "+ i);
                _netClientsList[i].Connect();
            }
        }

        internal void Disconnect()
        {
            for (int i = 0; i < _netClientsList.Count; i++)
            {
                Log.d("DISCONNECT CLIENT " + i);
                _netClientsList[i].Disconnect();
            }
        }
    }
}
