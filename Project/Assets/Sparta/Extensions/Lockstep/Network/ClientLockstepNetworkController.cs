using System;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Base;

namespace SocialPoint.Lockstep.Network
{
    public sealed class ClientLockstepNetworkController : IDisposable, INetworkMessageReceiver, INetworkClientDelegate
    {
        INetworkClient _client;
        LockstepCommandFactory _commandFactory;
        ClientLockstepController _clientLockstep;
        INetworkMessageReceiver _receiver;

        bool _sendPlayerReadyPending;
        bool _clientSetupReceived;

        public int PlayerId{ get; private set; }

        public bool Running
        {
            get
            {
                return _client.Connected && _clientLockstep.Running;
            }
        }

        public event Action<int> StartScheduled;

        public ClientLockstepNetworkController(INetworkClient client)
        {
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public ClientLockstepNetworkController(INetworkClient client, ClientLockstepController clientLockstep, LockstepCommandFactory factory)
        {
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
            SetupClientLockstep(clientLockstep, factory);
        }

        [Obsolete("Use the constructor")]
        public void Init(ClientLockstepController clientLockstep, LockstepCommandFactory factory)
        {
            SetupClientLockstep(clientLockstep, factory);
        }

        public void SetupClientLockstep(ClientLockstepController clientLockstep, LockstepCommandFactory factory)
        {
            _clientLockstep = clientLockstep;
            _commandFactory = factory;
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

        public void OnNetworkError(Error err)
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
            _clientSetupReceived = true;
            if(_clientLockstep != null)
            {
                _clientLockstep.Config = msg.Config;
            }
            TrySendPlayerReady();
        }

        void OnAllPlayersReadyReceived(IReader reader)
        {
            var msg = new AllPlayersReadyMessage();
            msg.Deserialize(reader);
            int delay = msg.StartDelay - _client.GetDelay(msg.ServerTimestamp);
            if(delay < 0)
            {
                throw new InvalidOperationException("Should have already started lockstep.");
            }
            PlayerId = msg.PlayerId;
            _clientLockstep.Start(delay);
            if(StartScheduled != null)
            {
                StartScheduled(delay);
            }
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