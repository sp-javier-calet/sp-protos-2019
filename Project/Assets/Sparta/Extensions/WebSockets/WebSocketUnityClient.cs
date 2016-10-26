using UnityEngine;
using System;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.WebSockets
{
    public class WebSocketUnityClient : INetworkClient, IDisposable
    {
        #region Static methods for Game Object delegate

        static int ClientIdCounter = 0;

        static WebSocketUnityObjectDelegate CreateDelegateObject(WebSocketUnityClient client)
        {
            var go = new GameObject("WebSocketUnityObjectDelegate_" + ClientIdCounter++);
            UnityEngine.Object.DontDestroyOnLoad(go);

            var del = go.AddComponent<WebSocketUnityObjectDelegate>();
            del.Init(client);
            return del;
        }

        static void DisposeDelegateObject(WebSocketUnityObjectDelegate del)
        {
            GameObject.Destroy(del.gameObject);
        }

        #endregion

        readonly WebSocketUnity _socket;
        readonly WebSocketsEventDispatcher _dispatcher;
        WebSocketUnityObjectDelegate _delegate;

        public WebSocketUnityClient(string url, ICoroutineRunner runner)
        {
            _dispatcher = new WebSocketsEventDispatcher(runner);
            _delegate = CreateDelegateObject(this);
            _socket = new WebSocketUnity(url, _delegate);
        }

        public void Dispose()
        {
            Disconnect();
            _dispatcher.Dispose();
            DisposeDelegateObject(_delegate);
        }

        public void SendNetworkMessage(NetworkMessageData info, string msg)
        {
            _socket.Send(msg);
        }

        protected byte[] Decode(string base64EncodedData)
        {
            return _socket.decodeBase64String(base64EncodedData);
        }

        #region INetworkClient implementation

        public void Connect()
        {
            _socket.Open();
        }

        public void Disconnect()
        {
            _socket.Close();
        }

        public INetworkMessage CreateMessage(NetworkMessageData data)
        {
            return new WebSocketUnityNetworkMessage(data, this);
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
                return _socket.IsOpened();
            }
        }

        #endregion

        class WebSocketUnityObjectDelegate : MonoBehaviour, WebSocketUnityDelegate
        {
            WebSocketUnityClient _client;

            public void Init(WebSocketUnityClient client)
            {
                _client = client;
            }

            #region WebSocketUnityDelegate implementation

            public void OnWebSocketUnityOpen(string sender)
            {
                _client._dispatcher.NotifyConnected();
            }

            public void OnWebSocketUnityClose(string reason)
            {
                _client._dispatcher.NotifyDisconnected();
            }

            public void OnWebSocketUnityReceiveMessage(string message)
            {
                _client._dispatcher.NotifyMessage(message);
            }

            public void OnWebSocketUnityReceiveDataOnMobile(string base64EncodedData)
            {
                byte[] decodedData = _client.Decode(base64EncodedData);
                OnWebSocketUnityReceiveData(decodedData);
            }

            public void OnWebSocketUnityReceiveData(byte[] data)
            {
                var message = System.Text.Encoding.UTF8.GetString(data);
                OnWebSocketUnityReceiveMessage(message);
            }

            public void OnWebSocketUnityError(string error)
            {
                _client._dispatcher.NotifyError(error);
            }

            #endregion
        }
    }
}
