using System;
using SocialPoint.Base;
using SocialPoint.Utils;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public class TCPSocketNetworkServer : SocketNetworkServer
    {
        private  TCPServerListener _listener;

        public TCPSocketNetworkServer(IUpdateScheduler updateScheduler, int port = DefaultPort) : base(updateScheduler, port)
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer CONSTRUCTOR");

            IPAddress ipAdress = IPAddress.Any;
            _serverAddr = ipAdress.ToString();

            _listener = new TCPServerListener(this, ipAdress, port);
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
            UnityEngine.Debug.Log("TCPSocketNetworkServer CreateMessage " + data.MessageType);
            return new SocketNetworkMessage(data, null);
        }

      

        public override void Dispose()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Dispose");
        }

      
        public override void Update()
        {
            UnityEngine.Debug.Log("TCPSocketNetworkServer Update");
        }

        public void NotifyClientConnected(TCPServerListener tCPServerListener, TcpClient newClient)
        {
            throw new NotImplementedException();
        }

        public void NotifyClientDisconnected(TCPServerListener tCPServerListener, TcpClient disC)
        {
            throw new NotImplementedException();
        }

        public void NotifyDelimiterMessageRx(TCPServerListener tCPServerListener, TcpClient c, byte[] msg)
        {
            throw new NotImplementedException();
        }

        public void NotifyEndTransmissionRx(TCPServerListener tCPServerListener, TcpClient c, byte[] @byte)
        {
            throw new NotImplementedException();
        }














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
