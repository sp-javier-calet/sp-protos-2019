using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public class ServerLockstepController : IUpdateable, IDisposable
    {
        public long CommandStep;
        long _simulationTime;
        long _lastTimestamp;
        int _lastTurn;
        bool _isRunning;
        Dictionary<int, LockstepTurnData> _turns = new Dictionary<int, LockstepTurnData>();
        HashSet<int>[] _pendingConfirmationsPerClient;
        IUpdateScheduler _updateScheduler;

        public int CurrentTurn
        {
            get
            {
                return (int)(_simulationTime / CommandStep);
            }
        }

        public ServerLockstepController(IUpdateScheduler updateScheduler, int clientCount = 2, long commandStep = 300)
        {
            CommandStep = commandStep;
            if(updateScheduler != null)
            {
                _updateScheduler = updateScheduler;
                updateScheduler.Add(this);
            }
            _lastTurn = -1;
            _pendingConfirmationsPerClient = new HashSet<int>[clientCount];
            for(int i = 0; i < clientCount; ++i)
            {
                _pendingConfirmationsPerClient[i] = new HashSet<int>();
            }
        }

        public void OnClientCommandReceived(int client, ILockstepCommand command)
        {
            // If the execution turn is not in the future, ignore it.
            if(command.Turn > _lastTurn)
            {
                LockstepTurnData turnData;
                if(!_turns.TryGetValue(command.Turn, out turnData))
                {
                    turnData = new LockstepTurnData(command.Turn);
                    turnData.Commands = new List<ILockstepCommand>();
                    _turns.Add(turnData.Turn, turnData);
                }
                turnData.Commands.Add(command);
            }
        }

        public void OnClientTurnReceptionConfirmed(int client, int turn)
        {
            _pendingConfirmationsPerClient[client].Remove(turn);
        }

        public void Start(long timestamp)
        {
            _isRunning = true;
            _lastTimestamp = timestamp;
        }

        public void Stop()
        {
            _isRunning = false;
        }

        void RemoveClientConfirmedTurns()
        {
            List<int> turnsToRemove = null;
            var enumerator = _turns.GetEnumerator();
            while(enumerator.MoveNext())
            {
                if(enumerator.Current.Key > _lastTurn)
                {
                    continue;
                }
                bool isAnyPendingConfirmation = false;
                for(int i = 0; i < _pendingConfirmationsPerClient.Length; ++i)
                {
                    if(_pendingConfirmationsPerClient[i].Contains(enumerator.Current.Key))
                    {
                        isAnyPendingConfirmation = true;
                        break;
                    }
                }
                if(!isAnyPendingConfirmation)
                {
                    if(turnsToRemove == null)
                    {
                        turnsToRemove = new List<int>();
                    }
                    turnsToRemove.Add(enumerator.Current.Key);
                }
            }
            enumerator.Dispose();

            if(turnsToRemove != null)
            {
                for(int i = 0; i < turnsToRemove.Count; ++i)
                {
                    _turns.Remove(turnsToRemove[i]);
                }
            }
        }

        public Action<int, LockstepTurnData[]> SendClientTurnData;

        void SendTurnData()
        {
            for(int i = 0; i < _pendingConfirmationsPerClient.Length; ++i)
            {
                var pendingConfirmation = _pendingConfirmationsPerClient[i];
                LockstepTurnData[] pendingTurns = new LockstepTurnData[pendingConfirmation.Count];
                int j = 0;
                var enumerator = pendingConfirmation.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    pendingTurns[j++] = _turns[enumerator.Current];
                }
                enumerator.Dispose();

                if(SendClientTurnData != null)
                {
                    SendClientTurnData(i, pendingTurns);
                }
            }
        }

        void CreateTurnIfEmpty(int turn)
        {
            if(!_turns.ContainsKey(turn))
            {
                _turns.Add(turn, new LockstepTurnData(turn));
            }
        }

        public void Update()
        {
            if(!_isRunning)
            {
                return;
            }
            long timestamp = TimeUtils.TimestampMilliseconds;
            long elapsedTime = timestamp - _lastTimestamp;
            if(elapsedTime <= 0)
            {
                return;
            }
            _simulationTime += elapsedTime;
            _lastTimestamp = timestamp;
            long currentTurn = CurrentTurn;
            while(_lastTurn < currentTurn)
            {
                RemoveClientConfirmedTurns();
                _lastTurn++;
                for(int i = 0; i < _pendingConfirmationsPerClient.Length; ++i)
                {
                    _pendingConfirmationsPerClient[i].Add(_lastTurn);
                }
                CreateTurnIfEmpty(_lastTurn);
                SendTurnData();
            }
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