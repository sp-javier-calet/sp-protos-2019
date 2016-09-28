using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public sealed class ServerLockstepController : IUpdateable, IDisposable
    {
        int _time;
        long _timestamp;
        int _lastCmdTime;
        ServerLockstepTurnData _turn;
        IUpdateScheduler _updateScheduler;

        public bool Running{ get; private set; }
        public LockstepConfig Config { get; set; }

        public event Action<ServerLockstepTurnData> TurnReady;

        public ServerLockstepController(IUpdateScheduler updateScheduler = null)
        {
            Config = new LockstepConfig();
            _updateScheduler = updateScheduler;
            _turn = new ServerLockstepTurnData();
            Stop();
        }

        public void AddCommand(ServerLockstepCommandData command)
        {
            _turn.AddCommand(command);
        }

        public void Start()
        {
            Running = true;
            _time = 0;
            _timestamp = TimeUtils.TimestampMilliseconds;
            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this);
            }
        }

        public void Stop()
        {
            Running = false;
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
            }
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
            _time += dt;
            while(true)
            {
                var nextCmdTime = _lastCmdTime + Config.CommandStepDuration;
                if(nextCmdTime > _time)
                {                
                    break;
                }
                if(TurnReady != null)
                {
                    TurnReady(_turn);
                }
                ConfirmLocalClientTurn(_turn);
                _turn.Clear();
                _lastCmdTime = nextCmdTime;
            }
        }

        public void Dispose()
        {
            Stop();
            TurnReady = null;
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
                _localClient.CommandAdded -= AddPendingLocalClientCommand;
            }
            _localClient = null;
        }

        public void RegisterLocalClient(ClientLockstepController client, LockstepCommandFactory factory)
        {
            RemoveLocalClient();
            _localClient = client;
            _localFactory = factory;
            _localClient.Config = Config;
            if(_localClient != null)
            {
                _localClient.CommandAdded += AddPendingLocalClientCommand;
            }
        }

        void AddPendingLocalClientCommand(ClientLockstepCommandData command)
        {
            command.ClientId = LocalClientId;
            var serverCommand = command.ToServer(_localFactory);
            AddCommand(serverCommand);
        }

        void ConfirmLocalClientTurn(ServerLockstepTurnData turn)
        {
            if(_localClient == null)
            {
                return;
            }
            var clientTurn = turn.ToClient(_localFactory);
            _localClient.AddConfirmedTurn(clientTurn);
        }

        #endregion
    }
}