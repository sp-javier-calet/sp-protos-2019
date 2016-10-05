using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;

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

        public event Action<ServerLockstepTurnData> TurnReady;

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
            _updateScheduler = updateScheduler;
            _turns = new Dictionary<int, ServerLockstepTurnData>();
            Stop();
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

        public void Start(int dt=0)
        {
            Running = true;
            _time = -dt;
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
                if(!_turns.TryGetValue(t, out turn))
                {
                    turn = ServerLockstepTurnData.Empty;
                }
                if(TurnReady != null)
                {
                    TurnReady(turn);
                }
                ConfirmLocalClientTurn(turn);
                _lastCmdTime = nextCmdTime;
            }
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

        #endregion
    }
}