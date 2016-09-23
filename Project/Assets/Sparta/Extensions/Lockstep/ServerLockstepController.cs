﻿using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public sealed class ServerLockstepController : IUpdateable, IDisposable
    {
        public long CommandStep;
        long _simulationTime;
        long _lastTimestamp;
        bool _isRunning;
        ServerLockstepTurnData _turn;
        IUpdateScheduler _updateScheduler;

        public int CurrentTurn
        {
            get
            {
                return (int)(_simulationTime / CommandStep);
            }
        }

        public bool Running
        {
            get
            {
                return _isRunning;
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
            _turn = new ServerLockstepTurnData(-1);
        }

        public void OnClientCommandReceived(ServerLockstepCommandData command)
        {
            _turn.Commands.Add(command);
        }

        public void Start(long timestamp)
        {
            _isRunning = true;
            _lastTimestamp = timestamp;
        }

        public void Stop()
        {
            _isRunning = false;
            _simulationTime = 0;
            _lastTimestamp = 0;
            _turn.Turn = -1;
        }           

        public Action<ServerLockstepTurnData> SendClientTurnData;

        void SendTurnData()
        {
            if(SendClientTurnData != null)
            {
                SendClientTurnData(_turn);
            }
            SendLocalClientTurnData();
            _turn.Commands.Clear();
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
            while(_turn.Turn < currentTurn)
            {
                SendTurnData();
                _turn.Turn++;
            }
        }

        public void Dispose()
        {
            SendClientTurnData = null;
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
            RemoveLocalClient();
        }

        #region local client implementation

        ClientLockstepController _localClient;
        LockstepCommandFactory _localFactory;
        const int LocalClientId = -1;

        void RemoveLocalClient()
        {
            if(_localClient != null)
            {
                _localClient.PendingCommandAdded -= AddPendingLocalClientCommand;
            }
            _localClient = null;
        }

        public void RegisterLocalClient(ClientLockstepController client, LockstepCommandFactory factory)
        {
            RemoveLocalClient();
            _localClient = client;
            _localFactory = factory;
            if(_localClient != null)
            {
                _localClient.PendingCommandAdded += AddPendingLocalClientCommand;
            }
        }

        void AddPendingLocalClientCommand(ClientLockstepCommandData command, int turn)
        {
            command.ClientId = LocalClientId;
            var serverCommand = command.ToServer(_localFactory);
            OnClientCommandReceived(serverCommand);
        }

        void SendLocalClientTurnData()
        {
            if(_localClient == null)
            {
                return;
            }
            var clientTurn = _turn.ToClient(_localFactory);
            _localClient.ConfirmTurn(clientTurn);
        }

        #endregion
    }
}