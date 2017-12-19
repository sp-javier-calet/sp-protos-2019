
#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif 
#if UNITY_DEVICE || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
#define WEBSOCKET_SUPPORTED
#endif

using System;
using System.Runtime.InteropServices;
using SocialPoint.Base;

namespace SocialPoint.WebSockets
{
    /// <summary>
    /// Wrapper for the native WebSocket implementation
    /// </summary>
    public sealed class WebSocket : IDisposable
    {
        /// <summary>
        /// Static method to ask if WebSocket native implementation is available in the current platform
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                #if WEBSOCKET_SUPPORTED
                return true;
                #else
                return false;
                #endif
            }
        }


        public event Action<bool> ConnectionStateChanged;

        public event Action<string> MessageReceived;

        public event Action<Error> ConnectionError;

        WebSocketState _lastState;

        string[] _urls;

        WebSocketState State
        {
            get
            {
                return (WebSocketState)SPUnityWebSocketGetState(NativeSocket);
            }
        }

        UIntPtr _nativeSocket;

        UIntPtr NativeSocket
        {
            get
            {
                if(!IsValidInstance)
                {
                    throw new NullReferenceException("Native object reference not set or already disposed");
                }

                return _nativeSocket;
            }
        }

        public bool IsValidInstance
        {
            get
            {
                return _nativeSocket != default(UIntPtr);
            }
        }

        public WebSocket(string[] urls, string[] protocols)
        {
            _nativeSocket = SPUnityWebSocketsCreate();
            _lastState = WebSocketState.Closed;
            _urls = urls;

            DebugUtils.Assert(_urls != null && _urls.Length > 0, "Provided urls shoudl have one element at least");

            for(var i = 0; i < urls.Length; ++i)
            {
                var uri = new Uri(urls[i]);
                SPUnityWebSocketAddUrl(NativeSocket, uri.Scheme, uri.Host, uri.PathAndQuery, uri.Port);
            }

            if(protocols != null)
            {
                for(var i = 0; i < protocols.Length; ++i)
                {
                    SPUnityWebSocketAddProtocol(NativeSocket, protocols[i]);
                }
            }
        }

        public void Connect()
        {
            SPUnityWebSocketConnect(NativeSocket);
        }

        public void Disconnect()
        {
            SPUnityWebSocketDisconnect(NativeSocket);
        }

        public void Ping()
        {
            SPUnityWebSocketPing(NativeSocket);
        }

        public void OnWillGoBackground()
        {
            SPUnityWebSocketOnWillGoBackground(NativeSocket);
            CheckStateChanges();
        }

        public void OnWasOnBackground()
        {
            SPUnityWebSocketOnWasOnBackground(NativeSocket);
            CheckStateChanges();
        }

        public void Update()
        {
            // Update service
            SPUnityWebSocketUpdate(NativeSocket);

            // Changed the order of the processing of the message and the change of state because
            // it was very common that the connection would close (sending a callback) before the
            // Message was processed. The listener would not receive the expected message or (if it
            // did not unregister) would receive it after the disconnection response was received.

            // Check incoming messages
            var msg = NextMessage();
            if(!string.IsNullOrEmpty(msg) && MessageReceived != null)
            {
                MessageReceived(msg);
            }

            // Update Connection state
            CheckStateChanges();

            // Check error
            var err = Error;
            if(!Error.IsNullOrEmpty(err) && ConnectionError != null)
            {
                ConnectionError(err);
            }   
        }

        public void Send(string message)
        {
            SPUnityWebSocketSend(NativeSocket, message);
        }

        public void Dispose()
        {
            SPUnityWebSocketDestroy(NativeSocket);
            _nativeSocket = default(UIntPtr);
        }

        public bool IsConnected
        {
            get
            {
                return SPUnityWebSocketIsConnected(NativeSocket);
            }
        }

        public bool IsConnecting
        {
            get
            {
                return SPUnityWebSocketIsConnecting(NativeSocket);
            }
        }

        public bool InStandby
        {
            get
            {
                return SPUnityWebSocketInStandby(NativeSocket);
            }
        }

        public bool Verbose
        {
            set
            {
                SPUnityWebSocketSetVerbose(value);
            }
        }

        public string ConnectedUrl
        {
            get
            {
                var index = SPUnityWebSocketGetConnectedUrlIndex(NativeSocket);
                return (index < 0 || index >= _urls.Length) ? null : _urls[index];
            }
        }

        public string Proxy
        {
            set
            {
                if(!string.IsNullOrEmpty(value))
                {
                    var uri = new Uri(value);
                    SPUnityWebSocketSetProxy(uri.Host, uri.Port);
                }
                else
                {
                    SPUnityWebSocketSetProxy(string.Empty, 0);
                }
            }
        }

        void CheckStateChanges()
        {
            var newState = State;
            if(newState != _lastState)
            {
                _lastState = newState;
                NotifyConnectionState(newState);
            }
        }

        void NotifyConnectionState(WebSocketState state)
        {
            if(ConnectionStateChanged != null)
            {
                switch(state)
                {
                case WebSocketState.Open:
                    ConnectionStateChanged(true);
                    break;
                case WebSocketState.Closed:
                    ConnectionStateChanged(false);
                    break;
                }
            }
        }

        string NextMessage()
        {
            var message = String.Empty;
            var msgLength = SPUnityWebSocketGetMessageLength(NativeSocket);
            if(msgLength > 0)
            {
                var bytes = new byte[msgLength];
                SPUnityWebSocketGetMessage(NativeSocket, bytes);
                message = System.Text.Encoding.UTF8.GetString(bytes);
            }
            return message;
        }

        int ErrorCode
        {
            get
            {
                return SPUnityWebSocketGetErrorCode(NativeSocket);
            }
        }

        string ErrorMessage
        {
            get
            {
                string error = string.Empty;
                int errorLength = SPUnityWebSocketGetErrorLenght(NativeSocket);
                if(errorLength > 0)
                {
                    var bytes = new byte[errorLength];
                    SPUnityWebSocketGetError(NativeSocket, bytes);
                    error = System.Text.Encoding.UTF8.GetString(bytes);
                }
                return error;
            }
        }

        Error Error
        {
            get
            {
                Error error = null;
                var code = ErrorCode;
                if(code != 0)
                {
                    error = new Error(code, ErrorMessage);
                }
                return error;
            }
        }

        #region Native interface

        /// <summary>
        /// Web socket state. Tied to the native WebSocketConnection implementation
        /// </summary>
        enum WebSocketState
        {
            Closed,
            Closing,
            Connecting,
            Open
        }

        /// <summary>
        /// Web socket errors. Tied to the native WebSocketConnection implementation
        /// </summary>
        enum WebSocketError
        {
            None = 0,
            WriteError,
            StreamError,
            ConnectionError,
            MaxPings
        }

        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID && !UNITY_EDITOR
        const string PluginModuleName = "sp_unity_websockets";
        #elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        const string PluginModuleName = "__Internal";
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        const string PluginModuleName = "sp_unity_websockets";
        #else
        const string PluginModuleName = "none";
        #endif

        [DllImport(PluginModuleName)]
        static extern UIntPtr SPUnityWebSocketsCreate();

        [DllImport(PluginModuleName)]
        static extern UIntPtr SPUnityWebSocketDestroy(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern int SPUnityWebSocketGetConnectedUrlIndex(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketAddUrl(UIntPtr socket, string shceme, string host, string path, int port);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketAddProtocol(UIntPtr socket, string protocol);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketConnect(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketIsConnected(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketIsConnecting(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketInStandby(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketSetStandbyTimeout(UIntPtr socket, int timeout_seconds);

        [DllImport(PluginModuleName)]
        static extern int SPUnityWebSocketGetState(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketDisconnect(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketOnWillGoBackground(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketOnWasOnBackground(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketUpdate(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketPing(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketSend(UIntPtr socket, string data);

        [DllImport(PluginModuleName)]
        static extern int SPUnityWebSocketGetMessageLength(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketGetMessage(UIntPtr socket, byte[] data);

        [DllImport(PluginModuleName)]
        static extern int SPUnityWebSocketGetErrorLenght(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern int SPUnityWebSocketGetErrorCode(UIntPtr socket);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketGetError(UIntPtr socket, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityWebSocketSetProxy(string proxy, int port);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityWebSocketSetVerbose(bool verbose);

        #endregion
    }
}
