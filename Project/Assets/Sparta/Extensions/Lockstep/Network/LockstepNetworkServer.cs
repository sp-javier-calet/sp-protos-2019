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
        public const int DefaultMatchEndedWithoutConfirmationTimeout = 10;
        public const bool DefaultFinishOnClientDisconnection = true;
        public const int DefaultMetricSendInterval = 10000;
        public const bool DefaultAllowMatchStartWithOnePlayerReady = false;
        public const bool DefaultMatchmakingEnabled = true;

        public byte MaxPlayers = DefaultMaxPlayers;
        public int ClientStartDelay = DefaultClientStartDelay;
        public int ClientSimulationDelay = DefaultClientSimulationDelay;
        public int MatchEndedWithoutConfirmationTimeout = DefaultMatchEndedWithoutConfirmationTimeout;
        public bool FinishOnClientDisconnection = DefaultFinishOnClientDisconnection;
        public int MetricSendInterval = DefaultMetricSendInterval;
        public bool AllowMatchStartWithOnePlayerReady = DefaultAllowMatchStartWithOnePlayerReady;
        public bool MatchmakingEnabled = DefaultMatchmakingEnabled;

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
            public string PlayerToken;
            public long PlayerId;
            public byte PlayerNumber;
            public string Version;
            public int Connections;
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

        DateTime _matchEndTimeOut;

        int _logCount;
        bool _matchEnded;

        public LockstepServerConfig ServerConfig{ get; set; }

        public event Action BeforeMatchStarts;
        public event Action<byte[]> MatchStarted;
        public event Action<Error> ErrorProduced;
        public event Action<Error, byte> CommandFailed;
        public event Action<Dictionary<byte, Attr>, AttrDic> MatchFinished;
        public event Action<byte> OnClientConnectedEvent;
        public event Action<byte> OnClientDisconnectedEvent;

        public Func<bool> HasMatchEnded;

        public const int CommandFailedErrorCode = 300;
        public const int MatchmakingErrorCode = 301;
        public const int NetworkErrorCode = 302;

        const string PlayerTokenKey = "player{0}_token";
        const string PlayerIDKey = "player_{0}";
        const string PlayerFinishedIDKey = "player{0}_finished";
        const string TurnNumberParam = "turn_number";

        const string ErrorMessageClientNotFound = "Client not found for playerId: {0}";

        const string CommandFormat = "Command failed: {0}";

        const string LogMessageLockstepFormat = "Lockstep: {0}";
        const string LogMessageMatchmakingFormat = "Matchmaking: {0}";
        const string LogMessageNetworkFormat = "Network: {0}";

        const string LogMessageStart = "Start";
        const string LogMessageStopped = "Stopped";
        const string LogMessageEnded = "Ended";
        const string LogMessageClientConnected = "Client Connected";
        const string LogMessageClientDisconnected = "Client Disconnected";

        const string LogMessageAlreadyEnded = "Trying to Start a match that has already ended!";
        const string LogMessageStartingLockstep = "Starting Lockstep Server";
        const string LogMessagePlayerTryEnd = "Player said that match ended but game says it had not.";
        const string LogMessageTimedOut = "Timed Out After Player Said Game Was Over";
        const string LogMessageNoClients = "Match had no clients!";
        const string LogMessageDataIsEmpty = "Player Result or Custom Data is Empty!";
        const string LogMessageMatchEndNotification = "Match End Notification Error";
        const string LogMessageCouldNotFindPlayerID = "Client not found for given PlayerID";
        const string LogMessageNoPlayerTokenOnMatchInfo = "Match Info response has no Player Token";

        const string LogMessageIDKey = "id";
        const string LogMessageUniqueIDKey = "unique_id";
        const string LogMessageClientIDKey = "client_id";
        const string LogMessagePlayerIDKey = "player_id";
        const string LogMessageMatchIDKey = "match_id";

        const string ParamServerConfig = "lockstep_server_config";
        const string ParamConfig = "lockstep_config";


        const string ParamMatchResult = "match_result";
        const string ParamMatchResultCorrected = "match_result_corrected";

        const string ParamPlayers = "players";
        const string ParamPlayer = "player{0}";
        const string ParamDuration = "duration";
        const string ParamModified = "modified";
        const string ParamFlags = "flags";

        const string ParamResponseBody = "response_body";
        const string ParamDetail = "detail";

        const string ParamBody = "body";

        const string ParamsTypeBody = "bodyParams";
        const string ParamsTypeParams = "params";
        const string ParamsTypeQuery = "queryParams";

        const string ParamLogMatchID = "{0}.{1}";
        const string ParamLogModified = "{0}.{1}";

        const string ParamLogPlayersFormat = "{0}.players{1}.{2}";
        const string ParamLogPlayersFlagsFormat = "{0}.players{1}.{2}{3}";

        const string ParamLogNumber = "log_number";


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

            _matchEndTimeOut = DateTime.MinValue;

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

            CheckMatchEndTimeout();
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
            var clientIds = new List<byte>();
            clientIds.Add(client);
            if(ServerTurnData.IsNullOrEmpty(turnData))
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.EmptyTurns,
                    ClientIds = clientIds
                }, new EmptyTurnsMessage(1));
            }
            else
            {
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.Turn,
                    ClientIds = clientIds
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
                    var clientIds = new List<byte>();
                    clientIds.Add(client.ClientId);
                    _server.SendMessage(new NetworkMessageData {
                        MessageType = LockstepMsgType.EmptyTurns,
                        ClientIds = clientIds
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
                OnLockstepCommandReceived(data.ClientIds[0], reader);
                break;
            case LockstepMsgType.PlayerReady:
                OnPlayerReadyReceived(data.ClientIds[0], reader);
                break;
            case LockstepMsgType.PlayerFinish:
                OnPlayerFinishReceived(data.ClientIds[0], reader);
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
            return client != null && client.PlayerToken != null && PlayerResults.ContainsKey(client.PlayerNumber);
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
                if(_localClientData.Ready)
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
                    ids[client.PlayerNumber] = client.PlayerToken;
                }
                if(_localClient != null && _localClientData.Ready)
                {
                    ids[_localClientData.PlayerNumber] = _localClientData.PlayerToken;
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
                if(client.PlayerToken == playerId)
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
            return client.PlayerToken;
        }

        public byte FindClientId(string playerId)
        {
            var client = FindClientByPlayerId(playerId);
            if(client == null)
            {
                AttrDic dic = new AttrDic();
                dic.SetValue(LogMessagePlayerIDKey, playerId);

                SendCustomLog(LogMessageCouldNotFindPlayerID, dic);
                throw new Exception(string.Format(ErrorMessageClientNotFound, playerId));
            }
            return client.ClientId;
        }

        void OnPlayerReadyReceived(byte clientId, IReader reader)
        {
            var msg = new PlayerReadyMessage();
            msg.Deserialize(reader);

            var client = FindClientByPlayerId(msg.PlayerId);
            if(client == null)
            {
                // new client
                client = CreateClientData(msg.PlayerId);
                _clients.Add(client);
            }
            else
            {
                client.Connections++;
                if(client.Connections > 1)
                {
                    SendClientStatusMessage(client, true);
                }
            }
            client.ClientId = clientId;
            client.Version = msg.Version;

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
            var clientIds = new List<byte>();
            clientIds.Add(clientId);
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientStart,
                ClientIds = clientIds
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
            if(!_serverLockstep.Running
               && (ReadyPlayerCount == ServerConfig.MaxPlayers
               || (ReadyPlayerCount > 0 && ServerConfig.AllowMatchStartWithOnePlayerReady)))
            {
                StartLockstep();
            }
        }

        void StartLockstep()
        {
            SendCustomLog(LogMessageStart);

            if(_matchEnded)
            {
                SendCustomLog(LogMessageAlreadyEnded, LogLevel.Error);
            }
            if(BeforeMatchStarts != null)
            {
                BeforeMatchStarts();
            }
            if(ServerConfig.MatchmakingEnabled && _matchmaking != null && _matchmaking.Enabled)
            {
                var playerIds = PlayerIds;
                var matchId = MatchId;
                if(playerIds.Count > 0 && !string.IsNullOrEmpty(matchId))
                {
                    var dic = new AttrDic();
                    for(int i = 0; i < playerIds.Count; i++)
                    {
                        var client = FindClientByPlayerId(playerIds[i]);
                        if(client != null && !string.IsNullOrEmpty(client.Version))
                        {
                            dic.SetValue(playerIds[i], client.Version);
                        }
                    }
                    _matchmaking.ClientsVersions = dic;

                    _matchmaking.LoadInfo(matchId, playerIds);
                    return;
                }
            }
            DoStartLockstep();
        }

        void IMatchmakingServerDelegate.OnMatchInfoReceived(byte[] info)
        {
            var matchData = new JsonAttrParser().Parse(info).AsDic;

            for(int i = 0; i < MaxPlayers; i++)
            {
                var playerTokenKey = string.Format(PlayerTokenKey, i + 1);
                var playerIdKey = string.Format(PlayerIDKey, i + 1);
                
                if(matchData.ContainsKey(playerTokenKey))
                {
                    var playerID = matchData[playerTokenKey].ToString();

                    var client = FindClientByPlayerId(playerID);
                    if(client == null)
                    {
                        client = CreateClientData(playerID);
                        _clients.Add(client);
                    }
                    if(matchData.ContainsKey(playerIdKey))
                    {
                        client.PlayerId = matchData[playerIdKey].AsDic[LogMessageIDKey].AsValue.ToLong();
                    }
                }
                else
                {
                    AttrDic dic = new AttrDic();
                    dic.SetValue(ParamResponseBody, System.Text.Encoding.UTF8.GetString(_matchmaking.InfoResponse.Body));
                    SendCustomLog(LogMessageNoPlayerTokenOnMatchInfo, dic);
                }
            }

            if(MatchStarted != null)
            {
                MatchStarted(info);
            }
            DoStartLockstep();
        }

        ClientData CreateClientData(string id)
        {
            var client = new ClientData {
                PlayerToken = id,
                PlayerNumber = FreePlayerNumber
            };
            return client;
        }

        void IMatchmakingServerDelegate.OnError(Error ierr)
        {
            var err = new Error(MatchmakingErrorCode,
                          string.Format(LogMessageMatchmakingFormat, ierr.Msg),
                          ierr.Detail);
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
                if(results.ContainsKey(client.PlayerToken))
                {
                    var clientIds = new List<byte>();
                    clientIds.Add(client.ClientId);
                    var result = results[client.PlayerToken];
                    _server.SendMessage(new NetworkMessageData {
                        MessageType = LockstepMsgType.ClientEnd,
                        ClientIds = clientIds
                    }, new AttrMessage(result));
                }
            }
        }

        void DoStartLockstep()
        {
            SendCustomLog(LogMessageStartingLockstep);
            _serverLockstep.Start(ServerConfig.ClientSimulationDelay - ServerConfig.ClientStartDelay);
            if(SendMetric != null)
            {
                SendMetric(new Metric(MetricType.Counter, MatchStartMetricName, 1));
            }
            if(SendTrack != null)
            {
                var data = new AttrDic();
                data.Set(LogMessageUniqueIDKey, new AttrString(MatchId));
                data.Set(ParamServerConfig, ServerConfig.ToAttr());
                data.Set(ParamConfig, Config.ToAttr());
                SendTrack(MatchStartMetricName, data, null);
            }
            for(var i = 0; i < _clients.Count; i++)
            {
                var client = _clients[i];
                var clientIds = new List<byte>();
                clientIds.Add(client.ClientId);
                _server.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.ClientStart,
                    ClientIds = clientIds
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
            if(_serverLockstep.Running && FinishedPlayerCount > 0)
            {
                if(FinishedPlayerCount >= ReadyPlayerCount)
                {
                    EndLockstep();
                }
                else if(HasMatchEnded != null)
                {
                    if(HasMatchEnded())
                    {
                        EndLockstep();
                    }
                    else
                    {
                        // Should be tagged as cheater?
                        AttrDic dic = new AttrDic();

                        dic.SetValue(TurnNumberParam, CurrentTurnNumber);

                        for(int i = 0; i < _clients.Count; i++)
                        {
                            dic.SetValue(string.Format(PlayerFinishedIDKey, _clients[i].PlayerNumber), HasClientFinished(_clients[i]));
                        }
                        dic.SetValue(ParamDetail, new System.Diagnostics.StackTrace(true).ToString());

                        SendCustomLog(LogMessagePlayerTryEnd, dic);
                        _matchEndTimeOut = DateTime.Now;
                    }
                }
            }
        }

        void CheckMatchEndTimeout()
        {
            if(_serverLockstep.Running && _matchEndTimeOut > DateTime.MinValue && (DateTime.Now - _matchEndTimeOut).TotalSeconds >= ServerConfig.MatchEndedWithoutConfirmationTimeout)
            {
                SendCustomLog(LogMessageTimedOut, LogLevel.Error);

                EndLockstep();
            }
        }

        void EndLockstep()
        {
            _matchEnded = true;
            _serverLockstep.Stop();
            _matchEndTimeOut = DateTime.MinValue;

            var results = PlayerResults;
            var customData = new AttrDic();
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
                MatchFinished(results, customData);
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
                SendMetric(new Metric(MetricType.Counter, corrected ? MatchEndCorrectedMetricName : MatchEndMetricName, 1));
            }

            if(SendTrack != null)
            {
                var data = new AttrDic();
                data.Set(LogMessageUniqueIDKey, new AttrString(MatchId));
                var origResultsAttr = new AttrDic();
                {
                    var itr = results.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        var playerId = FindPlayerId(itr.Current.Key);
                        if(!string.IsNullOrEmpty(playerId))
                        {
                            origResultsAttr[playerId] = itr.Current.Value;
                        }
                    }
                    itr.Dispose();
                }
                data.Set(ParamMatchResult, origResultsAttr);
                if(corrected)
                {
                    data.Set(ParamMatchResultCorrected, resultsAttr);
                }
                SendTrack(corrected ? MatchEndCorrectedMetricName : MatchEndMetricName, data, null);
            }

            SendCustomLog(LogMessageEnded);

            if(_matchmaking == null || !_matchmaking.Enabled)
            {
                SendResults(resultsAttr);
            }
            else
            {
                if(_clients.Count < 1)
                {
                    SendCustomLog(LogMessageNoClients, LogLevel.Error);
                }
                else
                {
                    if(resultsAttr.Count < 1 || customData.Count < 1)
                    {
                        SendCustomLog(LogMessageDataIsEmpty, LogLevel.Error);
                    }
                    else
                    {
                        _matchmaking.NotifyResults(MatchId, resultsAttr, customData);
                    }
                }
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
                          string.Format(LogMessageNetworkFormat, ierr));
            OnError(err);
        }

        void OnError(Error err)
        {
            if(ErrorProduced != null)
            {
                ErrorProduced(err);
                SendServerOnErrorLog();
            }
        }

        AttrDic GetRequestAndResponseLogs(HttpRequest request, HttpResponse response)
        {
            var dic = new AttrDic();

            if(request.Body != null)
            {
                dic.SetValue(ParamBody, System.Text.Encoding.UTF8.GetString(request.Body));
            }

            dic.SetValue(LogMessageMatchIDKey, MatchId);

            if(request.BodyParams != null)
            {
                GetLogParams(dic, request.BodyParams, ParamsTypeBody);
            }
            if(request.Params != null)
            {
                GetLogParams(dic, request.Params, ParamsTypeParams);
            }
            if(request.QueryParams != null)
            {
                GetLogParams(dic, request.QueryParams, ParamsTypeQuery);
            }

            if(response.Body != null)
            {
                dic.SetValue(ParamResponseBody, System.Text.Encoding.UTF8.GetString(response.Body));
            }

            return dic;
        }

        void GetLogParams(AttrDic dic, AttrDic paramsDic, string paramType)
        {
            dic.Set(string.Format(ParamLogMatchID, paramType, LogMessageMatchIDKey), paramsDic[LogMessageMatchIDKey]);

            int i = 0;
            int playerFlag = 0;
            foreach(var valParam in paramsDic[ParamPlayers].AsDic)
            {
                playerFlag = 0;
                foreach(var param in valParam.Value.AsDic)
                {
                    if(param.Key == ParamDuration)
                        dic.Set(string.Format(ParamLogPlayersFormat, paramType, i, ParamDuration), param.Value);
                    else if(param.Key == ParamModified)
                        dic.Set(string.Format(ParamLogPlayersFormat, paramType, i, ParamModified), param.Value);
                    else
                    {
                        dic.Set(string.Format(ParamLogPlayersFlagsFormat, paramType, i, ParamFlags, playerFlag), param.Value);
                        playerFlag++;
                    }
                }
                i++;
            }
            dic.Set(string.Format(ParamLogModified, paramType, ParamModified), paramsDic[ParamModified]);
        }

        public void OnClientConnected(byte clientId)
        {
            var dic = new AttrDic();
            dic.SetValue(LogMessageClientIDKey, clientId);

            SendCustomLog(LogMessageClientConnected, dic);

            if(OnClientConnectedEvent != null)
            {
                OnClientConnectedEvent(clientId);
            }

            ClientCount++;
            var clientIds = new List<byte>();
            clientIds.Add(clientId);
            _server.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ClientSetup,
                ClientIds = clientIds,
                Unreliable = false
            }, new ClientSetupMessage(Config, GameParams));
        }

        public void OnClientDisconnected(byte clientId)
        {
            var dic = new AttrDic();
            dic.SetValue(LogMessageClientIDKey, clientId);
            var client = FindClientByClientId(clientId);
            if(client != null)
            {
                dic.SetValue(LogMessagePlayerIDKey, client.PlayerId);
            }

            SendCustomLog(LogMessageClientDisconnected, dic);

            if(OnClientDisconnectedEvent != null)
            {
                OnClientDisconnectedEvent(clientId);
            }

            ClientCount--;
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
            SendCustomLog(LogMessageStopped);
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

        public IEnumerator<ServerTurnData> GetLockstepTurnsEnumerator()
        {
            return _serverLockstep.GetTurnsEnumerator();
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
                if(other.Ready && !string.Equals(other.PlayerToken, client.PlayerToken, StringComparison.CurrentCultureIgnoreCase))
                {
                    var clientIds = new List<byte>();
                    clientIds.Add(other.ClientId);
                    _server.SendMessage(new NetworkMessageData {
                        MessageType = LockstepMsgType.ClientConnectionStatus,
                        ClientIds = clientIds
                    }, new ClientChangedConnectionStatusMessage(client.ClientId, connected));
                }
            }
        }

        void SendCustomLog(string message, LogLevel logLevel, AttrDic dic = null)
        {
            if(SendLog != null)
            {
                if(dic == null)
                {
                    dic = new AttrDic();
                }
                dic.SetValue(LogMessageMatchIDKey, MatchId);
                dic.SetValue(ParamLogNumber, _logCount);
                for(int i = 0; i < _clients.Count; i++)
                {
                    dic.SetValue(string.Format(ParamPlayer, i), _clients[i].PlayerId.ToString());
                }
                SendLog(new Network.ServerEvents.Log(logLevel, string.Format(LogMessageLockstepFormat, message), dic), true);
                _logCount++;
            }
            else
            {
                /* NOTE: The null check was added because it was messing with unit tests, 
                 * but in reality this class should not have logging functionality.
                 * A refactor/cleaning should be done.
                 * */
                SocialPoint.Base.Log.w("Server SendLog action not set...");
            }
        }

        void SendCustomLog(string message, AttrDic dic = null)
        {
            SendCustomLog(message, LogLevel.Debug, dic);
        }

        //TODO: Fix this? Should this data be custom for all games or should we add a way for each game to customize it
        void SendServerOnErrorLog()
        {
            var infoResponse = _matchmaking.InfoResponse;
            var infoRequest = _matchmaking.InfoRequest;
            var notifyResponse = _matchmaking.NotifyResponse;
            var notifyRequest = _matchmaking.NotifyRequest;

            AttrDic dic = null;
            if(notifyResponse != null && notifyResponse.HasError && notifyRequest != null)
            {
                dic = GetRequestAndResponseLogs(notifyRequest, notifyResponse);
            }
            else if(infoResponse != null && infoResponse.HasError && infoRequest != null)
            {
                dic = GetRequestAndResponseLogs(infoRequest, infoResponse);
            }

            if(dic != null)
            {
                SendCustomLog(LogMessageMatchEndNotification, LogLevel.Error, dic);
            }
        }

        void AddServerNotificationLogData(AttrDic dic, AttrDic data, string baseKey)
        {
            dic.Set(baseKey + ".match_id", data["match_id"]);

            int i = 0;
            var outerItr = data["players"].AsDic.GetEnumerator();
            while(outerItr.MoveNext())
            {
                int playerFlag = 0;
                var valParam = outerItr.Current;
                var innerItr = valParam.Value.AsDic.GetEnumerator();
                while(innerItr.MoveNext())
                {
                    var param = innerItr.Current;
                    if(param.Key == "duration")
                    {
                        dic.Set(baseKey + ".players" + i + ".duration", param.Value);
                    }
                    else if(param.Key == "modified")
                    {
                        dic.Set(baseKey + ".players" + i + ".modified", param.Value);
                    }
                    else
                    {
                        dic.Set(baseKey + ".players" + i + ".flags" + playerFlag, param.Value);
                        playerFlag++;
                    }
                }
                innerItr.Dispose();
                i++;
            }
            outerItr.Dispose();

            dic.Set(baseKey + ".modified", data["modified"]);
        }

        #region local client

        public string LocalPlayerId
        {
            get
            {
                return _localClientData.PlayerToken;
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
            if(_localClient != null)
            {
                Replay(_localClient, _localFactory);
            }
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
                          string.Format(CommandFormat, ierr.Msg),
                          ierr.Detail);
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
                _localClientData.PlayerToken = playerId;
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
