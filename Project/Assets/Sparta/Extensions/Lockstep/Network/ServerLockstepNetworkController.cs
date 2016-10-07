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
        public const byte Turn = 3;
        public const byte ClientSetup = 5;
        public const byte PlayerReady = 6;
        public const byte ClientStart = 7;
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
            public byte PlayerNumber;
            public bool Ready;
        }

        ServerLockstepController _serverLockstep;
        INetworkServer _server;
        Dictionary<byte, ClientData> _clients;
        Dictionary<uint, byte> _playerNums;
        INetworkMessageReceiver _receiver;

        ClientLockstepController _localClient;
        LockstepCommandFactory _localFactory;
        ClientData _localClientData;

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

        public ServerLockstepNetworkController(INetworkServer server, IUpdateScheduler scheduler = null)
        {
            ServerConfig = new ServerLockstepConfig();
            _clients = new Dictionary<byte, ClientData>();
            _playerNums = new Dictionary<uint, byte>();
            _localClientData = new ClientData();
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
                var client = itr.Current.Value;
                if(client.Ready)
                {
                    SendTurn(turnData, client.ClientId);
                }
            }
            itr.Dispose();
        }

        void SendTurn(ServerLockstepTurnData turnData, byte client)
        {
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.Turn,
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
                OnPlayerReadyReceived(clientData, reader);
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

        bool FindPlayerClient(byte playerNum, out byte clientId)
        {
            var itr = _clients.GetEnumerator();
            bool found = false;
            clientId = 0;
            while(itr.MoveNext())
            {
                var client = itr.Current.Value;
                if(client.Ready && client.PlayerNumber == playerNum)
                {
                    clientId = client.ClientId;
                    found = true;
                    break;
                }
            }
            itr.Dispose();
            return found;
        }

        byte FreePlayerNumber
        {
            get
            {
                byte num = 0;
                byte clientId;
                for(; num < byte.MaxValue; num++)
                {
                    if(FindPlayerClient(num, out clientId))
                    {
                        continue;
                    }
                    if(IsLocalPlayerNumber(num))
                    {
                        continue;
                    }
                    break;
                }
                return num;
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
                if(_localClientData.Ready)
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
                if(_localClient != null)
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

        public int ClientUpdateTime
        {
            get
            {
                return UpdateTime - ServerConfig.ClientSimulationDelay;
            }
        }

        public int CommandDeltaTime
        {
            get
            {
                return _serverLockstep.CommandDeltaTime;
            }
        }

        void OnPlayerReadyReceived(ClientData clientData, IReader reader)
        {
            if(clientData.Ready)
            {
                // client sent multiple ready messages
                return;
            }
            var msg = new PlayerReadyMessage();
            msg.Deserialize(reader);
            var playerNum = FreePlayerNumber;
            clientData.Ready = true;
            if(!Running)
            {
                clientData.PlayerNumber = playerNum;
                _playerNums[msg.PlayerId] = clientData.PlayerNumber;
                CheckAllPlayersReady();
                return;
            }
            // client reconnected
            if(!_playerNums.TryGetValue(msg.PlayerId, out playerNum))
            {
                // bad player, kick him out!
                _clients.Remove(clientData.ClientId);
                return;
            }

            // send the old turns
            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                SendTurn(itr.Current, clientData.ClientId);
            }
            itr.Dispose();

            clientData.PlayerNumber = playerNum;
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientStart,
                ClientId = clientData.ClientId
            }, new ClientStartMessage(
                _server.GetTimestamp(),
                ClientUpdateTime,
                clientData.PlayerNumber
            ));
        }

        void CheckAllPlayersReady()
        {
            if(!_serverLockstep.Running && ReadyPlayerCount == ServerConfig.MaxPlayers)
            {
                StartLockstep();
            }
        }

        void StartLockstep()
        {
            if(_serverLockstep != null)
            {
                _serverLockstep.Start(ServerConfig.ClientSimulationDelay - ServerConfig.ClientStartDelay);
            }
            var itr = _clients.GetEnumerator();
            while(itr.MoveNext())
            {
                var clientData = itr.Current.Value;
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.ClientStart,
                    ClientId = clientData.ClientId
                }, new ClientStartMessage(
                    _server.GetTimestamp(),
                    ClientUpdateTime,
                    clientData.PlayerNumber
                ));
            }
            itr.Dispose();
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
        }

        public void OnClientDisconnected(byte clientId)
        {
            _clients.Remove(clientId);
        }

        public void Stop()
        {
            _clients.Clear();
            _playerNums.Clear();
            _localClientData.Ready = false;
            if(_serverLockstep != null)
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

        public byte LocalPlayerNumber
        {
            get
            {
                return _localClientData.PlayerNumber;
            }
        }

        public bool IsLocalPlayerNumber(byte num)
        {
            return _localClient != null && _localClientData.Ready && _localClientData.PlayerNumber == num;
        }

        public void UnregisterLocalClient()
        {
            if(_serverLockstep != null)
            {
                _serverLockstep.UnregisterLocalClient();
            }
            _localClient = null;
            _localFactory = null;
            _localClientData.Ready = false;
        }

        public void RegisterLocalClient(ClientLockstepController ctrl, LockstepCommandFactory factory)
        {
            UnregisterLocalClient();
            _localClient = ctrl;
            _localFactory = factory;
            if(_serverLockstep != null)
            {
                _serverLockstep.RegisterLocalClient(ctrl, _localFactory);
            }
        }

        public void LocalPlayerReady()
        {
            if(_localClientData.Ready)
            {
                return;
            }
            var playerNum = FreePlayerNumber;
            _localClientData.Ready = true;
            if(!Running)
            {
                _localClientData.PlayerNumber = playerNum;
                CheckAllPlayersReady();
                return;
            }
            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                _localClient.AddConfirmedTurn(itr.Current.ToClient(_localFactory));
            }
            itr.Dispose();
            _localClient.Start(ClientUpdateTime);
        }

        void StartLocalClientOnAllPlayersReady()
        {
            if(_localClient != null)
            {
                _localClient.Start(ClientUpdateTime);
            }
        }

        #endregion
    }
}
