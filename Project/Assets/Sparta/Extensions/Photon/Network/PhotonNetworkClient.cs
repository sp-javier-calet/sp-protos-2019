using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;
using System.IO;

namespace SocialPoint.Network
{
    public class PhotonNetworkClient : PhotonNetworkBase, INetworkClient
    {
        public PhotonNetworkServer LocalPhotonServer;

        public bool HasLocalPhotonServer
        {
            get
            {
                return LocalPhotonServer != null;
            }
        }

        public byte ClientId
        {
            get
            {
                return GetClientId(PhotonNetwork.player);
            }
        }

        public bool Connected
        {
            get
            {
                return State == ConnState.Connected;
            }
        }

        public string BackendEnv;

        List<INetworkClientDelegate> _delegates = new List<INetworkClientDelegate>();
        INetworkMessageReceiver _receiver;

        public void Connect()
        {
            DoConnect();
        }

        public void Disconnect()
        {
            DoDisconnect();
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _receiver = receiver;
        }

        public int GetDelay(int networkTimestamp)
        {
            return PhotonNetwork.ServerTimestamp - networkTimestamp;
        }

        protected override void DoConnect()
        {
            if(HasLocalPhotonServer)
            {
                _state = ConnState.Connecting;

                OnJoinedRoom();
                LocalPhotonServer.OnPhotonPlayerConnected(PhotonNetwork.player);
            }
            else
            {
                base.DoConnect();
            }
        }

        protected override void OnConnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        protected override void OnDisconnected()
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientDisconnected();
            }
        }

        protected override void OnNetworkError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnNetworkError(err);
            }
        }

        protected override void OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMessageReceived(data);
            }
            if(_receiver != null)
            {
                _receiver.OnMessageReceived(data, reader);
            }
        }

        public override void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            var cdata = HttpEncoding.Encode(data, HttpEncoding.DefaultBodyCompression);
            
            var options = new RaiseEventOptions();
            var serverId = PhotonNetworkServer.PhotonPlayerId;
            options.TargetActors = new int[]{ serverId };

            var reliable = PhotonNetwork.PhotonServerSettings.Protocol == ExitGames.Client.Photon.ConnectionProtocol.Tcp && !info.Unreliable;
            PhotonNetwork.RaiseEvent(info.MessageType, cdata, reliable, options);
            Config.CustomPhotonConfig.RegisterOnGoingCommand();
        }

        protected override void ProcessOnEventReceived(byte eventcode, object content, int senderid)
        {
            var cdata = HttpEncoding.Decode((byte[])content, HttpEncoding.DefaultBodyCompression);

            byte clientId = 0;
            var serverId = PhotonNetworkServer.PhotonPlayerId;
            if(senderid != serverId)
            {
                clientId = GetClientId(GetPlayer((byte)senderid));
            }
            var info = new NetworkMessageData {
                MessageType = eventcode,
                ClientId = clientId
            };
            var stream = new MemoryStream(cdata);
            var reader = new SystemBinaryReader(stream);

            OnMessageReceived(info, reader);
        }
    }
}
