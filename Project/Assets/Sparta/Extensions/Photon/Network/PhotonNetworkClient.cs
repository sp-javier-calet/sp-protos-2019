using System;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.IO;

namespace SocialPoint.Network
{
    public class PhotonNetworkClient : PhotonNetworkBase, INetworkClient
    {
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

        protected override void OnConnected()
        {
            if(!string.IsNullOrEmpty(BackendEnv))
            {
                var options = new RaiseEventOptions();
                options.Receivers = ReceiverGroup.Others;

                if(!string.IsNullOrEmpty(BackendEnv))
                {
                    PhotonNetwork.RaiseEvent(PhotonMsgType.BackendEnv, BackendEnv, true, options);
                }
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnClientConnected();
            }
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
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

        public bool LatencySupported
        {
            get
            {
                return true;
            }
        }

        public int Latency
        {
            get
            {
                return PhotonNetwork.GetPing();
            }
        }
    }
}
