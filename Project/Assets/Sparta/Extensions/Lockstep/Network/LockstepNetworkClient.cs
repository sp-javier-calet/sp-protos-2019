using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Lockstep
{
    public sealed class LockstepNetworkClient : IDisposable, INetworkMessageReceiver, INetworkClientDelegate
    {
        public INetworkClient Network { get; private set;  }
        public LockstepCommandFactory CommandFactory { get;  private set; }
        public LockstepClient Lockstep { get; private set; }

        INetworkMessageReceiver _receiver;

        bool _sendPlayerReadyPending;
        bool _clientSetupReceived;

        public bool Running
        {
            get
            {
                return Network.Connected && Lockstep.Running;
            }
        }

        public string PlayerId{ get; set; }

        public byte PlayerNumber
        {
            get
            {
                return Lockstep.PlayerNumber;
            }

            private set
            {
                Lockstep.PlayerNumber = value;
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
        public event Action<Attr> EndReceived;
        public event Action<Error> ErrorProduced;

        public LockstepNetworkClient(INetworkClient client, LockstepClient clientLockstep, LockstepCommandFactory factory)
        {
            PlayerId = RandomUtils.GenerateSecurityToken();
            Network = client;
            Lockstep = clientLockstep;
            CommandFactory = factory;

            Network.RegisterReceiver(this);
            Network.AddDelegate(this);
            Lockstep.CommandAdded += OnCommandAdded;
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
            case LockstepMsgType.ClientEnd:
                OnClientEndReceived(reader);
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
            Lockstep.AddConfirmedEmptyTurns(emptyTurns);
        }

        void OnTurnReceived(IReader reader)
        {
            var turn = new ClientTurnData();
            turn.Deserialize(CommandFactory, reader);
            Lockstep.AddConfirmedTurn(turn);
        }

        void OnClientSetupReceived(IReader reader)
        {
            var msg = new ClientSetupMessage();
            msg.Deserialize(reader);
            _clientSetupReceived = true;
            if(Lockstep != null)
            {
                Lockstep.Config = msg.Config;
                Lockstep.GameParams = msg.GameParams;
            }
            TrySendPlayerReady();
        }

        void OnClientStartReceived(IReader reader)
        {
            var msg = new ClientStartMessage();
            msg.Deserialize(reader);
            var time = msg.StartTime + Network.GetDelay(msg.ServerTimestamp);
            _playerIds = msg.PlayerIds;
            PlayerNumber =  (byte)_playerIds.IndexOf(PlayerId);

            Lockstep.Start(time);

            if(StartScheduled != null)
            {
                StartScheduled(time);
            }
        }

        void OnClientEndReceived(IReader reader)
        {
            var msg = new AttrMessage();
            msg.Deserialize(reader);
            if(EndReceived != null)
            {
                EndReceived(msg.Data);
            }
            Lockstep.Stop();
        }

        public void SendPlayerReady()
        {
            _sendPlayerReadyPending = true;
            TrySendPlayerReady();
        }

        void TrySendPlayerReady()
        {
            if(!Network.Connected || !_clientSetupReceived)
            {
                return;
            }
            if(_sendPlayerReadyPending)
            {
                _sendPlayerReadyPending = false;
                Network.SendMessage(new NetworkMessageData {
                    MessageType = LockstepMsgType.PlayerReady,
                }, new PlayerReadyMessage(PlayerId, Lockstep.CurrentTurnNumber));
              if(PlayerReadySent != null)
                {
                    PlayerReadySent();
                }
            }
        }

        public void SendPlayerFinish(Attr data)
        {
            if(!Lockstep.Running)
            {
                return;
            }
            Network.SendMessage(new NetworkMessageData{
                MessageType = LockstepMsgType.PlayerFinish
            }, new AttrMessage(data));
            if(PlayerFinishSent != null)
            {
                PlayerFinishSent(data);
            }
        }

        void OnCommandAdded(ClientCommandData command)
        {
            var msg = Network.CreateMessage(new NetworkMessageData {
                MessageType = LockstepMsgType.Command,
            });
            command.Serialize(CommandFactory, msg.Writer);
            msg.Send();
        }

        public void Dispose()
        {
            Network.RegisterReceiver(null);
            Network.RemoveDelegate(this);
            if(Lockstep != null)
            {
                Lockstep.CommandAdded -= OnCommandAdded;
            }
        }
    }
}