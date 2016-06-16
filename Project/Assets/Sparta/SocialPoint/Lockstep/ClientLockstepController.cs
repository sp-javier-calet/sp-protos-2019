using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class ClientLockstepController : IUpdateable, IDisposable
    {
        ISimulateable _model;
        IUpdateScheduler _updateScheduler;

        long _simulationTime;
        long _lastModelSimulationTime;
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

        public bool NeedsTurnConfirmation { get; set; }

        public float TurnAnticipationAdjustmentFactor { get; set; }

        bool[] _pendingCommandResults = new bool[4];
        int _pendingCommandResultsIndex = 0;

        public event Action<int> MissingTurnConfirmation;
        public event Action<int> MissingTurnConfirmationReceived;

        public event Action<int[]> TurnsConfirmed;
        public event Action<ILockstepCommand> PendingCommandAdded;
        public event Action<long> SimulationStartScheduled;
        public event Action SimulationStarted;
        public event Action<ILockstepCommand> CommandApplied;

        public LockstepConfig LockstepConfig { get; protected set; }

        Dictionary<int, List<ILockstepCommand>> _pendingCommands = new Dictionary<int, List<ILockstepCommand>>();

        Dictionary<int, List<ILockstepCommand>> _confirmedCommands = new Dictionary<int, List<ILockstepCommand>>();

        public ClientLockstepController(ISimulateable model,
                                        IUpdateScheduler updateScheduler)
        {
            _updateScheduler = updateScheduler;				
            NeedsTurnConfirmation = true;
            _model = model;
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
                    ExecutionTurnAnticipation = Math.Max(MinExecutionTurnAnticipation, ExecutionTurnAnticipation - 1);
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

        public void AddPendingCommand(ILockstepCommand command)
        {
//            UnityEngine.Debug.Log("Pending command: " + CurrentTurn + " (" + command.Turn + ")");
            List<ILockstepCommand> commands;
            if(!_pendingCommands.TryGetValue(command.Turn, out commands))
            {
                commands = new List<ILockstepCommand>();
                _pendingCommands.Add(command.Turn, commands);
            }
            commands.Add(command);
            if(PendingCommandAdded != null)
            {
                PendingCommandAdded(command);
            }
        }

        bool IsTurnConfirmed(int turn)
        {
            return _confirmedCommands.ContainsKey(turn);
        }

        public void ConfirmTurn(int turn, List<ILockstepCommand> commands)
        {
            DoConfirmTurn(turn, commands);
            if(TurnsConfirmed != null)
            {
                TurnsConfirmed(new int[1]{ turn });
            }
        }

        public void DoConfirmTurn(int turn, List<ILockstepCommand> commands)
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

        public void AddConfirmedCommand(ILockstepCommand command)
        {
            List<ILockstepCommand> commands;
            if(!_confirmedCommands.TryGetValue(command.Turn, out commands))
            {
                commands = new List<ILockstepCommand>();
                _confirmedCommands.Add(command.Turn, commands);
            }
            commands.Add(command);
        }

        void ConsumeTurn(int turn)
        {
//            UnityEngine.Debug.Log("Consume turn: " + turn + " simulation time: " + _simulationTime);
            List<ILockstepCommand> commands;
            List<ILockstepCommand> pendingCommands = null;
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

        void ApplyCommand(ILockstepCommand command)
        {
            command.Apply();
            if(CommandApplied != null)
            {
                CommandApplied(command);
            }
        }

        #region IUpdateable implementation

        long _lastRawModelSimulationTime;

        public void Update()
        {
            long timestamp = SocialPoint.Utils.TimeUtils.TimestampMilliseconds;
            long elapsedTime = (long)(DesiredSimulationSpeed * (float)(timestamp - _lastTimestamp));
            if(elapsedTime <= 0)
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
                    _model.Simulate(_lastModelSimulationTime);
                    if(_lastModelSimulationTime >= _lastAppliedTurnTime + _commandStep)
                    {
                        ConsumeTurn(_lastAppliedTurn + 1);
                    }
                }
                _lastRawModelSimulationTime = nextModelSimulationTime;
                _simulationSpeed = (float)elapsedSimulationTime / (float)elapsedTime;
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