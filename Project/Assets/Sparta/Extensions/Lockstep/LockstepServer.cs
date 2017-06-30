using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Network.ServerEvents;

namespace SocialPoint.Lockstep
{
    public sealed class LockstepServer : IUpdateable, IDisposable
    {
        const string TurnProcessingTimeMetricName = "multiplayer.lockstep.turn_processing_time";
        const string TurnProcessingTimeExceedMetricName = "multiplayer.lockstep.turn_processing_time_exceed";
        public int MetricSendInterval = LockstepServerConfig.DefaultMetricSendInterval;
        int _timeSendMetric;

        int _time;
        long _timestamp;
        int _lastCmdTime;
        IUpdateScheduler _updateScheduler;
        Dictionary<int, ServerTurnData> _turns;
        int _pendingEmptyTurns;
        List<int> _processingTimes;

        public bool Running{ get; private set; }

        public LockstepConfig Config { get; set; }

        public LockstepGameParams GameParams { get; private set; }

        public event Action<ServerTurnData> TurnReady;
        public event Action<int> EmptyTurnsReady;

        public Action<Metric> SendMetric;

        public int UpdateTime
        {
            get
            {
                return _time;
            }
        }

        public int CommandDeltaTime
        {
            get
            {
                return _time - _lastCmdTime;
            }
        }

        public int CurrentTurnNumber
        {
            get
            {
                return _lastCmdTime / Config.CommandStepDuration;
            }
        }

        public LockstepServer(IUpdateScheduler updateScheduler = null)
        {
            Config = new LockstepConfig();
            GameParams = new LockstepGameParams();
            _updateScheduler = updateScheduler;
            _turns = new Dictionary<int, ServerTurnData>();
            _pendingEmptyTurns = 0;
            _processingTimes = new List<int>();
            Stop();
        }

        public IEnumerator<ServerTurnData> GetTurnsEnumerator(int afterTurnNumber = 0)
        {
            var t = afterTurnNumber;
            var itr = _turns.GetEnumerator();
            var n = CurrentTurnNumber;
            while(itr.MoveNext())
            {
                var k = itr.Current.Key;
                if(k < t)
                {
                    continue;
                }
                if(k >= n)
                {
                    break;
                }
                for(; t < k; t++)
                {
                    yield return ServerTurnData.Empty;
                }
                yield return itr.Current.Value;
                t++;
            }
            itr.Dispose();
            for(; t < n; t++)
            {
                yield return ServerTurnData.Empty;
            }
        }

        public void AddCommand(ServerCommandData command)
        {
            if(!Running || _time < 0)
            {
                return;
            }
            var t = CurrentTurnNumber;
            ServerTurnData turn;
            if(!_turns.TryGetValue(t, out turn))
            {
                turn = new ServerTurnData();
                _turns[t] = turn;
            }
            turn.AddCommand(command);
        }

        public void Start(int startTime = 0)
        {
            Running = true;
            _time = startTime;
            _lastCmdTime = 0;
            _timestamp = TimeUtils.TimestampMilliseconds;
            _turns.Clear();

            if(_localClient != null)
            {
                _localClient.ExternalUpdate = true;
            }
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            if(!Running)
            {
                return;
            }
            Running = false;
            if(_localClient != null)
            {
                _localClient.ExternalUpdate = false;
                _localClient.Stop();
            }
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

        void IUpdateable.Update()
        {
            Update();
        }

        public int Update()
        {
            var timestamp = TimeUtils.TimestampMilliseconds;
            var dt = (int)(timestamp - _timestamp);
            Update(dt);
            _timestamp = timestamp;
            return dt;
        }

        public void Update(int dt)
        {   
            if(!Running || dt < 0)
            {
                return;
            }
            if(_localClient != null)
            {
                _localClient.Update(dt);
            }
            _time += dt;
            _timeSendMetric += dt;
            if(_timeSendMetric > MetricSendInterval)
            {
                _timeSendMetric -= MetricSendInterval;
                SendAverageProcessingTime();
            }
            while(true)
            {
                var nextCmdTime = _lastCmdTime + Config.CommandStepDuration;
                if(nextCmdTime > _time)
                {                
                    break;
                }
                ServerTurnData turn;
                var t = CurrentTurnNumber;

                if(_turns.TryGetValue(t, out turn))
                {
                    SendEmptyTurnsToClient();
                    
                    if(TurnReady != null)
                    {
                        TurnReady(turn);
                    }

                    ConfirmLocalClientTurn(turn);
                }
                else
                {
                    AddEmptyTurn(t);
                    SendEmptyTurnsToClient();
                }

                _lastCmdTime = nextCmdTime;
            }
        }

        void AddEmptyTurn(int turn)
        {
            _turns.Add(turn, ServerTurnData.Empty);
            _pendingEmptyTurns++;
        }

        void SendEmptyTurnsToClient()
        {
            if(_pendingEmptyTurns == 0 || (Config.MaxSkippedEmptyTurns > 0 && _pendingEmptyTurns < Config.MaxSkippedEmptyTurns))
            {
                return;
            }
            
            if(EmptyTurnsReady != null)
            {
                EmptyTurnsReady(_pendingEmptyTurns);
            }
            else if(TurnReady != null)
            {
                for(int i = 0; i < _pendingEmptyTurns; ++i)
                {
                    TurnReady(null);
                }
            }

            ConfirmLocalClientEmptyTurns(_pendingEmptyTurns);

            _pendingEmptyTurns = 0;
        }

        public void Dispose()
        {
            Stop();
            _turns.Clear();
            TurnReady = null;
            UnregisterLocalClient();
        }

        void SendAverageProcessingTime()
        {
            if(SendMetric == null)
            {
                return;
            }
            var sum = 0;
            for(int i = 0; i < _processingTimes.Count; i++)
            {
                sum += _processingTimes[i];
            }
            SendMetric(new Metric(MetricType.Gauge, TurnProcessingTimeMetricName, sum > 0 ? (int)sum/_processingTimes.Count : 0));
            _processingTimes.Clear();
        }

        #region local client implementation

        LockstepClient _localClient;
        LockstepCommandFactory _localFactory;

        public void UnregisterLocalClient()
        {
            if(_localClient != null)
            {
                _localClient.CommandAdded -= AddPendingLocalClientCommand;
                _localClient.TurnApplied -= OnLocalClientTurnApplied;
            }
            _localClient = null;
            _localFactory = null;
        }

        public void RegisterLocalClient(LockstepClient client, LockstepCommandFactory factory)
        {
            UnregisterLocalClient();
            _localClient = client;
            _localFactory = factory;
            _localClient.Config = Config;
            _localClient.GameParams = GameParams;
            _localClient.CommandAdded += AddPendingLocalClientCommand;
            _localClient.TurnApplied += OnLocalClientTurnApplied;
        }

        void AddPendingLocalClientCommand(ClientCommandData command)
        {
            var serverCommand = command.ToServer(_localFactory);
            AddCommand(serverCommand);
        }

        private void OnLocalClientTurnApplied(ClientTurnData data, int processDuration)
        {
            if(SendMetric == null)
            {
                return;
            }
            if(processDuration >= Config.CommandStepDuration)
            {
                SendMetric(new Metric(MetricType.Counter, TurnProcessingTimeExceedMetricName, 1));
            }
            _processingTimes.Add(processDuration);
        }

        void ConfirmLocalClientTurn(ServerTurnData turn)
        {
            if(_localClient == null)
            {
                return;
            }
            var clientTurn = turn.ToClient(_localFactory);
            _localClient.AddConfirmedTurn(clientTurn);
        }

        void ConfirmLocalClientEmptyTurns(int emptyTurns)
        {
            if(_localClient == null)
            {
                return;
            }
            _localClient.AddConfirmedEmptyTurns(new EmptyTurnsMessage(emptyTurns));
        }

        #endregion
    }
}