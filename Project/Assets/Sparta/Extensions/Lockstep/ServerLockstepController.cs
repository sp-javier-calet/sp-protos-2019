using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public sealed class ServerLockstepController : IUpdateable, IDisposable
    {
        public long CommandStep;
        long _simulationTime;
        long _lastTimestamp;
        int _lastTurn;
        bool _isRunning;
        Dictionary<int, LockstepTurnData> _turns;
        Dictionary<byte, HashSet<int>> _pendingTurns;
        IUpdateScheduler _updateScheduler;

        public int CurrentTurn
        {
            get
            {
                return (int)(_simulationTime / CommandStep);
            }
        }

        public ServerLockstepController(IUpdateScheduler updateScheduler, long commandStep = 300)
        {
            CommandStep = commandStep;
            if(updateScheduler != null)
            {
                _updateScheduler = updateScheduler;
                updateScheduler.Add(this);
            }
            _lastTurn = -1;
            _turns = new Dictionary<int, LockstepTurnData>();
            _pendingTurns = new Dictionary<byte, HashSet<int>>();
        }

        public void OnClientCommandReceived(byte client, LockstepCommandData command)
        {
            // If the execution turn is not in the future, ignore it.
            if(command.Turn > _lastTurn)
            {
                LockstepTurnData turnData;
                if(!_turns.TryGetValue(command.Turn, out turnData))
                {
                    turnData = new LockstepTurnData(command.Turn);
                    turnData.Commands = new List<LockstepCommandData>();
                    _turns.Add(turnData.Turn, turnData);
                }
                turnData.Commands.Add(command);
            }
        }

        public void OnClientTurnReceptionConfirmed(byte client, int turn)
        {
            HashSet<int> turns;
            if(_pendingTurns.TryGetValue(client, out turns))
            {
                turns.Remove(turn);
            }
        }

        public void Start(long timestamp, byte[] clients)
        {
            for(int i = 0; i < clients.Length; ++i)
            {
                _pendingTurns[clients[i]] = new HashSet<int>();
            }

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
                var itr = _pendingTurns.GetEnumerator();
                while(itr.MoveNext())
                {
                    if(itr.Current.Value.Contains(enumerator.Current.Key))
                    {
                        isAnyPendingConfirmation = true;
                        break;
                    }
                }
                itr.Dispose();
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

        public Action<byte, LockstepTurnData[]> SendClientTurnData;

        void SendTurnData()
        {
            var itr = _pendingTurns.GetEnumerator();
            while(itr.MoveNext())
            {
                var pendingConfirmation = itr.Current.Value;
                var pendingTurns = new LockstepTurnData[pendingConfirmation.Count];
                int j = 0;
                var enumerator = pendingConfirmation.GetEnumerator();
                while(enumerator.MoveNext())
                {
                    pendingTurns[j++] = _turns[enumerator.Current];
                }
                enumerator.Dispose();

                if(SendClientTurnData != null)
                {
                    SendClientTurnData(itr.Current.Key, pendingTurns);
                }
            }
            itr.Dispose();
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
                var itr = _pendingTurns.GetEnumerator();
                while(itr.MoveNext())
                {
                    itr.Current.Value.Add(_lastTurn);
                }
                itr.Dispose();
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