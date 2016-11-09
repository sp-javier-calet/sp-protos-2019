using System;
using SocialPoint.Utils;
using SocialPoint.Network;

namespace SocialPoint.WebSockets
{
    public class WebSocketSharpClient : IWebSocketClient, IDisposable
    {
        readonly WebSocketsEventDispatcher _dispatcher;
        readonly string[] _protocols;
        WebSocketSharp.WebSocket _socket;

        public WebSocketSharpClient(string url, ICoroutineRunner runner) : this(url, null, runner)
        {
        }

        public WebSocketSharpClient(string url, string[] protocols, ICoroutineRunner runner)
        {
            _url = url;
            _protocols = protocols;
            _dispatcher = new WebSocketsEventDispatcher(runner);
            CreateSocket(url);
        }

        public void Dispose()
        {
            Disconnect();
            DestroySocket();
        }

        public void SendNetworkMessage(NetworkMessageData info, byte[] data)
        {
            _socket.SendAsync(data, null);
        }

        public void SendNetworkMessage(NetworkMessageData info, string data)
        {
            _socket.SendAsync(data, null);
        }

        void OnSocketOpened(object sender, EventArgs e)
        {
            _dispatcher.NotifyConnected();
        }

        void OnSocketMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            _dispatcher.NotifyMessage(e.Data);
        }

        void OnSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            var msg = e.Message;
            var baseException = e.Exception.GetBaseException();
            if(baseException != null)
            {
                msg = string.Format("{0}. Reason: {1}", e.Message, baseException.Message);
            }
            _dispatcher.NotifyError(msg);
        }

        void OnSocketClosed(object sender, WebSocketSharp.CloseEventArgs e)
        {
            _dispatcher.NotifyDisconnected();
        }

        void CreateSocket(string url)
        {
            if(_socket != null)
            {
                throw new InvalidOperationException("Socket already existing");
            }

            _socket = new WebSocketSharp.WebSocket(url, _protocols);
            _socket.OnOpen += OnSocketOpened;
            _socket.OnClose += OnSocketClosed;
            _socket.OnError += OnSocketError;
            _socket.OnMessage += OnSocketMessage;

            if(!string.IsNullOrEmpty(_proxy))
            {   
                _socket.SetProxy(_proxy, null, null);
            }
        }

        void DestroySocket()
        {
            if(_socket != null)
            {
                _socket.OnOpen -= OnSocketOpened;
                _socket.OnClose -= OnSocketClosed;
                _socket.OnError -= OnSocketError;
                _socket.OnMessage -= OnSocketMessage;
            }
        }

        #region WebsocketClient implementation

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
                    _socket.SetProxy(_proxy, null, null);
                }
            }
        }

        string _url;

        public string Url
        {
            get
            {
                return _url;
            }
            set
            {
                _url = value;
                DestroySocket();
                CreateSocket(value);
            }
        }

        public void Ping()
        {
            _socket.Ping();
        }

        #endregion

        #region INetworkClient implementation

        public void Connect()
        {
            _socket.ConnectAsync();
        }

        public void Disconnect()
        {
            _socket.CloseAsync();
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new WebSocketSharpNetworkMessage(data, this);
        }

        public void AddDelegate(INetworkClientDelegate dlg)
        {
            _dispatcher.OnClientConnected += dlg.OnClientConnected;
            _dispatcher.OnClientDisconnected += dlg.OnClientDisconnected;
            _dispatcher.OnMessageWasReceived += dlg.OnMessageReceived;
            _dispatcher.OnNetworkError += dlg.OnNetworkError;
        }

        public void RemoveDelegate(INetworkClientDelegate dlg)
        {
            _dispatcher.OnClientConnected -= dlg.OnClientConnected;
            _dispatcher.OnClientDisconnected -= dlg.OnClientDisconnected;
            _dispatcher.OnMessageWasReceived -= dlg.OnMessageReceived;
            _dispatcher.OnNetworkError -= dlg.OnNetworkError;
        }

        public void RegisterReceiver(INetworkMessageReceiver receiver)
        {
            _dispatcher.OnMessage += receiver.OnMessageReceived;
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
                return _socket.ReadyState == WebSocketSharp.WebSocketState.Open;
            }
        }

        #endregion
    }
}