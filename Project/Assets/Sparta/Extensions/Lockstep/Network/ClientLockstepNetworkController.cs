using System;
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
            _clientLockstep.CommandAdded += OnCommandAdded;
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
            case LockstepMsgType.ConfirmTurn:
                OnConfirmTurnReceived(reader);
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

        void OnConfirmTurnReceived(IReader reader)
        {
            var turn = new ClientLockstepTurnData();
            turn.Deserialize(_commandFactory, reader);
            _clientLockstep.AddConfirmedTurn(turn);
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
            _clientLockstep.Start(remaining);
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
                }).Send();
            }
        }

        void OnCommandAdded(ClientLockstepCommandData command)
        {
            command.ClientId = _client.ClientId;
            var msg = _client.CreateMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.Command,
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
                _clientLockstep.CommandAdded -= OnCommandAdded;
            }
        }
    }
}