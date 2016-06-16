﻿using System;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;
using System.Collections.Generic;
using UnityEngine;
using SocialPoint.Utils;
using SocialPoint.Lockstep;
using SocialPoint.Lockstep.Network;
using System.IO;
using SocialPoint.Lockstep.Network.UNet;

namespace SocialPoint.Lockstep.Network.UNet
{
    public class ServerNetworkController : IDisposable, INetworkMessageController, IUpdateable
    {
        const int _packetSize = 1440;

        int _port;
        int _clientsCount;
        ServerLockstepNetworkController _serverLockstepNetwork;
        NetworkServerSimple _server;
        IUpdateScheduler _updateScheduler;

        public short PlayerConnectedMsgType  { get; protected set; }

        public short PlayerDisconnectedMsgType  { get; protected set; }

        public event Action<string> Log;

        Dictionary<int,int> _playerIdDictionary = new Dictionary<int, int>();

        public ServerNetworkController(int port,
                                       LockstepConfig lockstepConfig,
                                       IUpdateScheduler updateScheduler,
                                       int clientsCount,
                                       int startLockstepDelay,
                                       short playerConnectedMsgType = 1000, 
                                       short playerDisconnectedMsgType = 1001)
        {
            _port = port;
            _clientsCount = clientsCount;
            _updateScheduler = updateScheduler;
            PlayerConnectedMsgType = playerConnectedMsgType;
            PlayerDisconnectedMsgType = playerDisconnectedMsgType;
            _serverLockstepNetwork = new ServerLockstepNetworkController(this, clientsCount, lockstepConfig, startLockstepDelay);
            _updateScheduler.Add(this);
        }

        public void Start()
        {
            if(_server == null)
            {
                ConnectionConfig config = new ConnectionConfig{ PacketSize = _packetSize };
                config.AddChannel(QosType.Reliable);
                config.AddChannel(QosType.Unreliable);
                var topology = new HostTopology(config, _clientsCount);
                _server = new NetworkServerSimple();
                _server.Configure(topology);

                RegisterHandlers();
                _server.Listen(_port);
                _serverLockstepNetwork.Start();
            }
        }

        public void Stop()
        {
            if(_server != null)
            {
                _serverLockstepNetwork.Stop();
                UnregisterHandlers();
                _server.DisconnectAllConnections();
                _server.Stop();
                _server = null;
            }
        }

        public void Init(ServerLockstepController serverLockstep,
                         LockstepCommandDataFactory commandDataFactory)
        {
            _serverLockstepNetwork.Init(serverLockstep, commandDataFactory);
        }

        void RegisterHandlers()
        {
            _server.RegisterHandler(MsgType.Connect, OnConnectReceived);
            _server.RegisterHandler(MsgType.Disconnect, OnDisconnectReceived);
            _server.RegisterHandler(MsgType.Error, OnErrorReceived);
        }

        void UnregisterHandlers()
        {
            _server.UnregisterHandler(MsgType.Connect);
            _server.UnregisterHandler(MsgType.Disconnect);
            _server.UnregisterHandler(MsgType.Error);
        }

        void SendMessageToAll(short msgType, MessageBase msg, NetworkChannel channel = NetworkChannel.Reliable)
        {
            var channelId = GetChannelIdByNetworkChannel(channel);
            var connections = _server.connections;
            for(int i = 0; i < connections.Count; ++i)
            {
                var connection = connections[i];
                if(connection != null)
                {
                    connection.SendByChannel(msgType, msg, channelId);
                }
            }
        }

        void OnConnectReceived(NetworkMessage netMsg)
        {
            _playerIdDictionary[netMsg.conn.connectionId] = _serverLockstepNetwork.OnClientConnected(netMsg.conn.connectionId);
            SendMessageToAll(PlayerConnectedMsgType, new IntegerMessage(_playerIdDictionary[netMsg.conn.connectionId]));
        }

        void OnDisconnectReceived(NetworkMessage netMsg)
        {
            _serverLockstepNetwork.OnClientDisconnected(netMsg.conn.connectionId);
            SendMessageToAll(PlayerDisconnectedMsgType, new IntegerMessage(_playerIdDictionary[netMsg.conn.connectionId]));
        }

        void OnErrorReceived(NetworkMessage netMsg)
        {
            SendMessageToAll(MsgType.Error, netMsg.ReadMessage<ErrorMessage>());
        }

        void WriteLog(string logText)
        {
            if(Log != null)
            {
                Log(logText);
            }
        }

        public void Dispose()
        {
            Stop();
            if(_serverLockstepNetwork != null)
            {
                _serverLockstepNetwork.Dispose();
            }
            _updateScheduler.Remove(this);
        }

        #region INetworkMessageController implementation

        Dictionary<short, BaseNetworkMessageHandler> _handlers = new Dictionary<short, BaseNetworkMessageHandler>();

        public void RegisterHandler(short msgType, Action<NetworkMessageData> handler)
        {
            var msgHandler = new NetworkMessageHandler(msgType, handler);
            _handlers.Add(msgType, msgHandler);
            msgHandler.RegisterServer(_server);
        }

        public void RegisterSyncHandler(short msgType, Action<SyncNetworkMessageData> handler)
        {
            var msgHandler = new SyncNetworkMessageHandler(msgType, handler);
            _handlers.Add(msgType, msgHandler);
            msgHandler.RegisterServer(_server);
        }

        public void UnregisterHandler(short msgType)
        {
            BaseNetworkMessageHandler handler;
            if(_handlers.TryGetValue(msgType, out handler))
            {
                handler.UnregisterServer(_server);
                _handlers.Remove(msgType);
            }
        }

        int GetChannelIdByNetworkChannel(NetworkChannel channel)
        {
            return channel == NetworkChannel.Reliable ? 0 : 1;
        }

        public void Send(short msgType, INetworkMessage msg, NetworkChannel channel = NetworkChannel.Reliable, int connectionId = 0)
        {
            var channelId = GetChannelIdByNetworkChannel(channel);
            _server.FindConnection(connectionId).SendByChannel(msgType, new NetworkMessageWrapper(msg), channelId);
        }

        public void SendToAll(short msgType, INetworkMessage msg, NetworkChannel channel = NetworkChannel.Reliable)
        {
            SendMessageToAll(msgType, new NetworkMessageWrapper(msg), channel);
        }

        #endregion

        #region IUpdateable implementation

        public void Update()
        {
            if(_server != null)
            {
                _server.Update();
            }
        }

        #endregion
    }
}