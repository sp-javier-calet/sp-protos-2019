using System;
using SocialPoint.Utils;
using System.IO;

namespace SocialPoint.Lockstep.Network
{
    public class ClientLockstepNetworkController : IDisposable
    {
        INetworkMessageController _client;
        LockstepCommandDataFactory _networkCommandDataFactory;
        ClientLockstepController _clientLockstep;
        LockstepConfig _lockstepConfig;

        public int PlayerId { get; private set; }

        public byte LockstepCommandMsgType { get; protected set; }

        public byte ConfirmTurnsMsgType { get; protected set; }

        public byte ConfirmTurnsReceptionMsgType { get; protected set; }

        public byte SetLockstepConfigMsgType { get; protected set; }

        public byte AllClientsReadyMsgType { get; protected set; }

        public byte ClientReadyMsgType { get; protected set; }

        public event Action<int, LockstepConfig> LockstepConfigReceived;

        public ClientLockstepNetworkController(INetworkMessageController client,
                                               byte lockstepCommandMsgType = 102,
                                               byte confirmTurnsMsgType = 103,
                                               byte confirmTurnsReceptionMsgType = 104,
                                               byte setLockstepConfigMsgType = 105,
                                               byte clientReadyMsgType = 106,
                                               byte allClientsReadyMsgType = 107)
        {
            _client = client;
            LockstepCommandMsgType = lockstepCommandMsgType;
            ConfirmTurnsMsgType = confirmTurnsMsgType;
            ConfirmTurnsReceptionMsgType = confirmTurnsReceptionMsgType;
            SetLockstepConfigMsgType = setLockstepConfigMsgType;
            ClientReadyMsgType = clientReadyMsgType;
            AllClientsReadyMsgType = allClientsReadyMsgType;
            RegisterHandlers();
        }

        public void Init(ClientLockstepController clientLockstep,
                         LockstepCommandDataFactory networkCommandDataFactory)
        {
            _clientLockstep = clientLockstep;
            _networkCommandDataFactory = networkCommandDataFactory;
            if(_lockstepConfig != null)
            {
                _clientLockstep.Init(_lockstepConfig);
            }

            _clientLockstep.TurnsConfirmed += OnTurnsConfirmed;
            _clientLockstep.PendingCommandAdded += OnPendingCommandAdded;
        }

        void RegisterHandlers()
        {
            _client.RegisterHandler(ConfirmTurnsMsgType, OnConfirmTurnsReceived);
            _client.RegisterHandler(SetLockstepConfigMsgType, OnSetLockstepConfigReceived);
            _client.RegisterSyncHandler(AllClientsReadyMsgType, OnAllClientsReadyMsgTypeReceived);
        }

        void UnregisterHandlers()
        {
            _client.UnregisterHandler(ConfirmTurnsMsgType);
            _client.UnregisterHandler(SetLockstepConfigMsgType);
            _client.UnregisterHandler(AllClientsReadyMsgType);
        }

        void OnConfirmTurnsReceived(NetworkMessageData data)
        {
            ConfirmTurnsMessage turnsAction = new ConfirmTurnsMessage(_networkCommandDataFactory);
            turnsAction.Deserialize(data.Reader);
            _clientLockstep.ConfirmTurns(turnsAction.ConfirmedTurns);
        }

        void OnSetLockstepConfigReceived(NetworkMessageData data)
        {
            var msg = new SetLockstepConfigMessage();
            msg.Deserialize(data.Reader);
            _lockstepConfig = msg.Config;
            PlayerId = (int)msg.PlayerId;
            if(_clientLockstep != null)
            {
                _clientLockstep.Init(_lockstepConfig);
            }
            if(LockstepConfigReceived != null)
            {
                LockstepConfigReceived(PlayerId, msg.Config);
            }
        }

        public void SendClientReady()
        {
            _client.Send(ClientReadyMsgType, new EmptyMessage(), NetworkReliability.Reliable);
        }

        void OnAllClientsReadyMsgTypeReceived(SyncNetworkMessageData data)
        {
            AllClientsReadyMessage msg = new AllClientsReadyMessage();
            msg.Deserialize(data.Reader);
            int remaining = msg.RemainingMillisecondsToStart - data.ServerDelay;
            _clientLockstep.Start(SocialPoint.Utils.TimeUtils.TimestampMilliseconds + (long)remaining);
        }

        void OnTurnsConfirmed(int[] turns)
        {
            var confirmTurnReception = new ConfirmTurnsReceptionMessage(turns);
            _client.Send(ConfirmTurnsReceptionMsgType, confirmTurnReception, NetworkReliability.Unreliable);
        }

        void OnPendingCommandAdded(ILockstepCommand command)
        {
            LockstepCommandMessage action = new LockstepCommandMessage(_networkCommandDataFactory, command);
            _client.Send(LockstepCommandMsgType, action, NetworkReliability.Unreliable);
        }

        public void Dispose()
        {
            UnregisterHandlers();
            if(_clientLockstep != null)
            {
                _clientLockstep.TurnsConfirmed -= OnTurnsConfirmed;
                _clientLockstep.PendingCommandAdded -= OnPendingCommandAdded;
            }
        }
    }
}