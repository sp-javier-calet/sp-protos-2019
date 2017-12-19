using System;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class WebSocketClient : IWebSocketClient, IUpdateable, IDisposable
    {
        event Action ClientConnected;
        event Action ClientDisconnected;
        event Action<NetworkMessageData> MessageWasReceived;
        event Action<Error> NetworkError;
        event Action<NetworkMessageData, IReader> MessageReceived;

        readonly IUpdateScheduler _scheduler;
        readonly string[] _protocols;
        WebSocket _socket;

        public WebSocketClient(string[] url, IUpdateScheduler scheduler) : this(url, null, scheduler)
        {
        }

        public WebSocketClient(string[] urls, string[] protocols, IUpdateScheduler scheduler)
        {
            _urls = urls;
            _protocols = protocols;
            _scheduler = scheduler;
            CreateSocket(urls);
        }

        public void Update()
        {
            _socket.Update();
        }

        public void Dispose()
        {
            Disconnect();
            DestroySocket();
        }

        public void SendNetworkMessage(NetworkMessageData info, string data)
        {
            if(_socket != null && _socket.IsConnected)
            {
                _socket.Send(data);
            }
            else
            {
                DebugUtils.Assert(false, "Message could not be sent. Socket is not connected");
            }
        }

        void OnSocketStateChanged(bool connected)
        {
            if(connected)
            {
                if(ClientConnected != null)
                {
                    ClientConnected();
                }
            }
            else
            {
                if(ClientDisconnected != null)
                {
                    ClientDisconnected();
                }
            }
        }

        void OnSocketMessage(string message)
        {
            var data = new NetworkMessageData();
            if(MessageReceived != null)
            {
                MessageReceived(data, new WebSocketsTextReader(message));
            }
            if(MessageWasReceived != null)
            {
                MessageWasReceived(data);
            }
        }

        void OnSocketError(Error err)
        {
            if(NetworkError != null)
            {
                NetworkError(err);
            }
        }

        void CreateSocket(string[] urls)
        {
            if(_socket != null)
            {
                throw new InvalidOperationException("Socket already existing");
            }

            _socket = new WebSocket(urls, _protocols);
            _socket.ConnectionStateChanged += OnSocketStateChanged;
            _socket.ConnectionError += OnSocketError;
            _socket.MessageReceived += OnSocketMessage;

            if(!string.IsNullOrEmpty(_proxy))
            {   
                _socket.Proxy = _proxy;
            }
        }

        void DestroySocket()
        {
            if(_socket != null)
            {
                _socket.ConnectionStateChanged -= OnSocketStateChanged;
                _socket.ConnectionError -= OnSocketError;
                _socket.MessageReceived -= OnSocketMessage;
                _socket.Dispose();
            }
            _socket = null;
        }

        #region IWebSocketClient implementation

        public string ConnectedUrl
        {
            get
            {
                return _socket.ConnectedUrl;
            }
        }

        string[] _urls;

        public string[] Urls
        {
            get
            {
                return _urls;
            }
            set
            {
                _urls = value;
                DestroySocket();
                CreateSocket(value);
            }
        }

        string _proxy;

        public string Proxy
        {
            get
            {
                return _proxy;
            }
            set
            {
                _proxy = value;
                if(_socket != null)
                {
                    _socket.Proxy = _proxy;
                }
            }
        }

        public void Ping()
        {
            _socket.Ping();
        }

        public void OnWillGoBackground()
        {
            _socket.OnWillGoBackground();
        }

        public void OnWasOnBackground()
        {
            _socket.OnWasOnBackground();
        }

        #endregion

        #region INetworkClient implementation

        public void Connect()
        {
            _scheduler.Add(this);
            _socket.Connect();
        }

        public void Disconnect()
        {
            _socket.Disconnect();
            _scheduler.Remove(this);
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new WebSocketNetworkMessage(data, this);
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            ClientConnected += dlg.OnClientConnected;
            ClientDisconnected += dlg.OnClientDisconnected;
            MessageWasReceived += dlg.OnMessageReceived;
            NetworkError += dlg.OnNetworkError;

        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            ClientConnected -= dlg.OnClientConnected;
            ClientDisconnected -= dlg.OnClientDisconnected;
            MessageWasReceived -= dlg.OnMessageReceived;
            NetworkError -= dlg.OnNetworkError;
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            MessageReceived += receiver.OnMessageReceived;
        }

        public int GetDelay(int networkTimestamp)
        {
            return 0;
        }

        public byte ClientId
        {
            get
            {
                return 0;
            }
        }

        public bool Connected
        {
            get
            {
                return _socket.IsConnected;
            }
        }

        public bool Connecting
        {
            get
            {
                return _socket.IsConnecting;
            }
        }

        public bool InStandby
        {
            get
            {
                return _socket.InStandby;
            }
        }

        public bool LatencySupported
        {
            get
            {
                return false;
            }
        }

        public int Latency
        {
            get
            {
                DebugUtils.Assert(LatencySupported);
                return -1;
            }
        }

        #endregion
    }
}
