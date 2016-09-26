using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    public class ClientLockstepController : IUpdateable, IDisposable
    {
        IUpdateScheduler _updateScheduler;

        long _simulationTime;
        long _lastSimulationTime;
        long _lastTimestamp;
        int _lastAppliedTurn;
        int _nextCommandId;

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();
        List<ClientLockstepCommandData> _pendingCommands = new List<ClientLockstepCommandData>();
        Dictionary<int, List<ClientLockstepCommandData>> _confirmedCommands = new Dictionary<int, List<ClientLockstepCommandData>>();

        int CurrentTurn
        {
            get
            {
                return (int)(_simulationTime / Config.CommandStep);
            }
        }

        bool NeedsTurnConfirmation
        {
            get
            {
                return CommandAdded != null;
            }
        }

        public bool Running
        {
            get
            {
                return _simulationTime >= 0;
            }
        }

        int LastConfirmedTurn
        {
            get
            {
                var turn = 0;
                if(_confirmedCommands != null)
                {
                    var itr = _confirmedCommands.GetEnumerator();
                    while(itr.MoveNext())
                    {
                        turn = Math.Max(turn, itr.Current.Key);
                    }
                    itr.Dispose();
                }
                return turn;
            }
        }
            
        const int _maxSimulationStepsPerFrame = 10;

        public LockstepConfig Config { get; private set; }
        public float SimulationSpeed { get; set; }

        public event Action<ClientLockstepCommandData> CommandAdded;
        public event Action<ClientLockstepCommandData, int> CommandApplied;
        public event Action SimulationStarted;
        public event Action<long> Simulate;

        public ClientLockstepController(IUpdateScheduler updateScheduler)
        {
            _updateScheduler = updateScheduler;
            _simulationTime = 0;
            SimulationSpeed = 1f;
        }

        public void Init(LockstepConfig config)
        {
            Config = config;
        }

        public void Start(long timestamp)
        {
            _lastTimestamp = timestamp;
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            _simulationTime = 0;
            _lastSimulationTime = 0;
            _lastTimestamp = 0;
            _lastAppliedTurn = 0;
            _nextCommandId = 0;

            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

        public void RegisterCommandLogic<T>(Action<T> apply) where T:  ILockstepCommand, new()
        {
            RegisterCommandLogic<T>(new ActionLockstepCommandLogic<T>(apply));
        }

        public void RegisterCommandLogic<T>(ILockstepCommandLogic<T> logic) where T:  ILockstepCommand, new()
        {
            RegisterCommandLogic(typeof(T), new LockstepCommandLogic<T>(logic));
        }

        public void RegisterCommandLogic(Type type, ILockstepCommandLogic logic)
        {
            _commandLogics[type] = logic;
        }

        public void AddPendingCommand<T>(T command, Action<T> finish) where T : ILockstepCommand
        {
            AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        public void AddPendingCommand<T>(T command, ILockstepCommandLogic<T> finish=null) where T : ILockstepCommand
        {
            AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        void AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var data = new ClientLockstepCommandData(
                _nextCommandId, command, logic);
            _nextCommandId++;
            AddPendingCommand(data);
        }

        void AddPendingCommand(ClientLockstepCommandData commandData)
        {
            _pendingCommands.Add(commandData);
            if(CommandAdded != null)
            {
                CommandAdded(commandData);
            }
            else
            {
                AddConfirmedCommand(commandData);
            }
        }

        bool IsTurnConfirmed(int turn)
        {
            return _confirmedCommands.ContainsKey(turn);
        }
            
        public void ConfirmTurn(ClientLockstepTurnData confirmation)
        {
            List<ClientLockstepCommandData> commands;
            if(!_confirmedCommands.TryGetValue(confirmation.Turn, out commands))
            {
                commands = new List<ClientLockstepCommandData>();
                _confirmedCommands[confirmation.Turn] = commands;
            }
            commands.AddRange(confirmation.Commands);
        }

        public void AddConfirmedCommand(ClientLockstepCommandData commandData, int turn=-1)
        {
            List<ClientLockstepCommandData> commands;
            turn = turn < 0 ? CurrentTurn + 1 : turn;
            if(!_confirmedCommands.TryGetValue(turn, out commands))
            {
                commands = new List<ClientLockstepCommandData>();
                _confirmedCommands.Add(turn, commands);
            }
            commands.Add(commandData);
        }

        void ApplyTurn(int turn)
        {
            var turns = new List<int>(_confirmedCommands.Keys);
            for(var i=0; i<turns.Count;i++)
            {
                var t = turns[i];
                if(t < turn)
                {
                    ProcessCommands(t, false);
                }
            }
            ProcessCommands(turn, true);
            if(turn > _lastAppliedTurn)
            {
                _lastAppliedTurn = turn;
            }
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

        void ProcessCommands(int turn, bool apply)
        {
            List<ClientLockstepCommandData> commands;
            if(_confirmedCommands.TryGetValue(turn, out commands) && commands != null)
            {
                for(int i = 0; i < commands.Count; ++i)
                {
                    var command = FindCommand(commands[i]);
                    ProcessCommand(command, turn, apply);
                }
                _confirmedCommands.Remove(turn);
            }
        }

        void ProcessCommand(ClientLockstepCommandData command, int turn, bool apply)
        {
            if(apply)
            {
                var itr = _commandLogics.GetEnumerator();
                while(itr.MoveNext())
                {
                    command.Apply(itr.Current.Key, itr.Current.Value);
                }
                itr.Dispose();
                if(CommandApplied != null)
                {
                    CommandApplied(command, turn);
                }
            }
            command.Finish();
        }

        public void Pause()
        {
            _lastTimestamp = long.MaxValue;
        }

        public void Resume()
        {
            _lastTimestamp = TimeUtils.TimestampMilliseconds;
        }

        public void Update()
        {
            long timestamp = TimeUtils.TimestampMilliseconds;
            long elapsedTime = (long)(SimulationSpeed * (float)(timestamp - _lastTimestamp));
            if(elapsedTime <= 0 && SimulationSpeed > 0f)
            {
                return;
            }
            else if(_simulationTime == 0)
            {
                if(SimulationStarted != null)
                {
                    SimulationStarted();
                }
            }
            _simulationTime += elapsedTime;

            var simStep = Config.SimulationStep;
            var comStep = Config.CommandStep;
            var lastTurn = LastConfirmedTurn;
            long maxConfirmedSimulationTime = NeedsTurnConfirmation ? (lastTurn * Config.CommandStep) + comStep - simStep : long.MaxValue;
            if(_lastSimulationTime <= maxConfirmedSimulationTime)
            {
                long simulationTime = Math.Min(maxConfirmedSimulationTime, _simulationTime);
                simulationTime = Math.Min(simulationTime, _lastSimulationTime + _maxSimulationStepsPerFrame * simStep);
                for(long nextST = _lastSimulationTime + simStep; nextST <= simulationTime; nextST += simStep)
                {
                    if(Simulate != null)
                    {
                        Simulate(nextST);
                    }
                    if(nextST >= _lastAppliedTurn * Config.CommandStep + comStep)
                    {
                        ApplyTurn(_lastAppliedTurn + 1);
                    }
                }
                _lastSimulationTime = simulationTime;
            }
            _lastTimestamp = timestamp;
        }

        public void Dispose()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

    }
}