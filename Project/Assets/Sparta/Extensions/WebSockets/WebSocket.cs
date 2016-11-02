#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif 
#if UNITY_DEVICE || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
#define WEBSOCKET_SUPPORTED
#endif

using System;
using System.Runtime.InteropServices;

namespace SocialPoint.WebSockets
{
    /// <summary>
    /// Wrapper for the native WebSocket implementation
    /// </summary>
    public sealed class WebSocket : IDisposable
    {
        /// <summary>
        /// Static method to ask if Curl implementation is available in the current platform
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

        public WebSocket()
        {
            _nativeSocket = SPUnityWebSocketsCreate();
        }

        public void Update()
        {
            
        }
        
        public void Dispose()
        {
            SPUnityWebSocketDestroy(NativeSocket);
            _nativeSocket = default(UIntPtr);
        }

        #region Native interface

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

        #endregion
    }
}