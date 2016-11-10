using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommandLogic<T>
    {
        void Apply(T data);
    }

    public class ActionLockstepCommandLogic<T> : ILockstepCommandLogic<T>
    {
        readonly Action<T> _action;

        public ActionLockstepCommandLogic(Action<T> action)
        {
            _action = action;
        }

        public void Apply(T data)
        {
            if(_action != null)
            {
                _action(data);
            }
        }
    }

    public interface ILockstepCommandLogic : ILockstepCommandLogic<ILockstepCommand>
    {
    }

    public class LockstepCommandLogic<T> : ILockstepCommandLogic
    {
        ILockstepCommandLogic<T> _inner;

        public LockstepCommandLogic(Action<T> action) :
            this(new ActionLockstepCommandLogic<T>(action))
        {
        }

        public LockstepCommandLogic(ILockstepCommandLogic<T> inner)
        {
            _inner = inner;
        }

        public void Apply(ILockstepCommand data)
        {
            if(data is T && _inner != null)
            {
                _inner.Apply((T)data);
            }
        }
    }

    [Serializable]
    public sealed class ClientLockstepConfig
    {
        public const int DefaultLocalSimulationDelay = 1000;
        public const int DefaultMaxSimulationStepsPerFrame = 0;
        public const float DefaultSpeedFactor = 1.0f;

        public int LocalSimulationDelay = DefaultLocalSimulationDelay;
        public int MaxSimulationStepsPerFrame = DefaultMaxSimulationStepsPerFrame;
        public float SpeedFactor = DefaultSpeedFactor;

        public override string ToString()
        {
            return string.Format("[ClientLockstepConfig\n" +
            "LocalSimulationDelay:{0}\n" +
            "MaxSimulationStepsPerFrame:{1}\n" +
            "SpeedFactor:{2}]",
                LocalSimulationDelay,
                MaxSimulationStepsPerFrame,
                SpeedFactor);
        }
    }

    public class ClientLockstepController : IUpdateable, IDisposable
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
        List<ClientLockstepCommandData> _pendingCommands = new List<ClientLockstepCommandData>();
        Dictionary<int, ClientLockstepTurnData> _confirmedTurns = new Dictionary<int, ClientLockstepTurnData>();

        public bool Running{ get; private set; }

        public LockstepConfig Config { get; set; }

        public LockstepGameParams GameParams { get; set; }

        public ClientLockstepConfig ClientConfig { get; set; }

        public event Action<ClientLockstepCommandData> CommandAdded;
        public event Action<ClientLockstepTurnData> TurnApplied;
        public event Action SimulationStarted;
        public event Action SimulationRecovered;
        public event Action ConnectionChanged;
        public event Action<int> Simulate;

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

        public ClientLockstepController(IUpdateScheduler updateScheduler = null)
        {
            _state = State.Normal;
            Config = new LockstepConfig();
            GameParams = new LockstepGameParams();
            ClientConfig = new ClientLockstepConfig();
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
            if(_updateScheduler != null)
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

        public void RegisterCommandLogic<T>(Action<T> apply) where T:  ILockstepCommand
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

        public ClientLockstepCommandData AddPendingCommand<T>(T command, Action<T> finish) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        public ClientLockstepCommandData AddPendingCommand<T>(T command, ILockstepCommandLogic<T> finish = null) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        ClientLockstepCommandData AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var data = new ClientLockstepCommandData(command, logic);
            if(!Running || _time < 0)
            {
                data.Finish();
                return null;
            }
            AddPendingCommand(data);
            return data;
        }

        void AddPendingCommand(ClientLockstepCommandData command)
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

        public void AddConfirmedEmptyTurns()
        {
            int emptyTurns = 4;
            _lastConfirmedTurnNumber += emptyTurns;
            _lastCmdTime += Config.CommandStepDuration * emptyTurns;
        }

        public void AddConfirmedTurn(ClientLockstepTurnData turn=null)
        {
            _lastConfirmedTurnNumber++;
            if(!ClientLockstepTurnData.IsNullOrEmpty(turn))
            {
                _confirmedTurns[_lastConfirmedTurnNumber] = turn;
            }
        }

        void AddConfirmedCommand(ClientLockstepCommandData cmd)
        {
            var t = 1 + ((_lastCmdTime + ClientConfig.LocalSimulationDelay) / Config.CommandStepDuration);
            ClientLockstepTurnData turn;
            if(!_confirmedTurns.TryGetValue(t, out turn))
            {
                turn = new ClientLockstepTurnData();
                _confirmedTurns[t] = turn;
                _lastConfirmedTurnNumber = Math.Max(_lastConfirmedTurnNumber, t);
            }
            turn.AddCommand(cmd);
        }

        ClientLockstepCommandData FindCommand(ClientLockstepCommandData cmd)
        {
            var idx = _pendingCommands.IndexOf(cmd);
            if(idx >= 0)
            {
                cmd = _pendingCommands[idx];
                _pendingCommands.RemoveAt(idx);
            }
            return cmd;
        }

        void ProcessTurn(ClientLockstepTurnData turn)
        {
            var itr = turn.GetCommandEnumerator();
            while(itr.MoveNext())
            {
                var command = FindCommand(itr.Current);
                if(command == null)
                {
                    continue;
                }
                var itr2 = _commandLogics.GetEnumerator();
                while(itr2.MoveNext())
                {
                    command.Apply(itr2.Current.Key, itr2.Current.Value);
                }
                itr2.Dispose();
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

                // Consume Simulation if all commands before simulation time were consumed
                if(nextSimTime <= nextCmdTime && nextSimTime <= _time)
                {
                    if(Simulate != null)
                    {
                        Simulate(Config.SimulationStepDuration);
                    }
                    _lastSimTime = nextSimTime;
                    simSteps++;
                    //if(ClientConfig.MaxSimulationStepsPerFrame > 0 && simSteps > ClientConfig.MaxSimulationStepsPerFrame)
                    {
                        _state = State.Recovering;
                        break;
                    }
                }
                // Consume Commands until our current time
                else if(nextCmdTime <= _time)
                {
                    var t = CurrentTurnNumber + 1;
                    if(_lastConfirmedTurnNumber >= t)
                    {
                        _state = State.Normal;
                        ClientLockstepTurnData turn;
                        if(!_confirmedTurns.TryGetValue(t, out turn))
                        {
                            turn = ClientLockstepTurnData.Empty;
                        }
                        else
                        {
                            _confirmedTurns.Remove(t);
                        }
                        ProcessTurn(turn);
                    }
                    else if(CommandAdded == null)
                    {
                        ProcessTurn(ClientLockstepTurnData.Empty);
                    }
                    else
                    {
                        // We are trying to run a turn ahead of the lastConfirmed turn so we break and do nothing until a new lastConfirmed command is received
//                        _state = State.Waiting;
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