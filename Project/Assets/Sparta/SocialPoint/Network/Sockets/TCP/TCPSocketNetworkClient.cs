using System;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace SocialPoint.Network
{
    public sealed class TCPSocketNetworkClient : SocketNetworkClient
    {
        const int ReadLoopIntervalMs = 10;

        internal bool QueueStop { get; set; }

        private TcpClient _client;
        private Thread _receiveMessagesThread = null;

        public override byte ClientId
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override bool Connected
        {
            get
            {
                return _client != null && _client.Connected;
            }
        }


        public TCPSocketNetworkClient(string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort)
            : base(serverAddr, serverPort)
        {
            _client = new TcpClient();
        }


        public override void Connect()
        {
            UnityEngine.Debug.Log("Connect");

            if (Connected)
            {
                return;
            }

            _client.Connect(_serverAddr, _serverPort);
            StartReceiveMessagesThread();
        }

        void StartReceiveMessagesThread()
        {
            if (_receiveMessagesThread != null)
            {
                return;
            }

            _receiveMessagesThread = new Thread(ListenerLoop);
            _receiveMessagesThread.IsBackground = true;
            _receiveMessagesThread.Start();
        }

        private void ListenerLoop(object state)
        {
            while (!QueueStop)
            {
                try
                {
                    RunLoopStep();
                }
                catch
                {
        
                }
        
                System.Threading.Thread.Sleep(ReadLoopIntervalMs);
            }
        
            _receiveMessagesThread = null;
        }

        private void RunLoopStep()
        {
            if (_client == null)
            {
                return;
            }
            if (!Connected)
            {
                return;
            }
        
            UnityEngine.Debug.Log("CLIENT RunLoopStep");
//            var delimiter = this.Delimiter;
//            var c = _client;
//        
//            int bytesAvailable = c.Available;
//            if (bytesAvailable == 0)
//            {
//                System.Threading.Thread.Sleep(10);
//                return;
//            }
//        
//            List<byte> bytesReceived = new List<byte>();
//        
//            while (c.Available > 0 && c.Connected)
//            {
//                byte[] nextByte = new byte[1];
//                c.Client.Receive(nextByte, 0, 1, SocketFlags.None);
//                bytesReceived.AddRange(nextByte);
//                if (nextByte[0] == delimiter)
//                {
//                    byte[] msg = _queuedMsg.ToArray();
//                    _queuedMsg.Clear();
//                    NotifyDelimiterMessageRx(c, msg);
//                }
//                else
//                {
//                    _queuedMsg.AddRange(nextByte);
//                }
//            }
//        
//            if (bytesReceived.Count > 0)
//            {
//                NotifyEndTransmissionRx(c, bytesReceived.ToArray());
//            }
        }

        public override void Disconnect()
        {
            UnityEngine.Debug.Log("Disconnect");

            if (!Connected)
            {
                return;
            }

            _client.Client.Shutdown(SocketShutdown.Both);
            _client.Client.Disconnect(true);

        }

        public override int GetDelay(int networkTimestamp)
        {
            throw new NotImplementedException();
        }


        public override INetworkMessage CreateMessage(NetworkMessageData data)
        {
            throw new NotImplementedException();
        }


        public override void Dispose()
        {
            base.Dispose();

            QueueStop = true;

            _client.Close();
            _client = null;
        }

        //        INetworkMessageReceiver _receiver;
        //        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        //        NetworkClient _client;
        //        string _serverAddr;
        //        int _serverPort;
        //
        //        public const string DefaultServerAddr = "localhost";
        //
        //        public byte ClientId
        //        {
        //            get
        //            {
        //                if(!Connected)
        //                {
        //                    return 0;
        //                }
        //                return (byte)_client.connection.connectionId;
        //            }
        //        }
        //
        //        public bool Connected
        //        {
        //            get
        //            {
        //                return _client != null && _client.isConnected;
        //            }
        //        }
        //
        //        public EnetNetworkClient(string serverAddr = null, int serverPort = UnetNetworkServer.DefaultPort, HostTopology topology = null)
        //        {
        //            if(string.IsNullOrEmpty(serverAddr))
        //            {
        //                serverAddr = DefaultServerAddr;
        //            }
        //            _serverAddr = serverAddr;
        //            _serverPort = serverPort;
        //            NetworkTransport.Init();
        //            _client = new NetworkClient();
        //            if(topology != null)
        //            {
        //                _client.Configure(topology);
        //            }
        //            RegisterHandlers();
        //        }
        //
        //        public void Dispose()
        //        {
        //            Disconnect();
        //            UnregisterHandlers();
        //            _client.Shutdown();
        //            _client = null;
        //            _delegates.Clear();
        //            _delegates = null;
        //            _receiver = null;
        //        }
        //
        //        void RegisterHandlers()
        //        {
        //            UnregisterHandlers();
        //            _client.RegisterHandler(UnityEngine.Networking.MsgType.Connect, OnConnectReceived);
        //            _client.RegisterHandler(UnityEngine.Networking.MsgType.Disconnect, OnDisconnectReceived);
        //            _client.RegisterHandler(UnityEngine.Networking.MsgType.Error, OnErrorReceived);
        //            _client.RegisterHandler(UnetMsgType.Fail, OnFailReceived);
        //            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
        //            {
        //                _client.RegisterHandler(i, OnMessageReceived);
        //            }
        //        }
        //
        //        void UnregisterHandlers()
        //        {
        //            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Connect);
        //            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Disconnect);
        //            _client.UnregisterHandler(UnityEngine.Networking.MsgType.Error);
        //            _client.UnregisterHandler(UnetMsgType.Fail);
        //            for(byte i = UnetMsgType.Highest + 1; i < byte.MaxValue; i++)
        //            {
        //                _client.UnregisterHandler(i);
        //            }
        //        }
        //
        //        void OnConnectReceived(NetworkMessage umsg)
        //        {
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnClientConnected();
        //            }
        //        }
        //
        //        void OnDisconnectReceived(NetworkMessage umsg)
        //        {
        //            for(var i = 0; i < _delegates.Count; i++)
        //            {
        //                _delegates[i].OnClientDisconnected();
        //            }
        //        }
        //
        //        void OnErrorReceived(NetworkMessage umsg)
        //        {
        //            var errMsg = umsg.ReadMessage<ErrorMessage>();
        //            var err = new Error(errMsg.errorCode, errMsg.ToString());
        //            OnNetworkError(err);
        //        }
        //
        //        void OnFailReceived(NetworkMessage umsg)
        //        {
        //            var err = Error.FromString(umsg.reader.ReadString());
        //            OnNetworkError(err);
        //            Disconnect();
        //        }
        //
        //        void OnNetworkError(Error err)
        //        {
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
        //        public void Connect()
        //        {
        //            if(Connected)
        //            {
        //                return;
        //            }
        //            _client.Connect(_serverAddr, _serverPort);
        //        }
        //
        //        public void Disconnect()
        //        {
        //            if(!_client.isConnected)
        //            {
        //                return;
        //            }
        //            if(_client.connection != null && _client.connection.hostId >= 0)
        //            {
        //                NetworkTransport.RemoveHost(_client.connection.hostId);
        //            }
        //            _client.Disconnect();
        //        }
        //
        //        public INetworkMessage CreateMessage(NetworkMessageData info)
        //        {
        //            return new UnetNetworkMessage(info, new NetworkConnection[]{ _client.connection });
        //        }
        //
        //        public void AddDelegate(INetworkClientDelegate dlg)
        //        {
        //            _delegates.Add(dlg);
        //            if(Connected && dlg != null)
        //            {
        //                dlg.OnClientConnected();
        //            }
        //        }
        //
        //        public void RemoveDelegate(INetworkClientDelegate dlg)
        //        {
        //            _delegates.Remove(dlg);
        //        }
        //
        //        public void RegisterReceiver(INetworkMessageReceiver receiver)
        //        {
        //            _receiver = receiver;
        //        }
        //
        //        public int GetDelay(int serverTimestamp)
        //        {
        //            byte error;
        //            return NetworkTransport.GetRemoteDelayTimeMS(_client.connection.hostId, _client.connection.connectionId, serverTimestamp, out error);
        //        }
        //
        //        public bool LatencySupported
        //        {
        //            get
        //            {
        //                return false;
        //            }
        //        }
        //
        //        public int Latency
        //        {
        //            get
        //            {
        //                DebugUtils.Assert(LatencySupported);
        //                return -1;
        //            }
        //        }

    }
}