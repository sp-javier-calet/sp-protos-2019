using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public interface ILockstepCommandLogic<T>
    {
        void Apply(T data);
    }

    public class ActionLockstepCommandLogic<T> : ILockstepCommandLogic<T>
    {
        Action<T> _action;

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
        IUpdateScheduler _updateScheduler;

        long _timestamp;
        int _time;
        int _lastSimTime;
        int _lastCmdTime;
        int _lastConfirmedTurnNumber;

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();
        List<ClientLockstepCommandData> _pendingCommands = new List<ClientLockstepCommandData>();
        Dictionary<int, ClientLockstepTurnData> _confirmedTurns = new Dictionary<int, ClientLockstepTurnData>();

        public bool Connected{ get; private set; }

        public bool Running{ get; private set; }

        public LockstepConfig Config { get; set; }

        public ClientLockstepConfig ClientConfig { get; set; }

        public event Action<ClientLockstepCommandData> CommandAdded;
        public event Action<ClientLockstepTurnData> TurnApplied;
        public event Action Started;
        public event Action ConnectionChanged;
        public event Action<int> Simulate;

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
            Config = new LockstepConfig();
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
            Connected = true;
            _time = startTime;
            _timestamp = TimeUtils.TimestampMilliseconds;
            _lastSimTime = 0;
            _lastCmdTime = 0;
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            Running = false;
            Connected = false;
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

        public void AddConfirmedTurn(ClientLockstepTurnData turn)
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
            var time = _time + dt;
            if(_time <= 0 && time >= 0)
            {
                if(Started != null)
                {
                    Started();
                }
            }
            _time = time;
            var simSteps = 0;
            var wasConnected = Connected;
            while(true)
            {
                var nextSimTime = _lastSimTime + Config.SimulationStepDuration;
                var nextCmdTime = _lastCmdTime + Config.CommandStepDuration;
                var finished = true;
                if(nextSimTime <= nextCmdTime && nextSimTime <= time)
                {
                    if(Simulate != null)
                    {
                        Simulate(Config.SimulationStepDuration);
                    }
                    _lastSimTime = nextSimTime;
                    simSteps++;
                    if(ClientConfig.MaxSimulationStepsPerFrame <= 0 || simSteps <= ClientConfig.MaxSimulationStepsPerFrame)
                    {
                        finished = false;
                    }
                }
                else if(nextCmdTime <= time)
                {
                    var t = CurrentTurnNumber + 1;
                    if(_lastConfirmedTurnNumber >= t)
                    {
                        Connected = true;
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
                        Connected = false;
                    }
                    if(Connected)
                    {
                        _lastCmdTime = nextCmdTime;
                        finished = false;
                    }
                }
                if(finished)
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

        public void Dispose()
        {
            Stop();
        }

    }
}