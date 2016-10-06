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
        public const int DefaultClientStartDelay = 3000;
        public const int DefaultClientSimulationDelay = 1000;

        public byte MaxPlayers = DefaultMaxPlayers;
        public int ClientStartDelay = DefaultClientStartDelay;
        public int ClientSimulationDelay = DefaultClientSimulationDelay;

        public override string ToString()
        {
            return string.Format("[ServerLockstepConfig\n" +
                "MaxPlayers:{0}\n" +
                "ClientStartDelay:{1}\n" +
                "ClientSimulationDelay:{2}]",
                MaxPlayers, ClientStartDelay, ClientSimulationDelay);
        }
    }

    public sealed class ServerLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkServerDelegate
    {
        class ClientData
        {
            public byte ClientId;
            public byte PlayerId = BadPlayerId;
            public bool Ready;

            public void Clear()
            {
                PlayerId = BadPlayerId;
                Ready = false;
            }
        }
            
        const byte BadPlayerId = byte.MaxValue;

        ServerLockstepController _serverLockstep;
        INetworkServer _server;
        Dictionary<byte, ClientData> _clients;
        INetworkMessageReceiver _receiver;

        public ServerLockstepConfig ServerConfig{ get; set; }
        public LockstepConfig Config
        {
            get
            {
                return _serverLockstep.Config;
            }

            set
            {
                _serverLockstep.Config = value;
            }
        }

        public ServerLockstepNetworkController(INetworkServer server, IUpdateScheduler scheduler=null)
        {
            ServerConfig = new ServerLockstepConfig();
            _clients = new Dictionary<byte, ClientData>();
            _server = server;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
            SetupServerLockstep(new ServerLockstepController(scheduler));
        }

        [Obsolete("Pass scheduler to constructor.")]
        public void Init(ServerLockstepController serverLockstep)
        {
            SetupServerLockstep(serverLockstep);
        }

        void SetupServerLockstep(ServerLockstepController serverLockstep)
        {
            DebugUtils.Assert(serverLockstep != null);
            _serverLockstep = serverLockstep;
            _serverLockstep.TurnReady += OnServerTurnReady;
            if(_localClient != null)
            {
                _serverLockstep.RegisterLocalClient(_localClient, _localFactory);
            }
        }

        public void Update()
        {
            _serverLockstep.Update();
        }

        public void Update(int dt)
        {
            _serverLockstep.Update(dt);
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
                SendTurn(turnData, itr.Current.Key);
            }
            itr.Dispose();
        }

        void SendTurn(ServerLockstepTurnData turnData, byte client)
        {
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ConfirmTurn,
                ClientId = client
            }, turnData);
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
            _serverLockstep.AddCommand(command);
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
                return ServerConfig.MaxPlayers;
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
                return PlayerCount >= ServerConfig.MaxPlayers;
            }
        }

        public int UpdateTime
        {
            get
            {
                return _serverLockstep.UpdateTime;
            }
        }            

        public int CommandDeltaTime
        {
            get
            {
                return _serverLockstep.CommandDeltaTime;
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
            if(ReadyPlayerCount == ServerConfig.MaxPlayers)
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
                              ServerConfig.ClientStartDelay,
                              client.PlayerId);
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.AllPlayersReady,
                    ClientId = client.ClientId
                }, msg);
            }
            itr.Dispose();

            if(_serverLockstep != null)
            {
                _serverLockstep.Start(ServerConfig.ClientStartDelay - ServerConfig.ClientSimulationDelay);
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
            }, new ClientSetupMessage(Config));

            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                SendTurn(itr.Current, clientId);
            }
            itr.Dispose();
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
            UnregisterLocalClient();
        }

        #region local client

        ClientLockstepController _localClient;
        LockstepCommandFactory _localFactory;
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

        public void UnregisterLocalClient()
        {
            if(_serverLockstep != null)
            {
                _serverLockstep.UnregisterLocalClient();
            }
            _localClient = null;
            _localFactory = null;
            _localClientData = null;
        }

        public void RegisterLocalClient(ClientLockstepController ctrl, LockstepCommandFactory factory)
        {
            UnregisterLocalClient();
            _localClient = ctrl;
            _localFactory = factory;                
            _localClientData = new ClientData();
            if(_serverLockstep != null)
            {
                _serverLockstep.RegisterLocalClient(ctrl, _localFactory);
            }
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
                _localClient.Start(ServerConfig.ClientStartDelay);
            }
        }

        #endregion
    }
}
