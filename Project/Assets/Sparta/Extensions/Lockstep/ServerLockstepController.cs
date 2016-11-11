using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.IO;
using FixMath.NET;

namespace SocialPoint.Lockstep
{
    public sealed class ServerLockstepController : IUpdateable, IDisposable
    {
        int _time;
        long _timestamp;
        int _lastCmdTime;
        IUpdateScheduler _updateScheduler;
        Dictionary<int, ServerLockstepTurnData> _turns;

        public bool Running{ get; private set; }

        public LockstepConfig Config { get; set; }

        public LockstepGameParams GameParams { get; private set; }

        public event Action<ServerLockstepTurnData> TurnReady;
        public event Action<int> EmptyTurnsReady;
        int _pendingEmptyTurns;

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

        public ServerLockstepController(IUpdateScheduler updateScheduler = null)
        {
            Config = new LockstepConfig();
            GameParams =  new LockstepGameParams();
            _updateScheduler = updateScheduler;
            _turns = new Dictionary<int, ServerLockstepTurnData>();
            _pendingEmptyTurns = 0;
            Stop();
        }

        public IEnumerator<ServerLockstepTurnData> GetTurnsEnumerator()
        {
            var t = 0;
            var itr = _turns.GetEnumerator();
            var n = CurrentTurnNumber;
            while(itr.MoveNext())
            {
                var k = itr.Current.Key;
                if(k >= n)
                {
                    break;
                }
                for(; t < k; t++)
                {
                    yield return ServerLockstepTurnData.Empty;
                }
                yield return itr.Current.Value;
                t++;
            }
            itr.Dispose();
            for(; t < n; t++)
            {
                yield return ServerLockstepTurnData.Empty;
            }
        }

        public void AddCommand(ServerLockstepCommandData command)
        {
            if(!Running || _time < 0)
            {
                return;
            }
            var t = CurrentTurnNumber;
            ServerLockstepTurnData turn;
            if(!_turns.TryGetValue(t, out turn))
            {
                turn = new ServerLockstepTurnData();
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
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            Running = false;
            _turns.Clear();
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

        public void Update()
        {
            var timestamp = TimeUtils.TimestampMilliseconds;
            Update((int)(timestamp - _timestamp));
            _timestamp = timestamp;
        }

        public void Update(int dt)
        {   
            if(!Running || dt < 0)
            {
                return;
            }
            _time += dt;
            while(true)
            {
                var nextCmdTime = _lastCmdTime + Config.CommandStepDuration;
                if(nextCmdTime > _time)
                {                
                    break;
                }
                ServerLockstepTurnData turn;
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
                    _pendingEmptyTurns++;
                    SendEmptyTurnsToClient();
                }

                _lastCmdTime = nextCmdTime;
            }
        }

        void SendEmptyTurnsToClient()
        {
            int emptyTurnsDuration = _pendingEmptyTurns * Config.CommandStepDuration;
            if(_pendingEmptyTurns == 0 || (Config.MaxTurnSkipDuration > 0 && emptyTurnsDuration < Config.MaxTurnSkipDuration))
            {
                return;
            }
            
            if(EmptyTurnsReady != null)
            {
                EmptyTurnsReady(_pendingEmptyTurns);
            }

            ConfirmLocalClientEmptyTurns(_pendingEmptyTurns);

            _pendingEmptyTurns = 0;
        }

        public void Dispose()
        {
            Stop();
            TurnReady = null;
            UnregisterLocalClient();
        }

        #region local client implementation

        ClientLockstepController _localClient;
        LockstepCommandFactory _localFactory;

        public void UnregisterLocalClient()
        {
            if(_localClient != null)
            {
                _localClient.CommandAdded -= AddPendingLocalClientCommand;
            }
            _localClient = null;
            _localFactory = null;
        }

        public void RegisterLocalClient(ClientLockstepController client, LockstepCommandFactory factory)
        {
            UnregisterLocalClient();
            _localClient = client;
            _localFactory = factory;
            _localClient.Config = Config;
            _localClient.GameParams = GameParams;
            if(_localClient != null)
            {
                _localClient.CommandAdded += AddPendingLocalClientCommand;
            }
        }

        void AddPendingLocalClientCommand(ClientLockstepCommandData command)
        {
            var serverCommand = command.ToServer(_localFactory);
            AddCommand(serverCommand);
        }

        void ConfirmLocalClientTurn(ServerLockstepTurnData turn)
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

        void AddLocalMessage()
        {
            
        }

        #endregion
    }
}