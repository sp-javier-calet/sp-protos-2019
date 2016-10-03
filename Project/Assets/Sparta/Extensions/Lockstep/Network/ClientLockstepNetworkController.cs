﻿using System;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkClientDelegate
    {
        INetworkClient _client;
        LockstepCommandFactory _commandFactory;
        ClientLockstepController _clientLockstep;
        LockstepConfig _lockstepConfig;
        bool _sendPlayerReadyPending;
        bool _clientSetupReceived;
        INetworkMessageReceiver _receiver;

        public int PlayerId{ get; private set; }

        public bool Running
        {
            get
            {
                return _client.Connected && _clientLockstep.Running;
            }
        }

        public ClientLockstepNetworkController(INetworkClient client)
        {
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public void Init(ClientLockstepController clientLockstep, LockstepCommandFactory factory)
        {
            _clientLockstep = clientLockstep;
            _commandFactory = factory;
            if(_lockstepConfig != null)
            {
                _clientLockstep.Init(_lockstepConfig);
            }

            _clientLockstep.TurnsConfirmed += OnTurnsConfirmed;
            _clientLockstep.PendingCommandAdded += OnPendingCommandAdded;
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public void OnClientConnected()
        {
            _clientSetupReceived = false;
        }

        public void OnClientDisconnected()
        {
            _clientSetupReceived = false;
        }

        public void OnMessageReceived(NetworkMessageData data)
        {
        }

        public void OnNetworkError(SocialPoint.Base.Error err)
        {
        }

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch(data.MessageType)
            {
            case LockstepMsgType.ConfirmTurns:
                OnConfirmTurnsReceived(reader);
                break;
            case LockstepMsgType.ClientSetup:
                OnClientSetupReceived(reader);
                break;
            case LockstepMsgType.AllPlayersReady:
                OnAllPlayersReadyReceived(reader);
                break;
            default:
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
                break;
            }
        }

        void OnConfirmTurnsReceived(IReader reader)
        {
            var turnsAction = new ClientConfirmTurnsMessage(_commandFactory);
            turnsAction.Deserialize(reader);
            _clientLockstep.ConfirmTurns(turnsAction.ConfirmedTurns);
        }

        void OnClientSetupReceived(IReader reader)
        {
            var msg = new ClientSetupMessage();
            msg.Deserialize(reader);
            _lockstepConfig = msg.Config;
            _clientSetupReceived = true;
            if(_clientLockstep != null)
            {
                _clientLockstep.Init(_lockstepConfig);
            }
            TrySendPlayerReady();
        }

        void OnAllPlayersReadyReceived(IReader reader)
        {
            var msg = new AllPlayersReadyMessage();
            msg.Deserialize(reader);
            var delay = _client.GetDelay(msg.ServerTimestamp);
            int remaining = msg.StartDelay - delay;
            if(remaining < 0)
            {
                throw new InvalidOperationException("Should have already started lockstep.");
            }
            PlayerId = msg.PlayerId;
            _clientLockstep.Start(TimeUtils.TimestampMilliseconds + (long)remaining);
        }

        public void SendPlayerReady()
        {
            _sendPlayerReadyPending = true;
            TrySendPlayerReady();
        }

        void TrySendPlayerReady()
        {
            if(!_client.Connected || !_clientSetupReceived)
            {
                return;
            }
            if(_sendPlayerReadyPending)
            {
                _sendPlayerReadyPending = false;
                _client.CreateMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.PlayerReady,
                    Unreliable = false
                }).Send();
            }
        }

        void OnTurnsConfirmed(int[] turns)
        {
            var confirmTurnReception = new ConfirmTurnsReceptionMessage(turns);
            _client.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ConfirmTurnsReception,
                Unreliable = true
            }, confirmTurnReception);
        }

        void OnPendingCommandAdded(ClientLockstepCommandData command)
        {
            command.ClientId = _client.ClientId;
            var msg = _client.CreateMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.LockstepCommand,
                Unreliable = true
            });
            command.Serialize(_commandFactory, msg.Writer);
            msg.Send();
        }

        public void Dispose()
        {
            _client.RegisterReceiver(null);
            _client.RemoveDelegate(this);
            if(_clientLockstep != null)
            {
                _clientLockstep.TurnsConfirmed -= OnTurnsConfirmed;
                _clientLockstep.PendingCommandAdded -= OnPendingCommandAdded;
            }
        }
    }
}