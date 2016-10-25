using System;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class WebSocketSharpClient : IWebSocketClient, IDisposable
    {
        readonly WebSocketsEventDispatcher _dispatcher;
        WebSocketSharp.WebSocket _socket;

        public WebSocketSharpClient(string url, ICoroutineRunner runner)
        {
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
            _socket.Send(data);
        }

        void OnSocketOpened(object sender, EventArgs e)
        {
            _dispatcher.NotifyConnected();
        }

        void OnSocketMessage(object sender, WebSocketSharp.MessageEventArgs e)
        {
            _dispatcher.NotifyMessage(e.RawData);
        }

        void OnSocketError(object sender, WebSocketSharp.ErrorEventArgs e)
        {
            _dispatcher.NotifyError(e.Message);
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

            _socket = new WebSocketSharp.WebSocket(url);
            _socket.OnOpen += OnSocketOpened;
            _socket.OnClose += OnSocketClosed;
            _socket.OnError += OnSocketError;
            _socket.OnMessage += OnSocketMessage;
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
            _socket = null;
        }

        #region WebsocketClient implementation

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
            _socket.Connect();
        }

        public void Disconnect()
        {
            _socket.Close();
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