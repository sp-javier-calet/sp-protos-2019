using System;
using SocialPoint.Base;
using SocialPoint.Utils;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Net.NetworkInformation;

namespace SocialPoint.Network
{
    public class TCPSocketNetworkServer : SocketNetworkServer
    {
        private  List<TCPServerListener> _listenersList;
        private  TCPServerListener _listener;

        public TCPSocketNetworkServer(IUpdateScheduler updateScheduler, string serverAddr = null, int port = DefaultPort)
            : base(updateScheduler,serverAddr, port)
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer CONSTRUCTOR " + port);

            _listenersList = new List<TCPServerListener>();

            _listener = new TCPServerListener(IPAddress.Parse(serverAddr), port);
//            var ipAddresses = GetIPAddresses();
//                foreach (var ipAddr in ipAddresses)
//                {
//                  try
//                  {
//                      CreateListeners(ipAddr, port);
//                  }
//                  catch (SocketException ex)
//                  {
//                    UnityEngine.Debug.LogException(ex);
//                  }
//                }


            RegisterHandlers();

        }

        void CreateListeners (IPAddress ipAddress, int port)
        {
            var listener = new TCPServerListener(ipAddress, port);
            _listenersList.Add(listener);
        }
       

        void RegisterHandlers()
        {
            _listener.OnConnectClient += NotifyClientConnected;
            _listener.OnDisconnectClient += NotifyClientConnected;

//            foreach(var listener in _listenersList)
//            {
//                listener.OnConnectClient += NotifyClientConnected;
//                listener.OnDisconnectClient += NotifyClientConnected;
//            }
           
        }

        public override void Start()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Start");
            if(Running)
            {
                return;
            }

            Running = true;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStarted();
            }

            _listener.Start();

//            foreach(var listener in _listenersList)
//            {
//                listener.Start();
//            }
           
        }


        public override void Stop()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer RegisterReceiver");
            if(!Running)
            {
                return;
            }

            Running = false;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnServerStopped();
            }

            _listener.QueueStop = true;

//            foreach(var listener in _listenersList)
//            {
//                listener.QueueStop = true;
//            }
        }

       
        public override void Fail(Error err)
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Fail");
        }

        public override int GetTimestamp()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer GetTimestamp");

            return 0;
        }


        public override string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override INetworkMessage CreateMessage(NetworkMessageData data)
        {
//            UnityEngine.Debug.Log("TCPSocketNetworkServer CreateMessage " + data.MessageType);
            return new SocketNetworkMessage(data, null);
        }

      

        public override void Dispose()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Dispose");
            base.Dispose();
            UnregisterHandlers();
        }

        void UnregisterHandlers()
        {
            _listener.OnConnectClient -= NotifyClientConnected;
            _listener.OnDisconnectClient -= NotifyClientConnected;

//            foreach(var listener in _listenersList)
//            {
//                listener.OnConnectClient -= NotifyClientConnected;
//                listener.OnDisconnectClient -= NotifyClientConnected;
//            }
        }

        public override void Update()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Update");
        }

        public void NotifyClientConnected(TcpClient client)
        {
            UnityEngine.Debug.Log("NotifyClientConnected");
            byte clientId = 0;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected(clientId);
            }
        }

        public void NotifyClientDisconnected(TcpClient client)
        {
            UnityEngine.Debug.Log("NotifyClientDisconnected");
            byte clientId = 0;
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected(clientId);
            }
        }




        public IEnumerable<IPAddress> GetIPAddresses()
        {
            List<IPAddress> ipAddresses = new List<IPAddress>();

            IEnumerable<NetworkInterface> enabledNetInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach(NetworkInterface netInterface in enabledNetInterfaces)
            {
                if(netInterface.OperationalStatus != OperationalStatus.Up)
                {
                    continue;
                }

                IPInterfaceProperties ipProps = netInterface.GetIPProperties();
                foreach(UnicastIPAddressInformation addr in ipProps.UnicastAddresses)
                {
                    if(!ipAddresses.Contains(addr.Address))
                    {
                        ipAddresses.Add(addr.Address);
                    }
                }
            }

            var ipSorted = ipAddresses; //.OrderByDescending(ip => RankIpAddress(ip)).ToList();
            return ipSorted;
        }




//        private int RankIpAddress(IPAddress addr)
//        {
//            int rankScore = 1000;
//
//            if (IPAddress.IsLoopback(addr))
//            {
//                // rank loopback below others, even though their routing metrics may be better
//                rankScore = 300;
//            }
//            else if (addr.AddressFamily == AddressFamily.InterNetwork)
//            {
//                rankScore += 100;
//                // except...
//                if (addr.GetAddressBytes().Take(2).SequenceEqual(new byte[] { 169, 254 }))
//                {
//                    // APIPA generated address - no router or DHCP server - to the bottom of the pile
//                    rankScore = 0;
//                }
//            }
//
//            if (rankScore > 500)
//            {
//                foreach (var nic in TryGetCurrentNetworkInterfaces())
//                {
//                    var ipProps = nic.GetIPProperties();
//                    if (ipProps.GatewayAddresses.Any())
//                    {
//                        if (ipProps.UnicastAddresses.Any(u => u.Address.Equals(addr)))
//                        {
//                            // if the preferred NIC has multiple addresses, boost all equally
//                            // (justifies not bothering to differentiate... IOW YAGNI)
//                            rankScore += 1000;
//                        }
//
//                        // only considering the first NIC that is UP and has a gateway defined
//                        break;
//                    }
//                }
//            }
//
//            return rankScore;
//        }




        //
        //
        //
        //        INetworkMessageReceiver _receiver;
        //        List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        //        IUpdateScheduler _updateScheduler;
        //        NetworkServerSimple _server;
        //        int _port;
        //
        //        public bool Running{ get; private set; }
        //
        //        public string Id
        //        {
        //            get
        //            {
        //                return _server.serverHostId.ToString();
        //            }
        //        }
        //
        //        public const int DefaultPort = 8888;
        //
        //        public UnetNetworkServer(IUpdateScheduler updateScheduler, int port = DefaultPort, HostTopology topology = null)
        //        {
        //            _updateScheduler = updateScheduler;
        //            _port = port;
        //            _server = new NetworkServerSimple();
        //            if(topology != null)
        //            {
        //                _server.Configure(topology);
        //            }
        //            _updateScheduler.Add(this);
        //            RegisterHandlers();
        //        }
        //
        //        public void Dispose()
        //        {
        //            Stop();
        //            UnregisterHandlers();
        //            _server = null;
        //            _delegates.Clear();
        //            _delegates = null;
        //            _receiver = null;
        //            _updateScheduler.Remove(this);
        //        }
        //
        //        public void Start()
        //        {
        //            if(Running)
        //            {
        //                return;
        //            }
        //            if(!_server.Listen(_port) || _server.serverHostId == -1)
        //            {
        //                throw new Exception("Failed to start.");
        //            }
        //            Running = true;
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnServerStarted();
        //            }
        //        }
        //
        //        public void Stop()
        //        {
        //            if(!Running)
        //            {
        //                return;
        //            }
        //            if(_server.serverHostId >= 0)
        //            {
        //                NetworkTransport.RemoveHost(_server.serverHostId);
        //            }
        //            _server.Stop();
        //            Running = false;
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnServerStopped();
        //            }
        //        }
        //
        //        public void Fail(Error err)
        //        {
        //            if(!Running)
        //            {
        //                return;
        //            }
        //            var writer = new NetworkWriter();
        //            writer.StartMessage(UnetMsgType.Fail);
        //            writer.Write(err.ToString());
        //            writer.FinishMessage();
        //            for(var i = 0; i < _server.connections.Count; i++)
        //            {
        //                _server.connections[i].SendWriter(writer, Channels.DefaultReliable);
        //            }
        //        }
        //
        //        public void Update()
        //        {
        //            _server.Update();
        //        }
        //
        //        void RegisterHandlers()
        //        {
        //            UnregisterHandlers();
        //            _server.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnConnectReceived);
        //            _server.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnDisconnectReceived);
        //            _server.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnErrorReceived);
        //            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
        //            {
        //                _server.RegisterHandler(i, OnMessageReceived);
        //            }
        //        }
        //
        //        void UnregisterHandlers()
        //        {
        //            _server.UnregisterHandler(UnityEngine.Networking.MsgType.Connect);
        //            _server.UnregisterHandler(UnityEngine.Networking.MsgType.Disconnect);
        //            _server.UnregisterHandler(UnityEngine.Networking.MsgType.Error);
        //            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
        //            {
        //                _server.UnregisterHandler(i);
        //            }
        //        }
        //
        //        void OnConnectReceived(NetworkMessage umsg)
        //        {
        //            var clientId = (byte)umsg.conn.connectionId;
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnClientConnected(clientId);
        //            }
        //        }
        //
        //        void OnDisconnectReceived(NetworkMessage umsg)
        //        {
        //            var clientId = (byte)umsg.conn.connectionId;
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnClientDisconnected(clientId);
        //            }
        //        }
        //
        //        void OnErrorReceived(NetworkMessage umsg)
        //        {
        //            var errMsg = umsg.ReadMessage<ErrorMessage>();
        //            var err = new Error(errMsg.errorCode, errMsg.ToString());
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnNetworkError(err);
        //            }
        //        }
        //
        //        void OnMessageReceived(NetworkMessage umsg)
        //        {
        //            var data = new NetworkMessageData {
        //                MessageType = UnetMsgType.ConvertType(umsg.msgType),
        //                ClientIds = new List<byte>(){ (byte)umsg.conn.connectionId }
        //            };
        //            if(_receiver != null)
        //            {
        //                _receiver.OnMessageReceived(data, new UnetNetworkReader(umsg.reader));
        //            }
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnMessageReceived(data);
        //            }
        //        }
        //
        //        public INetworkMessage CreateMessage(NetworkMessageData data)
        //        {
        //            NetworkConnection[] conns;
        //            if(data.ClientIds != null && data.ClientIds.Count > 0)
        //            {
        //                var conn = _server.FindConnection(data.ClientIds[0]);
        //                if(conn == null)
        //                {
        //                    conns = new NetworkConnection[]{ };
        //                }
        //                else
        //                {
        //                    conns = new NetworkConnection[]{ conn };
        //                }
        //            }
        //            else
        //            {
        //                conns = new NetworkConnection[_server.connections.Count];
        //                _server.connections.CopyTo(conns, 0);
        //            }
        //            return new UnetNetworkMessage(data, conns);
        //        }
        //
        //        public void AddDelegate(INetworkServerDelegate dlg)
        //        {
        //            _delegates.Add(dlg);
        //            if(Running && dlg != null)
        //            {
        //                dlg.OnServerStarted();
        //            }
        //        }
        //
        //        public void RemoveDelegate(INetworkServerDelegate dlg)
        //        {
        //            _delegates.Remove(dlg);
        //        }
        //
        //        public void RegisterReceiver(INetworkMessageReceiver receiver)
        //        {
        //            _receiver = receiver;
        //        }
        //
        //        public int GetTimestamp()
        //        {
        //            return NetworkTransport.GetNetworkTimestamp();
        //        }
        //
        //        public bool LatencySupported
        //        {
        //            get
        //            {
        //                return false;
        //            }
        //        }
    }
}
