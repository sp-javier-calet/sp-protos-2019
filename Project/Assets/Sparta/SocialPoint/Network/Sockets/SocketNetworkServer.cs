using System.Collections.Generic;
using System;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public abstract class SocketNetworkServer : INetworkServer, IDisposable, IUpdateable
    {
        public const int DefaultPort = 8888;

        protected INetworkMessageReceiver _receiver;
        protected List<INetworkServerDelegate> _delegates = new List<INetworkServerDelegate>();
        protected IUpdateScheduler _updateScheduler;
        protected  int _port;
        protected  string _serverAddr;

        public SocketNetworkServer(IUpdateScheduler updateScheduler, int port = DefaultPort)
        {
            UnityEngine.Debug.Log("SocketNetworkServer CONSTRUCTOR");
            _updateScheduler = updateScheduler;
            _port = port;
        }

        #region INetworkServer implementation

        public  abstract void Start();

        public abstract void Stop();

        public virtual void Fail(Error err)
        {
            throw new NotImplementedException();
        }

        public void AddDelegate(INetworkServerDelegate dlg)
        {
            UnityEngine.Debug.Log("SocketNetworkServer AddDelegate");
            _delegates.Add(dlg);
            if(Running && dlg != null)
            {
                dlg.OnServerStarted();
            }
        }

        public void RemoveDelegate(INetworkServerDelegate dlg)
        {
            UnityEngine.Debug.Log("SocketNetworkServer RemoveDelegate");
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            UnityEngine.Debug.Log("SocketNetworkServer RegisterReceiver");
            _receiver = receiver;
        }

        public virtual int GetTimestamp()
        {
            throw new NotImplementedException();
        }

        public bool Running{ get; protected set; }

        public virtual string Id
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        #endregion

        #region INetworkMessageSender implementation

        public virtual INetworkMessage CreateMessage(NetworkMessageData data)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDisposable implementation

        public virtual void Dispose()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IUpdateable implementation

        public virtual void Update()
        {
            throw new NotImplementedException();
        }

        #endregion


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
