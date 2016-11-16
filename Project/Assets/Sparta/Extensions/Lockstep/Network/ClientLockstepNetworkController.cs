using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Network;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.Attributes;

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

        public bool Running
        {
            get
            {
                return _client.Connected && _clientLockstep.Running;
            }
        }

        public string PlayerId{ get; set; }

        public byte PlayerNumber
        {
            get
            {
                return _clientLockstep.PlayerNumber;
            }

            private set
            {
                _clientLockstep.PlayerNumber = value;
            }
        }

        List<string> _playerIds;
        public ReadOnlyCollection<string> PlayerIds
        {
            get
            {
                return _playerIds.AsReadOnly();
            }
        }

        public event Action<int> StartScheduled;
        public event Action PlayerReadySent;
        public event Action<Attr> PlayerFinishSent;
        public event Action<Error> ErrorProduced;

        public ClientLockstepNetworkController(INetworkClient client, ClientLockstepController clientLockstep, LockstepCommandFactory factory)
        {
            PlayerId = RandomUtils.GenerateSecurityToken();
            _client = client;
            _clientLockstep = clientLockstep;
            _commandFactory = factory;

            _client.RegisterReceiver(this);
            _client.AddDelegate(this);
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
            if(ErrorProduced != null)
            {
                ErrorProduced(err);
            }
        }

        public void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            switch(data.MessageType)
            {
            case LockstepMsgType.EmptyTurns:
                OnEmptyTurnsReceived(reader);
                break;
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

        void OnEmptyTurnsReceived(IReader reader)
        {
            var emptyTurns = new EmptyTurnsMessage();
            emptyTurns.Deserialize(reader);
            _clientLockstep.AddConfirmedEmptyTurns(emptyTurns);
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
                _clientLockstep.GameParams = msg.GameParams;
            }
            TrySendPlayerReady();
        }

        void OnClientStartReceived(IReader reader)
        {
            var msg = new ClientStartMessage();
            msg.Deserialize(reader);
            var time = msg.StartTime + _client.GetDelay(msg.ServerTimestamp);
            _playerIds = msg.PlayerIds;
            PlayerNumber =  (byte)_playerIds.IndexOf(PlayerId);

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
                }, new PlayerReadyMessage(PlayerId));
                if(PlayerReadySent != null)
                {
                    PlayerReadySent();
                }
            }
        }

        public void SendPlayerFinish(Attr data)
        {
            if(!_clientLockstep.Running)
            {
                return;
            }
            _client.SendMessage(new NetworkMessageData{
                MessageType = LockstepMsgType.PlayerFinish
            }, new PlayerFinishedMessage(data));
            if(PlayerFinishSent != null)
            {
                PlayerFinishSent(data);
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