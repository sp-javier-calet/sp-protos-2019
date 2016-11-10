using System;
using System.Collections.Generic;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Attributes;

namespace SocialPoint.Lockstep.Network
{
    public static class LockstepMsgType
    {
        public const byte Command = 2;
        public const byte Turn = 3;
        public const byte EmptyTurn = 4;
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
            "ClientSimulationDelay:{2}\n" +
            "]",
                MaxPlayers, ClientStartDelay,
                ClientSimulationDelay);
        }
    }

    public sealed class ServerLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkServerDelegate
    {
        class ClientData
        {
            public byte ClientId;
            public bool Ready;
            public string PlayerId;
            public byte PlayerNumber;
        }

        ServerLockstepController _serverLockstep;
        INetworkServer _server;
        List<ClientData> _clients;
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

        public LockstepGameParams GameParams
        {
            get
            {
                return _serverLockstep.GameParams;
            }
        }

        public ServerLockstepNetworkController(INetworkServer server, IUpdateScheduler scheduler = null)
        {
            ServerConfig = new ServerLockstepConfig();
            _clients = new List<ClientData>();
            _serverLockstep = new ServerLockstepController(scheduler);
            _localClientData = new ClientData();

            _server = server;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
            _serverLockstep.TurnReady += OnServerTurnReady;
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
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(client.Ready)
                {
                    SendTurn(turnData, client.ClientId);
                }
            }
        }

        void SendTurn(ServerLockstepTurnData turnData, byte client)
        {
            if(ServerLockstepTurnData.IsNullOrEmpty(turnData))
            {
                _server.CreateMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.EmptyTurn,
                    ClientId = client
                }).Send();
            }
            else
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.Turn,
                    ClientId = client
                }, turnData);
            }
        }

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch(data.MessageType)
            {
            case LockstepMsgType.Command:
                OnLockstepCommandReceived(data.ClientId, reader);
                break;
            case LockstepMsgType.PlayerReady:
                OnPlayerReadyReceived(data.ClientId, reader);
                break;
            default:
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
                break;
            }
        }

        void OnLockstepCommandReceived(byte clientId, IReader reader)
        {
            var client = FindClientByClientId(clientId);
            if(client == null || !client.Ready)
            {
                // only ready clients can send commands
                return;
            }
            var command = new ServerLockstepCommandData();
            command.Deserialize(reader);
            // ignore client player number
            // maybe we could trigger a command failed if they don't match?
            command.PlayerNumber = client.PlayerNumber;
            _serverLockstep.AddCommand(command);
        }

        bool IsPlayerNumber(byte playerNum)
        {
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(client.PlayerNumber == playerNum)
                {
                    return true;
                }
            }
            return false;
        }

        byte FreePlayerNumber
        {
            get
            {
                byte num = 0;
                for(; num < byte.MaxValue; num++)
                {
                    if(IsPlayerNumber(num))
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
                var count = 0;
                for(var i = 0; i < _clients.Count; i++)
                {
                    if(_clients[i].Ready)
                    {
                        count++;
                    }
                }
                if( _localClientData.Ready)
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

        public int ClientCount{ get; private set; }

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
                return _serverLockstep.UpdateTime - ServerConfig.ClientSimulationDelay;
            }
        }

        public int CommandDeltaTime
        {
            get
            {
                return _serverLockstep.CommandDeltaTime;
            }
        }

        public int CurrentTurnNumber
        {
            get
            {
                return _serverLockstep.CurrentTurnNumber;
            }
        }

        List<string> PlayerIds
        {
            get
            {
                var ids = new SortedList<byte, string>();
                for(var i = 0; i < _clients.Count; i++)
                {
                    var client = _clients[i];
                    ids[client.PlayerNumber] = client.PlayerId;
                }
                ids[_localClientData.PlayerNumber] = _localClientData.PlayerId;
                return new List<string>(ids.Values);
            }
        }

        ClientData FindClientByPlayerId(string playerId)
        {
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(client.PlayerId == playerId)
                {
                    return client;
                }
            }
            return null;
        }

        ClientData FindClientByClientId(byte clientId)
        {
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(client.ClientId == clientId)
                {
                    return client;
                }
            }
            return null;
        }

        ClientData FindClientByPlayerNumber(byte playerNum)
        {
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(client.PlayerNumber == playerNum)
                {
                    return client;
                }
            }
            return null;
        }

        public string FindPlayerId(byte playerNum)
        {
            var client = FindClientByPlayerNumber(playerNum);
            if(client == null)
            {
                return null;
            }
            return client.PlayerId;
        }

        void OnPlayerReadyReceived(byte clientId, IReader reader)
        {
            var msg = new PlayerReadyMessage();
            msg.Deserialize(reader);

            var client = FindClientByPlayerId(msg.PlayerId);
            if(client == null)
            {
                // new client
                client = new ClientData {
                    PlayerId = msg.PlayerId,
                    PlayerNumber = FreePlayerNumber
                };
                _clients.Add(client);
            }
            client.ClientId = clientId;
            if(client.Ready)
            {
                // client sent multiple ready messages
                return;
            }
            client.Ready = true;
            if(!Running)
            {
                CheckAllPlayersReady();
                return;
            }
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientStart,
                ClientId = clientId
            }, new ClientStartMessage(
                _server.GetTimestamp(),
                ClientUpdateTime,
                PlayerIds
            ));

            // send the old turns
            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                SendTurn(itr.Current, client.ClientId);
            }
            itr.Dispose();
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
            DoStartLockstep();
        }

        void DoStartLockstep()
        {
            _serverLockstep.Start(ServerConfig.ClientSimulationDelay - ServerConfig.ClientStartDelay);
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.ClientStart,
                    ClientId = client.ClientId
                }, new ClientStartMessage(
                    _server.GetTimestamp(),
                    ClientUpdateTime,
                    PlayerIds
                ));
            }
            StartLocalClientOnAllPlayersReady();
        }

        void EndLockstep()
        {
            _serverLockstep.Stop();
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
            Stop();
        }

        public void OnClientConnected(byte clientId)
        {
            ClientCount++;
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientSetup,
                ClientId = clientId,
                Unreliable = false
            }, new ClientSetupMessage(Config, GameParams));
        }

        public void OnClientDisconnected(byte clientId)
        {
            ClientCount--;
            var client = FindClientByClientId(clientId);
            if(client != null)
            {
                client.Ready = false;
            }
        }

        public void Stop()
        {
            if(!Running)
            {
                return;
            }
            EndLockstep();
            _clients.Clear();
            _localClientData.Ready = false;
        }

        public void Dispose()
        {
            Stop();
            _server.RegisterReceiver(null);
            _server.RemoveDelegate(this);
            _serverLockstep.Dispose();
            UnregisterLocalClient();
        }

        public void Replay(ClientLockstepController client, LockstepCommandFactory factory)
        {
            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                var turn = itr.Current.ToClient(factory);
                client.AddConfirmedTurn(turn);
            }
            itr.Dispose();
        }

        #region local client

        public string LocalPlayerId
        {
            get
            {
                return _localClientData.PlayerId;
            }
        }

        public byte LocalPlayerNumber
        {
            get
            {
                return _localClientData.PlayerNumber;
            }
        }

        public bool IsLocalPlayerNumber(byte num)
        {
            return _localClientData.Ready && _localClientData.PlayerNumber == num;
        }

        public void ReplayLocalClient()
        {
            Replay(_localClient, _localFactory);
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

        public void LocalPlayerReady(string playerId = null)
        {
            if(playerId != null)
            {
                _localClientData.PlayerId = playerId;
            }
            var playerNum = FreePlayerNumber;
            _localClientData.Ready = true;
            if(!Running)
            {
                _localClientData.PlayerNumber = playerNum;
                _localClient.PlayerNumber = playerNum;
                CheckAllPlayersReady();
                return;
            }
            if(_localClient != null)
            {
                var itr = _serverLockstep.GetTurnsEnumerator();
                while(itr.MoveNext())
                {
                    _localClient.AddConfirmedTurn(itr.Current.ToClient(_localFactory));
                }
                itr.Dispose();
                _localClient.Start(ClientUpdateTime);
            }
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
