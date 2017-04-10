using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Matchmaking;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Network.ServerEvents;

namespace SocialPoint.Lockstep
{
    public static class LockstepMsgType
    {
        public const byte Command = 2;
        public const byte Turn = 3;
        public const byte EmptyTurns = 4;
        public const byte ClientSetup = 5;
        public const byte PlayerReady = 6;
        public const byte ClientStart = 7;
        public const byte PlayerFinish = 8;
        public const byte ClientEnd = 9;
        public const byte ClientConnectionStatus = 10;
    }

    [Serializable]
    public sealed class LockstepServerConfig
    {
        const string MaxPlayersAttrKey = "max_players";
        const string ClientStartDelayAttrKey = "client_start_delay";
        const string ClientSimulationDelayAttrKey = "client_simulation_delay";
        const string FinishOnClientDisconnectionAttrKey = "finish_on_client_disconnection";

        public const byte DefaultMaxPlayers = 2;
        public const int DefaultClientStartDelay = 3000;
        public const int DefaultClientSimulationDelay = 1000;
        public const bool DefaultFinishOnClientDisconnection = true;
        public const int DefaultMetricSendInterval = 10000;

        public byte MaxPlayers = DefaultMaxPlayers;
        public int ClientStartDelay = DefaultClientStartDelay;
        public int ClientSimulationDelay = DefaultClientSimulationDelay;
        public bool FinishOnClientDisconnection = DefaultFinishOnClientDisconnection;
        public int MetricSendInterval = DefaultMetricSendInterval;
        public override string ToString()
        {
            return string.Format("[LockstepServerConfig\n" +
            "MaxPlayers:{0}\n" +
            "ClientStartDelay:{1}\n" +
            "ClientSimulationDelay:{2}\n" +
            "]",
                MaxPlayers, ClientStartDelay,
                ClientSimulationDelay);
        }

        public Attr ToAttr()
        {
            var attrDic = new AttrDic();
            attrDic.Set(MaxPlayersAttrKey, new AttrInt(MaxPlayers));
            attrDic.Set(ClientStartDelayAttrKey, new AttrInt(ClientStartDelay));
            attrDic.Set(ClientSimulationDelayAttrKey, new AttrInt(ClientSimulationDelay));
            attrDic.Set(FinishOnClientDisconnectionAttrKey, new AttrBool(FinishOnClientDisconnection));
            return attrDic;
        }
    }

    public sealed class LockstepNetworkServer : IDisposable, INetworkMessageReceiver, INetworkServerDelegate, IMatchmakingServerDelegate, IServerEventTracker
    {
        class ClientData
        {
            public byte ClientId;
            public bool Ready;
            public string PlayerId;
            public byte PlayerNumber;
        }

        const string MatchStartMetricName = "multiplayer.lockstep.match_start";
        const string MatchEndMetricName = "multiplayer.lockstep.match_end";
        const string MatchEndCorrectedMetricName = "multiplayer.lockstep.match_corrected";

        IMatchmakingServer _matchmaking;

        LockstepServer _serverLockstep;

        public LockstepServer ServerLockstep
        {
            get
            {
                return _serverLockstep;
            }
        }

        INetworkServer _server;
        List<ClientData> _clients;
        Dictionary<uint, byte> _commandSenders;
        INetworkMessageReceiver _receiver;

        LockstepClient _localClient;
        LockstepCommandFactory _localFactory;
        ClientData _localClientData;

        public LockstepServerConfig ServerConfig{ get; set; }

        public event Action BeforeMatchStarts;
        public event Action<byte[]> MatchStarted;
        public event Action<Error> ErrorProduced;
        public event Action<Error, byte> CommandFailed;
        public event Action<Dictionary<byte, Attr>> MatchFinished;


        public const int CommandFailedErrorCode = 300;
        public const int MatchmakingErrorCode = 301;
        public const int NetworkErrorCode = 302;

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

        Action<Metric> _sendMetric;
        public Action<Metric> SendMetric
        {
            get
            {
                return _sendMetric;
            }

            set
            {
                _sendMetric = value;
                _serverLockstep.SendMetric = SendMetric;
            }
        }

        public Action<Network.ServerEvents.Log, bool> SendLog { get; set; }

        public Action<string, AttrDic, ErrorDelegate> SendTrack { get; set; }

        public LockstepNetworkServer(INetworkServer server, IMatchmakingServer matchmaking = null, IUpdateScheduler scheduler = null)
        {
            ServerConfig = new LockstepServerConfig();
            _clients = new List<ClientData>();
            _commandSenders = new Dictionary<uint, byte>();
            _serverLockstep = new LockstepServer(scheduler);
            _localClientData = new ClientData();
            PlayerResults = new Dictionary<byte, Attr>();
            _matchmaking = matchmaking;
            _server = server;

            _server.RegisterReceiver(this);
            _server.AddDelegate(this);

            _serverLockstep.TurnReady += OnServerTurnReady;
            _serverLockstep.EmptyTurnsReady += OnServerEmptyTurnsReady;
            if(_matchmaking != null)
            {
                _matchmaking.AddDelegate(this);
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

        void OnServerTurnReady(ServerTurnData turnData)
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

        void SendTurn(ServerTurnData turnData, byte client)
        {
            if(ServerTurnData.IsNullOrEmpty(turnData))
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.EmptyTurns,
                    ClientId = client
                }, new EmptyTurnsMessage(1));
            }
            else
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.Turn,
                    ClientId = client
                }, turnData);
            }
        }

        void OnServerEmptyTurnsReady(int emptyTurns)
        {
            var itr = _clients.GetEnumerator();
            while(itr.MoveNext())
            {
                var client = itr.Current;
                if(client.Ready)
                {
                    _server.SendMessage(new NetworkMessageData {
                        MessageType = LockstepMsgType.EmptyTurns,
                        ClientId = client.ClientId
                    }, new EmptyTurnsMessage(emptyTurns));
                }
            }
            itr.Dispose();
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
            case LockstepMsgType.PlayerFinish:
                OnPlayerFinishReceived(data.ClientId, reader);
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
            var command = new ServerCommandData();
            command.Deserialize(reader);
            // ignore client player number
            // maybe we could trigger a command failed if they don't match?
            command.PlayerNumber = client.PlayerNumber;
            if(_localClient != null)
            {
                _commandSenders[command.Id] = client.PlayerNumber;
            }
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

        bool HasClientFinished(ClientData client)
        {
            return client != null && client.PlayerId != null&& PlayerResults.ContainsKey(client.PlayerNumber);
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

        public int FinishedPlayerCount
        {
            get
            {
                var count = 0;
                for(var i = 0; i < _clients.Count; i++)
                {
                    if(HasClientFinished(_clients[i]))
                    {
                        count++;
                    }
                }
                if(HasClientFinished(_localClientData))
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

        public Dictionary<byte, Attr> PlayerResults{ get; private set; }

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
                if(_localClient != null && _localClientData.Ready)
                {
                    ids[_localClientData.PlayerNumber] = _localClientData.PlayerId;
                }
                return new List<string>(ids.Values);
            }
        }

        string MatchId
        {
            get
            {
                return _server.Id;
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
            else
            {
                SendClientStatusMessage(client, true);
            }
            client.ClientId = clientId;
            if(client.Ready)
            {
                // client sent multiple ready messages
                return;
            }
            if(HasClientFinished(client))
            {
                // client already sent result
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
            var itr = _serverLockstep.GetTurnsEnumerator(msg.CurrentTurn);
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
            if(BeforeMatchStarts != null)
            {
                BeforeMatchStarts();
            }
            if(_matchmaking != null && _matchmaking.Enabled)
            {
                var playerIds = PlayerIds;
                var matchId = MatchId;
                if(playerIds.Count > 0 && !string.IsNullOrEmpty(matchId))
                {
                    _matchmaking.LoadInfo(matchId, playerIds);
                    return;
                }
            }
            DoStartLockstep();
        }

        void IMatchmakingServerDelegate.OnMatchInfoReceived(byte[] info)
        {
            if(MatchStarted != null)
            {
                MatchStarted(info);
            }
            DoStartLockstep();
        }

        void IMatchmakingServerDelegate.OnError(Error ierr)
        {
            var err = new Error(MatchmakingErrorCode,
                string.Format("Matchmaking: {0}", ierr.Msg));
            err.Detail = ierr.Detail;
            OnError(err);
        }

        void IMatchmakingServerDelegate.OnResultsReceived(AttrDic results)
        {
            SendResults(results);
        }

        void SendResults(AttrDic results)
        {
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                if(results.ContainsKey(client.PlayerId))
                {
                    var result = results[client.PlayerId];
                    _server.SendMessage(new NetworkMessageData
                    {
                        MessageType = LockstepMsgType.ClientEnd,
                        ClientId = client.ClientId
                    }, new AttrMessage(result));
                }
            }
        }

        void DoStartLockstep()
        {
            _serverLockstep.Start(ServerConfig.ClientSimulationDelay - ServerConfig.ClientStartDelay);
            if(SendMetric != null)
            {
                SendMetric(new Metric(MetricType.Counter, MatchStartMetricName, 1));
            }
            if(SendTrack != null)
            {
                var data = new AttrDic();
                data.Set("unique_id", new AttrString(MatchId));
                data.Set("lockstep_server_config", ServerConfig.ToAttr());
                data.Set("lockstep_config", Config.ToAttr());
                SendTrack(MatchStartMetricName, data, null);
            }
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

        void OnPlayerFinishReceived(byte clientId, IReader reader)
        {
            var client = FindClientByClientId(clientId);
            if(client == null || HasClientFinished(client))
            {
                // client sent multiple end messages
                return;
            }
            var msg = new AttrMessage();
            msg.Deserialize(reader);
            PlayerResults[client.PlayerNumber] = msg.Data;
            CheckAllPlayersEnded();
        }

        void CheckAllPlayersEnded()
        {
            if(_serverLockstep.Running && FinishedPlayerCount == ReadyPlayerCount)
            {
                EndLockstep();
            }
        }

        void EndLockstep()
        {
            _serverLockstep.Stop();
            var results = PlayerResults;
            var originalResults = new Dictionary<byte, Attr>();
            {
                var itr = results.GetEnumerator();
                while(itr.MoveNext())
                {
                    originalResults[itr.Current.Key] = (Attr)itr.Current.Value.Clone();
                }
                itr.Dispose();
            }
            if(MatchFinished != null)
            {
                MatchFinished(results);
            }
            var resultsAttr = new AttrDic();
            {
                var itr = results.GetEnumerator();
                while(itr.MoveNext())
                {
                    var playerId = FindPlayerId(itr.Current.Key);
                    if(!string.IsNullOrEmpty(playerId))
                    {
                        resultsAttr[playerId] = itr.Current.Value;
                    }
                }
                itr.Dispose();
            }
            bool corrected = false;
            var keys = originalResults.Keys.GetEnumerator();
            while(keys.MoveNext())
            {
                if(results.ContainsKey(keys.Current))
                {
                    if(results[keys.Current] == originalResults[keys.Current])
                    {
                        continue;
                    }
                    else
                    {
                        corrected = true;
                    }
                }
                break;
            }
            keys.Dispose();
            if(SendMetric != null)
            {
                SendMetric(new Metric(MetricType.Counter, corrected? MatchEndCorrectedMetricName : MatchEndMetricName, 1));
            }

            if(SendTrack != null)
            {
                var data = new AttrDic();
                data.Set("unique_id", new AttrString(MatchId));
                var origResultsAttr = new AttrDic();
                {
                    var itr = results.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var playerId = FindPlayerId(itr.Current.Key);
                        if(!string.IsNullOrEmpty(playerId))
                        {
                            resultsAttr[playerId] = itr.Current.Value;
                        }
                    }
                    itr.Dispose();
                }
                data.Set("match_result", origResultsAttr);
                if(corrected)
                {
                    data.Set("match_result_corrected", resultsAttr);
                }
                SendTrack(corrected ? MatchEndCorrectedMetricName : MatchEndMetricName, null, null);
            }

            if(_matchmaking == null || !_matchmaking.Enabled)
            {
                SendResults(resultsAttr);
            }
            else
            {
                _matchmaking.NotifyResults(MatchId, resultsAttr);
            }
        }

        public void OnServerStarted()
        {
            _clients.Clear();
            PlayerResults.Clear();
        }

        public void OnServerStopped()
        {
            Stop();
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
        }

        public void OnNetworkError(Error ierr)
        {
            var err = new Error(NetworkErrorCode,
                string.Format("Network: {0}", ierr));
            OnError(err);
        }

        void OnError(Error err)
        {
            if (ErrorProduced != null)
            {
                ErrorProduced(err);
            }
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

                SendClientStatusMessage(client, false);
            }
            if(ServerConfig.FinishOnClientDisconnection)
            {
                CheckAllPlayersEnded();
            }
        }

        public void Stop()
        {
            if(_serverLockstep.Running)
            {
                EndLockstep();
            }
            _localClientData.Ready = false;
        }

        public void Fail(Error err)
        {
            _server.Fail(err);
            Stop();
        }

        public void Dispose()
        {
            Stop();
            _server.RegisterReceiver(null);
            _server.RemoveDelegate(this);

            _serverLockstep.TurnReady -= OnServerTurnReady;
            _serverLockstep.EmptyTurnsReady -= OnServerEmptyTurnsReady;

            if(_matchmaking != null)
            {
                _matchmaking.RemoveDelegate(this);
            }
            _serverLockstep.Dispose();
            UnregisterLocalClient();
        }

        public void Replay(LockstepClient client, LockstepCommandFactory factory)
        {
            var itr = _serverLockstep.GetTurnsEnumerator();
            while(itr.MoveNext())
            {
                var turn = itr.Current.ToClient(factory);
                client.AddConfirmedTurn(turn);
            }
            itr.Dispose();
        }

        void SendClientStatusMessage(ClientData client, bool connected)
        {
            if(!Running)
            {
                return;
            }

            for(var i = 0; i < _clients.Count; i++)
            {
                var other = _clients[i];
                if(other.Ready && !string.Equals(other.PlayerId, client.PlayerId, StringComparison.CurrentCultureIgnoreCase))
                {
                    _server.SendMessage(new NetworkMessageData {
                        MessageType = LockstepMsgType.ClientConnectionStatus,
                        ClientId = other.ClientId
                    }, new ClientChangedConnectionStatusMessage(client.ClientId, connected));
                }
            }
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
            if(_localClient != null)
            {
                _localClient.CommandFailed -= OnLocalClientCommandFailed;
            }
            _localClient = null;
            _localFactory = null;
            _localClientData.Ready = false;
        }

        public void RegisterLocalClient(LockstepClient ctrl, LockstepCommandFactory factory)
        {
            UnregisterLocalClient();
            _localClient = ctrl;
            _localFactory = factory;
            if(_localClient != null)
            {
                _localClient.CommandFailed += OnLocalClientCommandFailed;
            }
            if(_serverLockstep != null)
            {
                _serverLockstep.RegisterLocalClient(ctrl, _localFactory);
            }
        }

        void OnLocalClientCommandFailed(Error ierr, ClientCommandData cmd)
        {
            byte playerNum;
            _commandSenders.TryGetValue(cmd.Id, out playerNum);
            var err = new Error(CommandFailedErrorCode,
                string.Format("Command failed: {0}", ierr));
            if(CommandFailed != null)
            {
                CommandFailed(err, playerNum);
            }
            else
            {
                Fail(err);
            }
        }

        void OnLocalClientTurnApplied(ClientTurnData turn)
        {
            var itr = turn.GetCommandEnumerator();
            while(itr.MoveNext())
            {
                _commandSenders.Remove(itr.Current.Id);
            }
            itr.Dispose();
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

        public void LocalPlayerFinish(Attr result)
        {
            PlayerResults[_localClientData.PlayerNumber] = result;
        }

        void StartLocalClientOnAllPlayersReady()
        {
            if(_localClient != null)
            {
                _localClient.Start(ClientUpdateTime);
            }
        }

        public void OnClientMatchEnd()
        {
            CheckAllPlayersEnded();
        }

        #endregion
    }
}
