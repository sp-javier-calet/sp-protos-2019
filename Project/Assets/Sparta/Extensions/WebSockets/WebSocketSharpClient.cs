using System;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public class WebSocketSharpClient : INetworkClient, IDisposable
    {
        readonly WebSocketSharp.WebSocket _socket;
        readonly WebSocketsEventDispatcher _dispatcher;

        public WebSocketSharpClient(string url, ICoroutineRunner runner)
        {
            _dispatcher = new WebSocketsEventDispatcher(runner);
            _socket = new WebSocketSharp.WebSocket(url);
            _socket.OnOpen += OnSocketOpened;
            _socket.OnClose += OnSocketClosed;
            _socket.OnError += OnSocketError;
            _socket.OnMessage += OnSocketMessage;
        }

        public void Dispose()
        {
            Disconnect();
            _socket.OnOpen -= OnSocketOpened;
            _socket.OnClose -= OnSocketClosed;
            _socket.OnError -= OnSocketError;
            _socket.OnMessage -= OnSocketMessage;
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
                return _socket.IsConnected;
            }
        }

        #endregion
    }
}