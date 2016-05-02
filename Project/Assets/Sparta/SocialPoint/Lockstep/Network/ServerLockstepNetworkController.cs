using System;
using UnityEngine.Networking;
using SocialPoint.Utils;
using System.Collections.Generic;

namespace SocialPoint.Lockstep.Network
{
    public class ServerLockstepNetworkController : IDisposable
    {
        class LockstepClientData
        {
            public int ConnectionId;
            public int ClientId;
            public bool IsReady;
        }

        int _clientsCount;
        NetworkLockstepCommandDataFactory _networkCommandDataFactory;
        ServerLockstepController _serverLockstep;
        int _startLockstepDelay;
        LockstepConfig _lockstepConfig;

        public short LockstepCommandMsgType { get; protected set; }

        public short ConfirmTurnsMsgType { get; protected set; }

        public short ConfirmTurnsReceptionMsgType { get; protected set; }

        public short SetLockstepConfigMsgType { get; protected set; }

        public short AllClientsReadyMsgType { get; protected set; }

        public short ClientReadyMsgType { get; protected set; }

        Dictionary<int, LockstepClientData> _clientDataByConnectionId;
        Dictionary<int, LockstepClientData> _clientDataByClientId;

        public ServerLockstepNetworkController(int clientsCount,
                                               LockstepConfig lockstepConfig,
                                               int startLockstepDelay = 5000,
                                               short lockstepCommandMsgType = 2002,
                                               short confirmTurnsMsgType = 2003,
                                               short confirmTurnsReceptionMsgType = 2004,
                                               short setLockstepConfigMsgType = 2005,
                                               short clientReadyMsgType = 2006,
                                               short allClientsReadyMsgType = 2007)
        {
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
                         NetworkLockstepCommandDataFactory networkCommandDataFactory)
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
                NetworkServer.SendToClient(connectionId, ConfirmTurnsMsgType, action);
            }
        }

        void RegisterHandlers()
        {
            NetworkServer.RegisterHandler(ConfirmTurnsReceptionMsgType, OnConfirmTurnsReceptionReceived);
            NetworkServer.RegisterHandler(LockstepCommandMsgType, OnLockstepCommandReceived);
            NetworkServer.RegisterHandler(ClientReadyMsgType, OnClientReadyReceived);
        }

        void UnregisterHandlers()
        {
            NetworkServer.UnregisterHandler(ConfirmTurnsReceptionMsgType);
            NetworkServer.UnregisterHandler(LockstepCommandMsgType);
            NetworkServer.UnregisterHandler(ClientReadyMsgType);
        }

        void OnConfirmTurnsReceptionReceived(NetworkMessage netMsg)
        {
            var msg = new ConfirmTurnsReceptionMessage();
            int clientId = _clientDataByConnectionId[netMsg.conn.connectionId].ClientId;
            msg.Deserialize(netMsg.reader);
            for(int i = 0; i < msg.ConfirmedTurns.Length; ++i)
            {
                _serverLockstep.OnClientTurnReceptionConfirmed(clientId, msg.ConfirmedTurns[i]);
            }
        }

        void OnLockstepCommandReceived(NetworkMessage netMsg)
        {
            var msg = new LockstepCommandMessage(_networkCommandDataFactory);
            int clientId = _clientDataByConnectionId[netMsg.conn.connectionId].ClientId;
            msg.Deserialize(netMsg.reader);
            _serverLockstep.OnClientCommandReceived(clientId, msg.LockstepCommand);
        }

        void OnClientReadyReceived(NetworkMessage netMsg)
        {
            var clientData = _clientDataByConnectionId[netMsg.conn.connectionId];
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
                NetworkServer.SendToAll(AllClientsReadyMsgType, msg);
                _serverLockstep.Start(SocialPoint.Utils.TimeUtils.TimestampMilliseconds + _startLockstepDelay - _serverLockstep.CommandStep);
            }
        }

        public void OnClientConnected(int connectionId)
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
                        NetworkServer.SendToClient(connectionId,
                            SetLockstepConfigMsgType,
                            new SetLockstepConfigMessage(_lockstepConfig));
                        return;
                    }
                }
            }
        }

        public void OnClientDisconnected(int connectionId)
        {
            var clientId = _clientDataByConnectionId[connectionId].ClientId;
            _clientDataByClientId.Remove(clientId);
            _clientDataByConnectionId.Remove(connectionId);
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