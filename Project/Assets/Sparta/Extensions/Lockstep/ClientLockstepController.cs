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
        long _simulationStep;
        long _commandStep = 10;
        long _lastConfirmedTurnTime;
        int _lastConfirmedTurn;
        int _lastAppliedTurn;
        long _lastAppliedTurnTime;
        int _maxRetries;
        bool _missingTurn;
        float _simulationSpeed;
        int _nextCommandId;
        bool[] _pendingCommandResults = new bool[4];
        int _pendingCommandResultsIndex = 0;

        public float SimulationSpeed
        {
            get
            {
                return _simulationSpeed * DesiredSimulationSpeed;
            }
        }

        public float DesiredSimulationSpeed { get; set; }

        public long LastConfirmedTurn
        {
            get
            {
                return _lastConfirmedTurn;
            }
        }

        const int _maxSimulationStepsPerFrame = 10;

        public int ExecutionTurnAnticipation { get; set; }

        public int MinExecutionTurnAnticipation { get; set; }

        public int MaxExecutionTurnAnticipation { get; set; }

        public int CurrentTurn
        {
            get
            {
                return (int)(_simulationTime / _commandStep);
            }
        }

        public int ExecutionTurn
        {
            get
            {
                return CurrentTurn + ExecutionTurnAnticipation;
            }
        }

        bool NeedsTurnConfirmation
        {
            get
            {
                return PendingCommandAdded != null;
            }
        }

        public float TurnAnticipationAdjustmentFactor { get; set; }

        public event Action<int> MissingTurnConfirmation;
        public event Action<int> MissingTurnConfirmationReceived;
        public event Action<int[]> TurnsConfirmed;
        public event Action<LockstepCommandData> PendingCommandAdded;
        public event Action<long> SimulationStartScheduled;
        public event Action SimulationStarted;
        public event Action<LockstepCommandData> CommandApplied;
        public event Action<long> Simulate;

        public LockstepConfig LockstepConfig { get; protected set; }

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();

        Dictionary<int, List<LockstepCommandData>> _pendingCommands = new Dictionary<int, List<LockstepCommandData>>();
        Dictionary<int, List<LockstepCommandData>> _confirmedCommands = new Dictionary<int, List<LockstepCommandData>>();

        public ClientLockstepController(IUpdateScheduler updateScheduler)
        {
            _updateScheduler = updateScheduler;
            _simulationTime = 0;
            _lastConfirmedTurnTime = 0;
            _lastConfirmedTurn = 0;
            TurnAnticipationAdjustmentFactor = 0.7f;
            DesiredSimulationSpeed = 1f;
        }

        public void Init(LockstepConfig config)
        {
            LockstepConfig = config;
            _simulationStep = config.SimulationStep;
            _commandStep = config.CommandStep;
            ExecutionTurnAnticipation = config.ExecutionTurnAnticipation;
            MinExecutionTurnAnticipation = config.MinExecutionTurnAnticipation;
            MaxExecutionTurnAnticipation = config.MaxExecutionTurnAnticipation;
            _maxRetries = config.MaxRetries;
        }

        public void Start(long timestamp)
        {
            _lastTimestamp = timestamp;
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
            if(SimulationStartScheduled != null)
            {
                SimulationStartScheduled(timestamp);
            }
        }

        public void Stop()
        {
            _simulationTime = 0;
            _lastModelSimulationTime = 0;
            _lastRawModelSimulationTime = 0;
            _lastTimestamp = 0;
            _lastConfirmedTurnTime = 0;
            _lastConfirmedTurn = 0;
            _lastAppliedTurn = 0;
            _lastAppliedTurnTime = 0;
            _missingTurn = false;
            _nextCommandId = 0;
            _pendingCommandResults = new bool[4];
            _pendingCommandResultsIndex = 0;

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

        void ReportPendingCommandResult(bool result)
        {
            _pendingCommandResults[_pendingCommandResultsIndex] = result;

            _pendingCommandResultsIndex = (_pendingCommandResultsIndex + 1) % _pendingCommandResults.Length;

            // Adjust the turn anticipation when all the samples received
            if(_pendingCommandResultsIndex == 0)
            {
                int confirmedCommands = 0;
                for(int i = 0; i < _pendingCommandResults.Length; ++i)
                {
                    if(_pendingCommandResults[i])
                    {
                        confirmedCommands++;
                    }
                }
                float successRate = (float)confirmedCommands / (float)_pendingCommandResults.Length;
                if(successRate >= TurnAnticipationAdjustmentFactor)
                {
                    var previousAnticipation = ExecutionTurnAnticipation;
                    ExecutionTurnAnticipation = Math.Max(MinExecutionTurnAnticipation, ExecutionTurnAnticipation - 1);
                    if(previousAnticipation != ExecutionTurnAnticipation)
                    {
                        Log.i("Turn anticipation decreased to " + ExecutionTurnAnticipation);
                    }
                }
                if(successRate <= 1f - TurnAnticipationAdjustmentFactor)
                {
                    ExecutionTurnAnticipation = Math.Min(MaxExecutionTurnAnticipation, ExecutionTurnAnticipation + 1);
                }
            }
        }

        void UpdateTurnConfirmations()
        {
            int currentTurn = CurrentTurn;
            while(_lastConfirmedTurn < currentTurn)
            {
                int nextTurn = _lastConfirmedTurn + 1;
                if(!NeedsTurnConfirmation || IsTurnConfirmed(nextTurn))
                {
                    _lastConfirmedTurn++;
                    _lastConfirmedTurnTime += _commandStep;
                }
                else
                {
                    if(!_missingTurn)
                    {
                        _missingTurn = true;
                        if(MissingTurnConfirmation != null)
                        {
                            MissingTurnConfirmation(nextTurn);
                        }
                    }
                    return;
                }
            }

            if(_missingTurn)
            {
                _missingTurn = false;
                if(MissingTurnConfirmationReceived != null)
                {
                    MissingTurnConfirmationReceived(currentTurn);
                }
            }
        }

        public void AddPendingCommand<T>(T command, Action<T> dlg) where T : ILockstepCommand
        {
            AddPendingCommand(command, new LockstepCommandLogic<T>(dlg));
        }

        public void AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var commandData = new LockstepCommandData {
                Id = _nextCommandId,
                Command = command,
                Turn = ExecutionTurn,
                Logic = logic
            };
            _nextCommandId++;
            AddPendingCommand(commandData);
        }

        public void AddPendingCommand(LockstepCommandData commandData)
        {
            List<LockstepCommandData> commands;
            if(!_pendingCommands.TryGetValue(commandData.Turn, out commands))
            {
                commands = new List<LockstepCommandData>();
                _pendingCommands.Add(commandData.Turn, commands);
            }
            commands.Add(commandData);
            if(PendingCommandAdded != null)
            {
                PendingCommandAdded(commandData);
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

        public void ConfirmTurn(int turn, List<LockstepCommandData> commands)
        {
            DoConfirmTurn(turn, commands);
            if(TurnsConfirmed != null)
            {
                TurnsConfirmed(new int[1]{ turn });
            }
        }

        public void DoConfirmTurn(int turn, List<LockstepCommandData> commands)
        {
            _confirmedCommands[turn] = commands;
        }

        public void ConfirmTurns(LockstepTurnData[] confirmations)
        {
            int[] confirmedTurns = new int[confirmations.Length];
            for(int i = 0; i < confirmations.Length; ++i)
            {
                var confirmation = confirmations[i];
                DoConfirmTurn(confirmation.Turn, confirmation.Commands);
                confirmedTurns[i] = confirmation.Turn;
            }
            if(TurnsConfirmed != null)
            {
                TurnsConfirmed(confirmedTurns);
            }
        }

        public void AddConfirmedCommand(LockstepCommandData commandData)
        {
            List<LockstepCommandData> commands;
            if(!_confirmedCommands.TryGetValue(commandData.Turn, out commands))
            {
                commands = new List<LockstepCommandData>();
                _confirmedCommands.Add(commandData.Turn, commands);
            }
            commands.Add(commandData);
        }

        void ConsumeTurn(int turn)
        {
            List<LockstepCommandData> commands;
            List<LockstepCommandData> pendingCommands = null;
            if(_pendingCommands.TryGetValue(turn, out pendingCommands))
            {
                _pendingCommands.Remove(turn);
            }

            if(_confirmedCommands.TryGetValue(turn, out commands) && commands != null)
            {
                for(int i = 0; i < commands.Count; ++i)
                {
                    var command = commands[i];
                    bool applied = false;
                    if(pendingCommands != null)
                    {
                        for(int j = 0; j < pendingCommands.Count; ++j)
                        {
                            var pendingCommand = pendingCommands[j];
                            if(pendingCommand.Equals(command))
                            {
                                ReportPendingCommandResult(true);
                                ApplyCommand(pendingCommand);
                                applied = true;
                                pendingCommands.Remove(pendingCommand);
                                break;
                            }
                        }
                    }
                    if(!applied)
                    {
                        ApplyCommand(command);
                    }
                }
                _confirmedCommands.Remove(turn);
            }

            if(pendingCommands != null)
            {
                for(int i = 0; i < pendingCommands.Count; ++i)
                {
                    var pendingCommand = pendingCommands[i];
                    ReportPendingCommandResult(false);
                    if(pendingCommand.Retries >= _maxRetries)
                    {
                        pendingCommand.Discard();
                    }
                    else
                    {
                        if(pendingCommand.Retry(CurrentTurn + ExecutionTurnAnticipation + pendingCommand.Retries + 1))
                        {
                            AddPendingCommand(pendingCommand);
                        }
                    }
                }
            }

            if(turn > _lastAppliedTurn)
            {
                _lastAppliedTurn = turn;
                _lastAppliedTurnTime = turn * _commandStep;
            }
        }

        void ApplyCommand(LockstepCommandData command)
        {
            var itr = _commandLogics.GetEnumerator();
            while(itr.MoveNext())
            {
                if(itr.Current.Key.IsAssignableFrom(command.Command.GetType()))
                {
                    itr.Current.Value.Apply(command.Command);
                }
            }
            itr.Dispose();
            command.Apply();
            if(CommandApplied != null)
            {
                CommandApplied(command);
            }
        }

        public void Pause()
        {
            _lastTimestamp = long.MaxValue;
        }

        public void Resume()
        {
            _lastTimestamp = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
        }

        #region IUpdateable implementation

        public void Update()
        {
            long timestamp = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            long elapsedTime = (long)(DesiredSimulationSpeed * (float)(timestamp - _lastTimestamp));
            if(elapsedTime <= 0 && DesiredSimulationSpeed > 0f)
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

            long maxConfirmedSimulationTime = NeedsTurnConfirmation ? _lastConfirmedTurnTime + _commandStep - _simulationStep : long.MaxValue;
            if(_lastModelSimulationTime <= maxConfirmedSimulationTime)
            {
                long nextModelSimulationTime = Math.Min(maxConfirmedSimulationTime, _simulationTime);
                nextModelSimulationTime = Math.Min(nextModelSimulationTime, _lastModelSimulationTime + _maxSimulationStepsPerFrame * _simulationStep);
                long elapsedSimulationTime = nextModelSimulationTime - _lastRawModelSimulationTime;
                for(long nextST = _lastModelSimulationTime + _simulationStep; nextST <= nextModelSimulationTime; nextST += _simulationStep)
                {
                    _lastModelSimulationTime = nextST;
                    if(Simulate != null)
                    {
                        Simulate(_lastModelSimulationTime);
                    }
                    if(_lastModelSimulationTime >= _lastAppliedTurnTime + _commandStep)
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

        #endregion

        #region IDisposable implementation

        public void Dispose()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
        }

        #endregion
    }
}