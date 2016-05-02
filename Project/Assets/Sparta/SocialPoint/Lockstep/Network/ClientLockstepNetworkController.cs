using System;
using UnityEngine.Networking;
using SocialPoint.Utils;
using UnityEngine.Networking.NetworkSystem;

namespace SocialPoint.Lockstep.Network
{
    public class ClientLockstepNetworkController : IDisposable
    {
        NetworkClient _client;
        NetworkLockstepCommandDataFactory _networkCommandDataFactory;
        ClientLockstepController _clientLockstep;
        LockstepConfig _lockstepConfig;

        public short LockstepCommandMsgType { get; protected set; }

        public short ConfirmTurnsMsgType { get; protected set; }

        public short ConfirmTurnsReceptionMsgType { get; protected set; }

        public short SetLockstepConfigMsgType { get; protected set; }

        public short AllClientsReadyMsgType { get; protected set; }

        public short ClientReadyMsgType { get; protected set; }

        public event Action<LockstepConfig> LockstepConfigReceived;

        public ClientLockstepNetworkController(NetworkClient client,
                                               short lockstepCommandMsgType = 2002,
                                               short confirmTurnsMsgType = 2003,
                                               short confirmTurnsReceptionMsgType = 2004,
                                               short setLockstepConfigMsgType = 2005,
                                               short clientReadyMsgType = 2006,
                                               short allClientsReadyMsgType = 2007)
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
                         NetworkLockstepCommandDataFactory networkCommandDataFactory)
        {
            _clientLockstep = clientLockstep;
            _networkCommandDataFactory = networkCommandDataFactory;
            _clientLockstep.Init(_lockstepConfig);

            _clientLockstep.TurnsConfirmed += OnTurnsConfirmed;
            _clientLockstep.PendingCommandAdded += OnPendingCommandAdded;
        }

        void RegisterHandlers()
        {
            _client.RegisterHandler(ConfirmTurnsMsgType, OnConfirmTurnsReceived);
            _client.RegisterHandler(SetLockstepConfigMsgType, OnSetLockstepConfigReceived);
            _client.RegisterHandler(AllClientsReadyMsgType, OnAllClientsReadyMsgTypeReceived);
        }

        void UnregisterHandlers()
        {
            _client.UnregisterHandler(ConfirmTurnsMsgType);
            _client.UnregisterHandler(SetLockstepConfigMsgType);
            _client.UnregisterHandler(AllClientsReadyMsgType);
        }

        void OnConfirmTurnsReceived(NetworkMessage netMsg)
        {
            ConfirmTurnsMessage turnsAction = new ConfirmTurnsMessage(_networkCommandDataFactory);
            turnsAction.Deserialize(netMsg.reader);
            _clientLockstep.ConfirmTurns(turnsAction.ConfirmedTurns);
        }

        void OnSetLockstepConfigReceived(NetworkMessage netMsg)
        {
            var msg = new SetLockstepConfigMessage();
            msg.Deserialize(netMsg.reader);
            _lockstepConfig = msg.Config;
            if(LockstepConfigReceived != null)
            {
                LockstepConfigReceived(msg.Config);
            }
        }

        public void SendClientReady()
        {
            _client.Send(ClientReadyMsgType, new EmptyMessage());
        }

        void OnAllClientsReadyMsgTypeReceived(NetworkMessage netMsg)
        {
            AllClientsReadyMessage msg = new AllClientsReadyMessage();
            msg.Deserialize(netMsg.reader);
            int remaining = msg.GetRemaningMillisecondsToStart(netMsg.conn.hostId, netMsg.conn.connectionId);
            _clientLockstep.Start(SocialPoint.Utils.TimeUtils.TimestampMilliseconds + (long)remaining);
        }

        void OnTurnsConfirmed(int[] turns)
        {
            var confirmTurnReception = new ConfirmTurnsReceptionMessage(turns);
            _client.Send(ConfirmTurnsReceptionMsgType, confirmTurnReception);
        }

        void OnPendingCommandAdded(ILockstepCommand command)
        {
            LockstepCommandMessage action = new LockstepCommandMessage(_networkCommandDataFactory, command);
            _client.Send(LockstepCommandMsgType, action);
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