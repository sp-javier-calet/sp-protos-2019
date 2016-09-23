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
        long _lastModelSimulationTime;
        long _lastRawModelSimulationTime;
        long _lastTimestamp;
        int _lastConfirmedTurn;
        int _lastAppliedTurn;
        float _simulationSpeed;
        int _nextCommandId;

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();
        List<ClientLockstepCommandData> _pendingCommands = new List<ClientLockstepCommandData>();
        Dictionary<int, List<ClientLockstepCommandData>> _confirmedCommands = new Dictionary<int, List<ClientLockstepCommandData>>();

        public float SimulationSpeed
        {
            get
            {
                return _simulationSpeed * SimulationSpeedFactor;
            }
        }

        public long LastConfirmedTurn
        {
            get
            {
                return _lastConfirmedTurn;
            }
        }

        public int CurrentTurn
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


        const int _maxSimulationStepsPerFrame = 10;

        public LockstepConfig Config { get; private set; }
        public float SimulationSpeedFactor { get; set; }

        public event Action<ClientLockstepCommandData> CommandAdded;
        public event Action<ClientLockstepCommandData, int> CommandApplied;
        public event Action SimulationStarted;
        public event Action<long> Simulate;

        public ClientLockstepController(IUpdateScheduler updateScheduler)
        {
            _updateScheduler = updateScheduler;
            _simulationTime = 0;
            _lastConfirmedTurn = 0;
            SimulationSpeedFactor = 1f;
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
            _lastModelSimulationTime = 0;
            _lastRawModelSimulationTime = 0;
            _lastTimestamp = 0;
            _lastConfirmedTurn = 0;
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

        void UpdateTurnConfirmations()
        {
            int currentTurn = CurrentTurn;
            if(!NeedsTurnConfirmation)
            {
                _lastConfirmedTurn = currentTurn - 1;
                return;
            }
            for(; _lastConfirmedTurn < currentTurn; _lastConfirmedTurn++)
            {
                if(!IsTurnConfirmed(_lastConfirmedTurn + 1))
                {
                    break;
                }
            }
        }

        public void AddPendingCommand<T>(T command, Action<T> dlg) where T : ILockstepCommand
        {
            AddPendingCommand(command, new LockstepCommandLogic<T>(dlg));
        }

        public void AddPendingCommand<T>(T command, ILockstepCommandLogic<T> dlg=null) where T : ILockstepCommand
        {
            AddPendingCommand(command, new LockstepCommandLogic<T>(dlg));
        }

        void AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var commandData = new ClientLockstepCommandData(
                _nextCommandId, command, logic);
            _nextCommandId++;
            AddPendingCommand(commandData);
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

        void ConsumeTurn(int turn)
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

        void DiscardCommand(ClientLockstepCommandData command, int turn)
        {
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
            long elapsedTime = (long)(SimulationSpeedFactor * (float)(timestamp - _lastTimestamp));
            if(elapsedTime <= 0 && SimulationSpeedFactor > 0f)
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
            UpdateTurnConfirmations();

            var simStep = Config.SimulationStep;
            var comStep = Config.CommandStep;
            long maxConfirmedSimulationTime = NeedsTurnConfirmation ? (_lastConfirmedTurn * Config.CommandStep) + comStep - simStep : long.MaxValue;
            if(_lastModelSimulationTime <= maxConfirmedSimulationTime)
            {
                long nextModelSimulationTime = Math.Min(maxConfirmedSimulationTime, _simulationTime);
                nextModelSimulationTime = Math.Min(nextModelSimulationTime, _lastModelSimulationTime + _maxSimulationStepsPerFrame * simStep);
                long elapsedSimulationTime = nextModelSimulationTime - _lastRawModelSimulationTime;
                for(long nextST = _lastModelSimulationTime + simStep; nextST <= nextModelSimulationTime; nextST += simStep)
                {
                    _lastModelSimulationTime = nextST;
                    if(Simulate != null)
                    {
                        Simulate(_lastModelSimulationTime);
                    }
                    if(_lastModelSimulationTime >= _lastAppliedTurn * Config.CommandStep + comStep)
                    {
                        ConsumeTurn(_lastAppliedTurn + 1);
                    }
                }
                _lastRawModelSimulationTime = nextModelSimulationTime;
                _simulationSpeed = elapsedTime > 0f ? ((float)elapsedSimulationTime / (float)elapsedTime) : 0f; 
            }
            else
            {
                _simulationSpeed = 0f;
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