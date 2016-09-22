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
        public const byte LockstepCommand = 2;
        public const byte ConfirmTurns = 3;
        public const byte ConfirmTurnsReception = 4;
        public const byte ClientSetup = 5;
        public const byte PlayerReady = 6;
        public const byte AllPlayersReady = 7;
    }

    public sealed class ServerLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkServerDelegate
    {
        class LockstepClientData
        {
            public byte ClientId;
            public List<byte> Players = new List<byte>();
        }

        int _maxPlayers;
        ServerLockstepController _serverLockstep;
        int _startDelay;
        LockstepConfig _lockstepConfig;
        INetworkServer _server;
        Dictionary<byte, LockstepClientData> _clients;
        INetworkMessageReceiver _receiver;

        public ServerLockstepNetworkController(INetworkServer server,
                                               LockstepConfig lockstepConfig = null,
                                               int maxPlayers = 2,
                                               int startDelay = 5000)
        {
            if(lockstepConfig == null)
            {
                lockstepConfig = new LockstepConfig();
            }
            _clients = new Dictionary<byte, LockstepClientData>();
            _server = server;
            _maxPlayers = maxPlayers;
            _lockstepConfig = lockstepConfig;
            _startDelay = startDelay;
        }

        public void Init(ServerLockstepController serverLockstep)
        {
            _serverLockstep = serverLockstep;
            _serverLockstep.CommandStep = _lockstepConfig.CommandStep;
            _serverLockstep.SendClientTurnData = SendClientTurnData;
            _server.RegisterReceiver(this);
            _server.AddDelegate(this);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        void SendClientTurnData(byte clientId, ServerLockstepTurnData[] turnData)
        {
            var action = new ServerConfirmTurnsMessage();
            action.ConfirmedTurns = turnData;

            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ConfirmTurns,
                Unreliable = true,
                ClientId = clientId
            }, action);
        }

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            LockstepClientData clientData;
            if(!_clients.TryGetValue(data.ClientId, out clientData))
            {
                return;
            }
            switch(data.MessageType)
            {
            case LockstepMsgType.ConfirmTurnsReception:
                OnConfirmTurnsReceptionReceived(clientData, reader);
                break;
            case LockstepMsgType.LockstepCommand:
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

        void OnConfirmTurnsReceptionReceived(LockstepClientData clientData, IReader reader)
        {
            var msg = new ConfirmTurnsReceptionMessage();
            msg.Deserialize(reader);
            for(int i = 0; i < msg.ConfirmedTurns.Length; ++i)
            {
                _serverLockstep.OnClientTurnReceptionConfirmed(clientData.ClientId, msg.ConfirmedTurns[i]);
            }
        }

        void OnLockstepCommandReceived(LockstepClientData clientData, IReader reader)
        {
            var command = new ServerLockstepCommandData();
            command.Deserialize(reader);
            command.ClientId = clientData.ClientId;
            _serverLockstep.OnClientCommandReceived(command);
        }

        byte FindPlayerClient(byte playerId)
        {
            var itr = _clients.GetEnumerator();
            byte clientId = 0;
            while(itr.MoveNext())
            {
                var client = itr.Current.Value;
                if(client.Players.Contains(playerId))
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
                    if(FindPlayerClient(id) == 0 && !IsLocalPlayerId(id))
                    {
                        break;
                    }
                }
                return id;
            }
        }

        public int MaxPlayers
        {
            get
            {
                return _maxPlayers;
            }
        }

        public int PlayerCount
        {
            get
            {
                var itr = _clients.GetEnumerator();
                var count = 0;
                while(itr.MoveNext())
                {
                    var client = itr.Current.Value;
                    count += client.Players.Count;
                }
                itr.Dispose();
                count += _localPlayerIds.Count;
                return count;
            }
        }

        public bool Full
        {
            get
            {
                return PlayerCount >= MaxPlayers;
            }
        }

        public bool Running
        {
            get
            {
                return _server.Running && _serverLockstep.Running;
            }
        }

        byte OnPlayerReadyReceived(LockstepClientData clientData)
        {
            var playerId = FreePlayerId;
            clientData.Players.Add(playerId);
            CheckAllPlayersReady();
            return playerId;
        }

        void CheckAllPlayersReady()
        {
            if(PlayerCount == _maxPlayers)
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
                              _startDelay,
                              client.Players.ToArray());
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
                    ts + _startDelay - _serverLockstep.CommandStep,
                    new List<byte>(_clients.Keys).ToArray());
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
            var clientData = new LockstepClientData() {
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
            if(_localPlayerIds != null)
            {
                _localPlayerIds.Clear();
            }
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
        }

        #region local client

        ClientLockstepController _localClient;
        List<byte> _localPlayerIds = new List<byte>();

        public int LocalPlayerCount
        {
            get
            {
                if(_localPlayerIds == null)
                {
                    return 0;
                }
                return _localPlayerIds.Count;
            }
        }

        public bool GetLocalPlayerId(int position, out byte playerId)
        {
            if(_localPlayerIds == null || _localPlayerIds.Count <= position)
            {
                playerId = byte.MaxValue;
                return false;
            }
            playerId = _localPlayerIds[position];
            return true;
        }

        public bool IsLocalPlayerId(byte playerId)
        {
            return _localPlayerIds != null && _localPlayerIds.Contains(playerId);
        }

        public void RegisterLocalClient(ClientLockstepController ctrl, LockstepCommandFactory factory)
        {
            _localClient = ctrl;
            _serverLockstep.RegisterLocalClient(ctrl, factory);
        }

        public byte LocalPlayerReady()
        {
            var playerId = FreePlayerId;
            _localPlayerIds.Add(playerId);
            CheckAllPlayersReady();
            return playerId;
        }

        void StartLocalClientOnAllPlayersReady()
        {
            if(_localClient != null)
            {
                _localClient.Start(TimeUtils.TimestampMilliseconds + _startDelay);
            }
        }

        #endregion
    }
}
