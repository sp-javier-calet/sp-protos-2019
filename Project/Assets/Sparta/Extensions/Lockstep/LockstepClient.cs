using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommandLogic<T>
    {
        void Apply(T data, byte playerNum);
    }

    public class ActionLockstepCommandLogic<T> : ILockstepCommandLogic<T>
    {
        readonly Action _action1;
        readonly Action<T> _action2;
        readonly Action<T, byte> _action3;

        public ActionLockstepCommandLogic(Action action)
        {
            _action1 = action;
        }

        public ActionLockstepCommandLogic(Action<T> action)
        {
            _action2 = action;
        }

        public ActionLockstepCommandLogic(Action<T, byte> action)
        {
            _action3 = action;
        }

        public void Apply(T data, byte playerNum)
        {
            if(_action1 != null)
            {
                _action1();
            }
            if(_action2 != null)
            {
                _action2(data);
            }
            if(_action3 != null)
            {
                _action3(data, playerNum);
            }
        }
    }

    public interface ILockstepCommandLogic : ILockstepCommandLogic<ILockstepCommand>
    {
    }

    public class LockstepCommandLogic<T> : ILockstepCommandLogic
    {
        ILockstepCommandLogic<T> _inner;

        public LockstepCommandLogic(Action<T, byte> action) :
            this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(Action<T> action) :
        this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(Action action) :
        this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(ILockstepCommandLogic<T> inner)
        {
            _inner = inner;
        }

        public void Apply(ILockstepCommand data, byte playerNum)
        {
            if(data is T && _inner != null)
            {
                _inner.Apply((T)data, playerNum);
            }
        }
    }

    [Serializable]
    public sealed class LockstepClientConfig
    {
        public const int DefaultLocalSimulationDelay = 1000;
        public const int DefaultMaxSimulationStepsPerFrame = 0;
        public const float DefaultSpeedFactor = 1.0f;

        public int LocalSimulationDelay = DefaultLocalSimulationDelay;
        public int MaxSimulationStepsPerFrame = DefaultMaxSimulationStepsPerFrame;
        public float SpeedFactor = DefaultSpeedFactor;

        public override string ToString()
        {
            return string.Format("[LockstepClientConfig\n" +
            "LocalSimulationDelay:{0}\n" +
            "MaxSimulationStepsPerFrame:{1}\n" +
            "SpeedFactor:{2}]",
                LocalSimulationDelay,
                MaxSimulationStepsPerFrame,
                SpeedFactor);
        }
    }

    public class LockstepClient : IUpdateable, IDisposable
    {
        enum State
        {
            /**
             * turn buffer is withing the normal limits
             */
            Normal,

            /*
             * when the client is stopped because the current turn was not received
             */
            Waiting,

            /**
             * when the client is speeding to get to the current turn
             * (after lag or at the start of a reconnection)
             */
            Recovering
        }

        IUpdateScheduler _updateScheduler;

        long _timestamp;
        int _time;
        int _lastSimTime;
        int _lastCmdTime;
        int _lastConfirmedTurnNumber;
        bool _simStartedCalled;
        bool _simRecoveredCalled;
        State _state;
        XRandom _rootRandom;

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();
        List<ClientCommandData> _pendingCommands = new List<ClientCommandData>();
        Dictionary<int, ClientTurnData> _confirmedTurns = new Dictionary<int, ClientTurnData>();

        public bool Running{ get; private set; }

        public LockstepConfig Config { get; set; }

        public LockstepGameParams GameParams { get; set; }

        public LockstepClientConfig ClientConfig { get; set; }

        public event Action<ClientCommandData> CommandAdded;
        public event Action<ClientTurnData> TurnApplied;
        public event Action SimulationStarted;
        public event Action SimulationRecovered;
        public event Action ConnectionChanged;
        public event Action<int> Simulate;
        public event Action<Error, ClientCommandData> CommandFailed;

        public bool Connected
        {
            get
            {
                return _state != State.Waiting;
            }
        }

        public bool Recovering
        {
            get
            {
                return _state == State.Recovering;
            }
        }

        public int TurnBuffer
        {
            get
            {
                var b = _lastConfirmedTurnNumber - CurrentTurnNumber;
                b = Math.Max(b, 0);
                return b;
            }
        }

        public int UpdateTime
        {
            get
            {
                return _time;
            }
        }

        public int SimulationDeltaTime
        {
            get
            {
                return _time - _lastSimTime;
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

        public byte PlayerNumber;

        bool _externalUpdate;
        public bool ExternalUpdate
        {
            set
            {
                _externalUpdate = value;
                if(_updateScheduler != null)
                {
                    if(_externalUpdate)
                    {
                        _updateScheduler.Remove(this);
                    }
                    else if(Running)
                    {
                        _updateScheduler.Add(this);
                    }
                }
            }

            get
            {
                return _externalUpdate;
            }
        }

        public LockstepClient(IUpdateScheduler updateScheduler = null)
        {
            _state = State.Normal;
            Config = new LockstepConfig();
            GameParams = new LockstepGameParams();
            ClientConfig = new LockstepClientConfig();
            _updateScheduler = updateScheduler;
            Stop();
        }

        [Obsolete("Use the Config setter")]
        public void Init(LockstepConfig config)
        {
            Config = config;
        }

        public void Start(int startTime = 0)
        {
            Running = true;
            _time = startTime;
            _state = _time > 0 ? State.Recovering : State.Normal;
            _timestamp = TimeUtils.TimestampMilliseconds;
            _lastSimTime = 0;
            _lastCmdTime = 0;
            _simStartedCalled = false;
            _simRecoveredCalled = false;
            if(!_externalUpdate && _updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            Running = false;
            _rootRandom = null;
            _state = State.Waiting;
            _confirmedTurns.Clear();
            _pendingCommands.Clear();
            _lastConfirmedTurnNumber = 0;
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

        public void RegisterCommandLogic<T>(Action apply) where T:  ILockstepCommand
        {
            RegisterCommandLogic<T>(new ActionLockstepCommandLogic<T>(apply));
        }

        public void RegisterCommandLogic<T>(Action<T> apply) where T:  ILockstepCommand
        {
            RegisterCommandLogic<T>(new ActionLockstepCommandLogic<T>(apply));
        }

        public void RegisterCommandLogic<T>(Action<T, byte> apply) where T:  ILockstepCommand
        {
            RegisterCommandLogic<T>(new ActionLockstepCommandLogic<T>(apply));
        }

        public void RegisterCommandLogic<T>(ILockstepCommandLogic<T> logic) where T:  ILockstepCommand
        {
            RegisterCommandLogic(typeof(T), new LockstepCommandLogic<T>(logic));
        }

        public void RegisterCommandLogic(Type type, ILockstepCommandLogic logic)
        {
            _commandLogics[type] = logic;
        }

        public ClientCommandData AddPendingCommand<T>(T command, Action<T, byte> finish) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        public ClientCommandData AddPendingCommand<T>(T command, Action<T> finish) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        public ClientCommandData AddPendingCommand<T>(T command, Action finish) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        public ClientCommandData AddPendingCommand<T>(T command, ILockstepCommandLogic<T> finish = null) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        ClientCommandData AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var data = new ClientCommandData(command, logic, PlayerNumber);
            if(!Running || _time < 0)
            {
                data.Finish();
                return null;
            }
            AddPendingCommand(data);
            return data;
        }

        void AddPendingCommand(ClientCommandData command)
        {
            _pendingCommands.Add(command);
            if(CommandAdded != null)
            {
                CommandAdded(command);
            }
            else
            {
                AddConfirmedCommand(command);
            }
        }

        public void AddConfirmedTurn(ClientTurnData turn=null)
        {
            _lastConfirmedTurnNumber++;
            if(!ClientTurnData.IsNullOrEmpty(turn))
            {
                _confirmedTurns[_lastConfirmedTurnNumber] = turn;
            }
        }

        public void AddConfirmedEmptyTurns(EmptyTurnsMessage emptyTurns)
        {
            for(int i = 0; i < emptyTurns.EmptyTurns; ++i)
            {
                AddConfirmedTurn(null);
            }
        }

        void AddConfirmedCommand(ClientCommandData cmd)
        {
            var t = 1 + ((_lastCmdTime + ClientConfig.LocalSimulationDelay) / Config.CommandStepDuration);
            ClientTurnData turn;
            if(!_confirmedTurns.TryGetValue(t, out turn))
            {
                turn = new ClientTurnData();
                _confirmedTurns[t] = turn;
                _lastConfirmedTurnNumber = Math.Max(_lastConfirmedTurnNumber, t);
            }
            turn.AddCommand(cmd);
        }

        ClientCommandData FindCommand(ClientCommandData cmd)
        {
            var idx = _pendingCommands.IndexOf(cmd);
            if(idx >= 0)
            {
                cmd = _pendingCommands[idx];
                _pendingCommands.RemoveAt(idx);
            }
            return cmd;
        }

        void ProcessTurn(ClientTurnData turn)
        {
            var itr = turn.GetCommandEnumerator();
            while(itr.MoveNext())
            {
                var command = FindCommand(itr.Current);
                if(command == null)
                {
                    continue;
                }
                try
                {
                    var itr2 = _commandLogics.GetEnumerator();
                    while(itr2.MoveNext())
                    {
                        command.Apply(itr2.Current.Key, itr2.Current.Value);
                    }
                    itr2.Dispose();
                }
                catch(Exception e)
                {
                    if(CommandFailed != null)
                    {
                        CommandFailed(new Error(e.ToString()), command);
                    }
                }
                command.Finish();
            }
            itr.Dispose();
            if(TurnApplied != null)
            {
                TurnApplied(turn);
            }
        }

        public void Pause()
        {
            Running = false;
        }

        public void Resume()
        {
            Running = true;
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

            dt = (int)(ClientConfig.SpeedFactor * (float)dt);
            _time += dt;
            if(!_simStartedCalled && _time >= 0)
            {
                _simStartedCalled = true;
                if(SimulationStarted != null)
                {
                    SimulationStarted();
                }
            }
            if(!_simRecoveredCalled && _time >= 0 && _state == State.Normal)
            {
                _simRecoveredCalled = true;
                if(SimulationRecovered != null)
                {
                    SimulationRecovered();
                }
            }
            var simSteps = 0;
            var wasConnected = Connected;
            _state = State.Normal;
            while(true)
            {
                var nextSimTime = _lastSimTime + Config.SimulationStepDuration;
                var nextCmdTime = _lastCmdTime + Config.CommandStepDuration;

                if(nextSimTime <= nextCmdTime && nextSimTime <= _time)
                {
                    if(Simulate != null)
                    {
                        Simulate(Config.SimulationStepDuration);
                    }
                    _lastSimTime = nextSimTime;
                    simSteps++;
                    if(ClientConfig.MaxSimulationStepsPerFrame > 0 && simSteps > ClientConfig.MaxSimulationStepsPerFrame)
                    {
                        _state = State.Recovering;
                        break;
                    }
                }
                else if(nextCmdTime <= _time)
                {
                    var t = CurrentTurnNumber + 1;
                    if(_lastConfirmedTurnNumber >= t)
                    {
                        _state = State.Normal;
                        ClientTurnData turn;
                        if(!_confirmedTurns.TryGetValue(t, out turn))
                        {
                            turn = ClientTurnData.Empty;
                        }
                        else
                        {
                            _confirmedTurns.Remove(t);
                        }
                        ProcessTurn(turn);
                    }
                    else if(CommandAdded == null)
                    {
                        ProcessTurn(ClientTurnData.Empty);
                    }
                    else
                    {
                        int missingTurnsCount = t - _lastConfirmedTurnNumber;
                        bool shouldDisconnect = missingTurnsCount > Config.MaxSkippedEmptyTurns;
                        if(shouldDisconnect)
                        {
                            _state = State.Waiting;
                        }
                        break;
                    }
                    if(_state == State.Normal)
                    {
                        _lastCmdTime = nextCmdTime;
                    }
                }
                else
                {
                    break;
                }
            }
            if(wasConnected != Connected)
            {
                if(ConnectionChanged != null)
                {
                    ConnectionChanged();
                }
            }
        }

        public XRandom CreateRandomGenerator()
        {
            if(_rootRandom == null)
            {
                _rootRandom = new XRandom(GameParams.RandomSeed);
            }
            return new XRandom(_rootRandom.Next());
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
