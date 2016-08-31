using System;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ServerLockstepNetworkController : IDisposable
    {
        class LockstepClientData
        {
            public int ConnectionId;
            public int ClientId;
            public bool IsReady;
        }

        int _clientsCount;
        LockstepCommandDataFactory _networkCommandDataFactory;
        ServerLockstepController _serverLockstep;
        int _startLockstepDelay;
        LockstepConfig _lockstepConfig;
        INetworkMessageController _messageController;

        public byte LockstepCommandMsgType { get; private set; }

        public byte ConfirmTurnsMsgType { get; private set; }

        public byte ConfirmTurnsReceptionMsgType { get; private set; }

        public byte SetLockstepConfigMsgType { get; private set; }

        public byte AllClientsReadyMsgType { get; private set; }

        public byte ClientReadyMsgType { get; private set; }

        Dictionary<int, LockstepClientData> _clientDataByConnectionId;
        Dictionary<int, LockstepClientData> _clientDataByClientId;

        public ServerLockstepNetworkController(INetworkMessageController messageController,
                                               int clientsCount,
                                               LockstepConfig lockstepConfig,
                                               int startLockstepDelay = 5000,
                                               byte lockstepCommandMsgType = 102,
                                               byte confirmTurnsMsgType = 103,
                                               byte confirmTurnsReceptionMsgType = 104,
                                               byte setLockstepConfigMsgType = 105,
                                               byte clientReadyMsgType = 106,
                                               byte allClientsReadyMsgType = 107)
        {
            _messageController = messageController;
            _clientsCount = clientsCount;
            _clientDataByConnectionId = new Dictionary<int, LockstepClientData>();
            _clientDataByClientId = new Dictionary<int, LockstepClientData>();
            _lockstepConfig = lockstepConfig;
            _startLockstepDelay = startLockstepDelay;
            LockstepCommandMsgType = lockstepCommandMsgType;
            ConfirmTurnsMsgType = confirmTurnsMsgType;
            ConfirmTurnsReceptionMsgType = confirmTurnsReceptionMsgType;
            SetLockstepConfigMsgType = setLockstepConfigMsgType;
            ClientReadyMsgType = clientReadyMsgType;
            AllClientsReadyMsgType = allClientsReadyMsgType;
        }

        public void Init(ServerLockstepController serverLockstep,
                         LockstepCommandDataFactory networkCommandDataFactory)
        {
            _serverLockstep = serverLockstep;
            _serverLockstep.CommandStep = _lockstepConfig.CommandStep;
            _serverLockstep.SendClientTurnData = SendClientTurnData;
            _networkCommandDataFactory = networkCommandDataFactory;
        }

        public void Start()
        {
            RegisterHandlers();
        }

        void SendClientTurnData(int clientId, LockstepTurnData[] turnData)
        {
            LockstepClientData data;
            if(_clientDataByClientId.TryGetValue(clientId, out data))
            {
                int connectionId = data.ConnectionId;
                var action = new ConfirmTurnsMessage(_networkCommandDataFactory);
                action.ConfirmedTurns = turnData;
                _messageController.Send(ConfirmTurnsMsgType, action, NetworkReliability.Unreliable, connectionId);
            }
        }

        void RegisterHandlers()
        {
            _messageController.RegisterHandler(ConfirmTurnsReceptionMsgType, OnConfirmTurnsReceptionReceived);
            _messageController.RegisterHandler(LockstepCommandMsgType, OnLockstepCommandReceived);
            _messageController.RegisterHandler(ClientReadyMsgType, OnClientReadyReceived);
        }

        void UnregisterHandlers()
        {
            _messageController.UnregisterHandler(ConfirmTurnsReceptionMsgType);
            _messageController.UnregisterHandler(LockstepCommandMsgType);
            _messageController.UnregisterHandler(ClientReadyMsgType);
        }

        void OnConfirmTurnsReceptionReceived(NetworkMessageData data)
        {
            var msg = new ConfirmTurnsReceptionMessage();
            int clientId = _clientDataByConnectionId[data.ConnectionId].ClientId;
            msg.Deserialize(data.Reader);
            for(int i = 0; i < msg.ConfirmedTurns.Length; ++i)
            {
                _serverLockstep.OnClientTurnReceptionConfirmed(clientId, msg.ConfirmedTurns[i]);
            }
        }

        void OnLockstepCommandReceived(NetworkMessageData data)
        {
            var msg = new LockstepCommandMessage(_networkCommandDataFactory);
            int clientId = _clientDataByConnectionId[data.ConnectionId].ClientId;
            msg.Deserialize(data.Reader);
            _serverLockstep.OnClientCommandReceived(clientId, msg.LockstepCommand);
        }

        void OnClientReadyReceived(NetworkMessageData data)
        {
            var clientData = _clientDataByConnectionId[data.ConnectionId];
            if(clientData != null && !clientData.IsReady)
            {
                clientData.IsReady = true;
                CheckAllClientsReady();
            }
        }

        void CheckAllClientsReady()
        {
            if(_clientDataByConnectionId.Count == _clientsCount)
            {
                var enumerator = _clientDataByConnectionId.GetEnumerator();
                bool allClientsReady = true;
                while(enumerator.MoveNext())
                {
                    if(!enumerator.Current.Value.IsReady)
                    {
                        allClientsReady = false;
                        break;
                    }
                }
                enumerator.Dispose();
                if(allClientsReady)
                {
                    StartLockstep();
                }
            }
        }

        void StartLockstep()
        {
            if(_serverLockstep != null)
            {
                AllClientsReadyMessage msg = new AllClientsReadyMessage(_startLockstepDelay);
                _messageController.SendToAll(AllClientsReadyMsgType, msg);
                _serverLockstep.Start(SocialPoint.Utils.TimeUtils.TimestampMilliseconds + _startLockstepDelay - _serverLockstep.CommandStep);
            }
        }

        public int OnClientConnected(int connectionId)
        {
            if(!_clientDataByConnectionId.ContainsKey(connectionId))
            {
                for(int i = 0; i < _clientsCount; ++i)
                {
                    if(!_clientDataByClientId.ContainsKey(i))
                    {
                        var clientData = new LockstepClientData() {
                            ClientId = i,
                            ConnectionId = connectionId
                        };

                        _clientDataByClientId[i] = _clientDataByConnectionId[connectionId] = clientData;
                        _messageController.Send(SetLockstepConfigMsgType,
                            new SetLockstepConfigMessage((byte)i, _lockstepConfig),
                            NetworkReliability.Reliable,
                            connectionId);
                        return i;
                    }
                }
            }
            return _clientDataByConnectionId[connectionId].ClientId;
        }

        public int OnClientDisconnected(int connectionId)
        {
            var clientId = _clientDataByConnectionId[connectionId].ClientId;
            _clientDataByClientId.Remove(clientId);
            _clientDataByConnectionId.Remove(connectionId);
            return clientId;
        }

        public void Stop()
        {
            UnregisterHandlers();
            if(_serverLockstep != null)
            {
                _serverLockstep.Stop();
            }
        }

        public void Dispose()
        {
            Stop();
            if(_serverLockstep != null)
            {
                _serverLockstep.SendClientTurnData = null;
                _serverLockstep.Dispose();
            }
        }
    }
}