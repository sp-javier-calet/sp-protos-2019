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
        uint _playerHash;

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
            _playerHash = RandomUtils.GenerateUint();
            _client = client;
            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
        }

        public ClientLockstepNetworkController(INetworkClient client, ClientLockstepController clientLockstep, LockstepCommandFactory factory):this(client)
        {
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
            _clientLockstep.Stop();
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
            case LockstepMsgType.Turn:
                OnTurnReceived(reader);
                break;
            case LockstepMsgType.ClientSetup:
                OnClientSetupReceived(reader);
                break;
            case LockstepMsgType.ClientStart:
                OnClientStartReceived(reader);
                break;
            default:
                if(_receiver != null)
                {
                    _receiver.OnMessageReceived(data, reader);
                }
                break;
            }
        }

        void OnTurnReceived(IReader reader)
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

        void OnClientStartReceived(IReader reader)
        {
            var msg = new ClientStartMessage();
            msg.Deserialize(reader);
            var time = msg.StartTime - _client.GetDelay(msg.ServerTimestamp);
            PlayerId = msg.PlayerId;
            _clientLockstep.Start(time);
            if(StartScheduled != null)
            {
                StartScheduled(time);
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
                _client.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.PlayerReady,
                }, new PlayerReadyMessage(_playerHash));
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