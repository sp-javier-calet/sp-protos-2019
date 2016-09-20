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
        int _sendPlayerReadyPending;
        byte _unreliableChannel;
        byte _reliableChannel;
        bool _clientSetupReceived;
        INetworkMessageReceiver _receiver;
        byte[] _playerIds;

        public int PlayerCount
        {
            get
            {
                if(_playerIds == null)
                {
                    return 0;
                }
                return _playerIds.Length;
            }
        }

        public bool GetPlayerId(int position, out byte playerId)
        {
            if(_playerIds == null || _playerIds.Length <= position)
            {
                playerId = byte.MaxValue;
                return false;
            }
            playerId = _playerIds[position];
            return true;
        }

        public ClientLockstepNetworkController(INetworkClient client,
                                               byte unreliableChannel = 0,
                                               byte reliableChannel = 1)
        {
            _unreliableChannel = unreliableChannel;
            _reliableChannel = reliableChannel;
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
            _sendPlayerReadyPending = 0;
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
            case LockstepMsgType.AllClientsReady:
                OnAllClientsReadyReceived(reader);
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
            var turnsAction = new ConfirmTurnsMessage(_commandFactory);
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

        void OnAllClientsReadyReceived(IReader reader)
        {
            var msg = new AllPlayersReadyMessage();
            msg.Deserialize(reader);
            var delay = _client.GetDelay(msg.ServerTimestamp);
            int remaining = msg.RemainingMillisecondsToStart - delay;
            _playerIds = msg.PlayerIds;
            _clientLockstep.Start(TimeUtils.TimestampMilliseconds + (long)remaining);
        }

        public void SendPlayerReady()
        {
            _sendPlayerReadyPending++;
            TrySendPlayerReady();
        }

        void TrySendPlayerReady()
        {
            if(!_client.Connected || !_clientSetupReceived)
            {
                return;
            }
            for(; _sendPlayerReadyPending > 0; _sendPlayerReadyPending--)
            {
                _client.CreateMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.PlayerReady,
                    ChannelId = _reliableChannel
                }).Send();
            }
        }

        void OnTurnsConfirmed(int[] turns)
        {
            var confirmTurnReception = new ConfirmTurnsReceptionMessage(turns);
            _client.SendMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.ConfirmTurnsReception,
                ChannelId = _unreliableChannel
            }, confirmTurnReception);
        }

        void OnPendingCommandAdded(LockstepCommandData command)
        {
            command.ClientId = _client.ClientId;
            var msg = _client.CreateMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.LockstepCommand,
                ChannelId = _unreliableChannel
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