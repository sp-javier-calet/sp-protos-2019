using System;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public static class LockstepMsgType
    {
        public const byte Command = 2;
        public const byte ConfirmTurn = 3;
        public const byte ClientSetup = 5;
        public const byte PlayerReady = 6;
        public const byte AllPlayersReady = 7;
    }

    [Serializable]
    public sealed class ServerLockstepConfig
    {
        public const byte DefaultMaxPlayers = 2;
        public const int DefaultStartDelay = 3000;

        public byte MaxPlayers = DefaultMaxPlayers;
        public int StartDelay = DefaultStartDelay;
    }

    public sealed class ServerLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkServerDelegate
    {
        class ClientData
        {
            public byte ClientId;
            public byte PlayerId;
            public bool Ready;

            public void Clear()
            {
                PlayerId = BadPlayerId;
                Ready = false;
            }
        }
            
        const byte BadPlayerId = byte.MaxValue;

        ServerLockstepController _serverLockstep;
        LockstepConfig _lockstepConfig;
        ServerLockstepConfig _serverConfig;
        INetworkServer _server;
        Dictionary<byte, ClientData> _clients;
        INetworkMessageReceiver _receiver;

        public ServerLockstepNetworkController(INetworkServer server,
                                               LockstepConfig lockstepConfig = null,
                                               ServerLockstepConfig serverConfig = null)
        {
            if(lockstepConfig == null)
            {
                lockstepConfig = new LockstepConfig();
            }
            if(serverConfig == null)
            {
                serverConfig = new ServerLockstepConfig();
            }
            _clients = new Dictionary<byte, ClientData>();
            _server = server;
            _serverConfig = serverConfig;
            _lockstepConfig = lockstepConfig;

        }

        public void Init(ServerLockstepController serverLockstep)
        {
            _serverLockstep = serverLockstep;
            _serverLockstep.CommandStep = _lockstepConfig.CommandStep;
            _serverLockstep.TurnReady = OnServerTurnReady;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        void OnServerTurnReady(ServerLockstepTurnData turnData)
        {
            var itr = _clients.GetEnumerator();
            while(itr.MoveNext())
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.ConfirmTurn,
                    Unreliable = true,
                    ClientId = itr.Current.Key
                }, turnData);
            }
            itr.Dispose();
        }

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            ClientData clientData;
            if(!_clients.TryGetValue(data.ClientId, out clientData))
            {
                return;
            }
            switch(data.MessageType)
            {
            case LockstepMsgType.Command:
                OnLockstepCommandReceived(clientData, reader);
                break;
            case LockstepMsgType.PlayerReady:
                OnPlayerReadyReceived(clientData);
                break;
            default:
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
                break;
            }
        }

        void OnLockstepCommandReceived(ClientData clientData, IReader reader)
        {
            var command = new ServerLockstepCommandData();
            command.Deserialize(reader);
            command.ClientId = clientData.ClientId;
            _serverLockstep.OnClientCommandReceived(command);
        }

        byte FindPlayerClient(byte playerId)
        {
            var itr = _clients.GetEnumerator();
            byte clientId = BadPlayerId;
            while(itr.MoveNext())
            {
                var client = itr.Current.Value;
                if(client.Ready && client.PlayerId == playerId)
                {
                    clientId = client.ClientId;
                    break;
                }
            }
            itr.Dispose();
            return clientId;
        }

        byte FreePlayerId
        {
            get
            {
                byte id = 0;
                for(; id < byte.MaxValue; id++)
                {
                    if(FindPlayerClient(id) != BadPlayerId)
                    {
                        continue;
                    }
                    if(IsLocalPlayerId(id))
                    {
                        continue;
                    }
                    break;
                }
                return id;
            }
        }

        public int PlayerCount
        {
            get
            {
                return ClientCount;
            }
        }

        public int ReadyPlayerCount
        {
            get
            {
                var itr = _clients.GetEnumerator();
                var count = 0;
                while(itr.MoveNext())
                {
                    var client = itr.Current.Value;
                    if(client != null && client.Ready)
                    {
                        count++;
                    }
                }
                itr.Dispose();
                if(_localClientData != null && _localClientData.Ready)
                {
                    count++;
                }
                return count;
            }
        }

        public byte MaxPlayers
        {
            get
            {
                return _serverConfig.MaxPlayers;
            }
        }

        public int ClientCount
        {
            get
            {
                var count = _clients.Count;
                if(_localClientData != null)
                {
                    count++;
                }
                return count;
            }
        }

        public bool Running
        {
            get
            {
                return _server.Running && _serverLockstep.Running;
            }
        }
            
        public bool Full
        {
            get
            {
                return PlayerCount >= _serverConfig.MaxPlayers;
            }
        }

        byte OnPlayerReadyReceived(ClientData clientData)
        {
            var playerId = FreePlayerId;
            clientData.Ready = true;
            clientData.PlayerId = playerId;
            CheckAllPlayersReady();
            return playerId;
        }

        void CheckAllPlayersReady()
        {
            if(ReadyPlayerCount == _serverConfig.MaxPlayers)
            {
                StartLockstep();
            }
        }

        void StartLockstep()
        {
            var itr = _clients.GetEnumerator();
            while(itr.MoveNext())
            {
                var client = itr.Current.Value;
                var msg = new AllPlayersReadyMessage(
                              _server.GetTimestamp(),
                              _serverConfig.StartDelay,
                              client.PlayerId);
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.AllPlayersReady,
                    ClientId = client.ClientId
                }, msg);
            }
            itr.Dispose();

            if(_serverLockstep != null)
            {
                var ts = TimeUtils.TimestampMilliseconds;
                _serverLockstep.Start(
                    ts + _serverConfig.StartDelay - _serverLockstep.CommandStep);
            }

            StartLocalClientOnAllPlayersReady();
        }

        public void OnServerStarted()
        {
        }

        public void OnServerStopped()
        {
            Stop();
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
        }

        public void OnNetworkError(Error e)
        {
        }

        public void OnClientConnected(byte clientId)
        {
            if(_clients.ContainsKey(clientId))
            {
                return;
            }
            var clientData = new ClientData() {
                ClientId = clientId
            };
            _clients[clientId] = clientData;

            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientSetup,
                ClientId = clientId,
                Unreliable = false
            }, new ClientSetupMessage(_lockstepConfig));
        }

        public void OnClientDisconnected(byte clientId)
        {
            _clients.Remove(clientId);
        }

        public void Stop()
        {
            if(_clients != null)
            {
                _clients.Clear();
            }
            if (_localClientData != null)
            {
                _localClientData.Clear();
            }
            if (_serverLockstep != null)
            {
                _serverLockstep.Stop();
            }
        }

        public void Dispose()
        {
            Stop();
            _server.RegisterReceiver(null);
            _server.RemoveDelegate(this);
            if(_serverLockstep != null)
            {
                _serverLockstep.Dispose();
            }
        }

        #region local client

        ClientLockstepController _localClient;
        ClientData _localClientData;

        public byte LocalPlayerId
        {
            get
            {
                if(_localClientData == null)
                {
                    return BadPlayerId;
                }
                return _localClientData.PlayerId;
            }
        }

        public bool IsLocalPlayerId(byte id)
        {
            return _localClientData != null && _localClientData.Ready && _localClientData.PlayerId == id;
        }

        public void RegisterLocalClient(ClientLockstepController ctrl, LockstepCommandFactory factory)
        {
            _localClient = ctrl;
            _localClientData = new ClientData();
            _serverLockstep.RegisterLocalClient(ctrl, factory);
        }

        public byte LocalPlayerReady()
        {
            if(_localClientData == null)
            {
                return BadPlayerId;
            }
            var playerId = FreePlayerId;
            _localClientData.Ready = true;
            _localClientData.PlayerId = playerId;
            CheckAllPlayersReady();
            return playerId;
        }

        void StartLocalClientOnAllPlayersReady()
        {
            if(_localClient != null)
            {
                _localClient.Start(TimeUtils.TimestampMilliseconds + _serverConfig.StartDelay);
            }
        }

        #endregion
    }
}
