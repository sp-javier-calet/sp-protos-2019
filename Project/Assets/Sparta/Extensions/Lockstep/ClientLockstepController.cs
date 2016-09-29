﻿using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Lockstep
{
    [Serializable]
    public sealed class ClientLockstepConfig
    {
        public const int DefaultLocalSimulationDelay = 1000;
        public const int DefaultMaxSimulationStepsPerFrame = 0;
        public const float DefaultSpeedFactor = 1.0f;

        public int LocalSimulationDelay = DefaultLocalSimulationDelay;
        public int MaxSimulationStepsPerFrame = DefaultMaxSimulationStepsPerFrame;
        public float SpeedFactor = DefaultSpeedFactor;
    }

    public class ClientLockstepController : IUpdateable, IDisposable
    {
        IUpdateScheduler _updateScheduler;

        long _timestamp;
        int _time;
        int _lastSimTime;
        int _lastCmdTime;

        Dictionary<Type, ILockstepCommandLogic> _commandLogics = new Dictionary<Type, ILockstepCommandLogic>();
        List<ClientLockstepCommandData> _pendingCommands = new List<ClientLockstepCommandData>();
        List<ClientLockstepTurnData> _confirmedTurns = new List<ClientLockstepTurnData>();

        public bool Connected{ get; private set; }
        public bool Running{ get; private set; }
        public LockstepConfig Config { get; set; }
        public ClientLockstepConfig ClientConfig { get; set; }

        public event Action<ClientLockstepCommandData> CommandAdded;
        public event Action<ClientLockstepCommandData> CommandApplied;
        public event Action Started;
        public event Action ConnectionChanged;
        public event Action<int> Simulate;

        public ClientLockstepController(IUpdateScheduler updateScheduler=null)
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

        public void Start(int dt=0)
        {
            Running = true;
            Connected = true;
            _time = -dt;
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

        public ClientLockstepCommandData AddPendingCommand<T>(T command, ILockstepCommandLogic<T> finish=null) where T : ILockstepCommand
        {
            return AddPendingCommand(command, new LockstepCommandLogic<T>(finish));
        }

        ClientLockstepCommandData AddPendingCommand(ILockstepCommand command, ILockstepCommandLogic logic = null)
        {
            var data = new ClientLockstepCommandData(command, logic);
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
            _confirmedTurns.Add(turn);
        }

        void AddConfirmedCommand(ClientLockstepCommandData cmd)
        {
            var turnCount = ClientConfig.LocalSimulationDelay / Config.CommandStepDuration;
            turnCount = Math.Max(turnCount, 1);
            while(_confirmedTurns.Count < turnCount)
            {
                _confirmedTurns.Add(new ClientLockstepTurnData());
            }
            _confirmedTurns[_confirmedTurns.Count - 1].AddCommand(cmd);
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
            for(var i=0; i<turn.CommandCount; i++)
            {
                var command = FindCommand(turn.GetCommand(i));
                if(command == null)
                {
                    continue;
                }
                var itr = _commandLogics.GetEnumerator();
                while(itr.MoveNext())
                {
                    command.Apply(itr.Current.Key, itr.Current.Value);
                }
                itr.Dispose();
                if(CommandApplied != null)
                {
                    CommandApplied(command);
                }
                command.Finish();
            }
            _confirmedTurns.Remove(turn);
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
            dt = (int)(ClientConfig.SpeedFactor*(float)dt);
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
                if(nextSimTime < nextCmdTime && nextSimTime <= time)
                {
                    if(Simulate != null)
                    {
                        Simulate(Config.SimulationStepDuration);
                    }
                    _lastSimTime = nextSimTime;
                    simSteps++;
                    if(ClientConfig.MaxSimulationStepsPerFrame <= 0 || simSteps < ClientConfig.MaxSimulationStepsPerFrame)
                    {
                        finished = false;
                    }
                }
                else if(nextCmdTime <= time)
                {
                    if(_confirmedTurns.Count > 0)
                    {
                        Connected = true;
                        var turn = _confirmedTurns[0];
                        ProcessTurn(turn);
                    }
                    else if(CommandAdded != null)
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