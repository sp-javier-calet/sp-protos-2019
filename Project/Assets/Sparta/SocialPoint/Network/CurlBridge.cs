using System;
using System.Runtime.InteropServices;
using SocialPoint.Base;
using SocialPoint.Network;

namespace SocialPoint.Network
{
    public sealed class CurlBridge
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct RequestStruct
        {
            public int Id;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Url;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Query;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Method;
            public int Timeout;
            public int ActivityTimeout;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Proxy;
            [MarshalAs(UnmanagedType.LPStr)]
            public string Headers;
            [MarshalAs(UnmanagedType.LPArray)]
            public byte[] Body;
            public int BodyLength;
        };

        readonly UIntPtr _nativeClient;

        public CurlBridge(bool enableHttp2)
        {
            _nativeClient = SPUnityCurlCreate(enableHttp2);
        }

        public void SetConfig(string config)
        {
            SPUnityCurlSetConfig(_nativeClient, config);
        }

        public Connection CreateConnection()
        {
            var id = SPUnityCurlCreateConn(_nativeClient);
            return new Connection(this, id);
        }

        public void Dispose()
        {
            SPUnityCurlDestroy(_nativeClient);
        }

        public bool Pause
        {
            set
            {
                SPUnityCurlOnApplicationPause(_nativeClient, value);
            }
        }

        #region Inner Connection class

        public class Connection
        {
            readonly CurlBridge _curl;
            readonly int _connectionId;

            public Connection(CurlBridge curl, int connection)
            {
                _curl = curl;
                _connectionId = connection;
            }

            public int Update()
            {
                return SPUnityCurlUpdate(_curl._nativeClient, _connectionId);
            }

            public int Send(RequestStruct req)
            {
                return SPUnityCurlSend(_curl._nativeClient, req);
            }

            public int Id
            {
                get
                {
                    return _connectionId;
                }
            }

            public double ConnectTime
            {
                get
                {
                    return SPUnityCurlGetConnectTime(_curl._nativeClient, _connectionId);
                }
            }

            public double TotalTime
            {
                get
                {
                    return SPUnityCurlGetTotalTime(_curl._nativeClient, _connectionId);
                }
            }

            public double DownloadSize
            {
                get
                {
                    return SPUnityCurlGetDownloadSize(_curl._nativeClient, _connectionId);
                }
            }

            public double DownloadSpeed
            {
                get
                {
                    return SPUnityCurlGetDownloadSpeed(_curl._nativeClient, _connectionId);
                }
            }

            public int Code
            {
                get
                {   
                    return SPUnityCurlGetResponseCode(_curl._nativeClient, _connectionId);
                }
            }

            public string Headers
            {
                get
                {
                    var headers = String.Empty;
                    var headersLength = SPUnityCurlGetHeadersLength(_curl._nativeClient, _connectionId);
                    if(headersLength > 0)
                    {
                        var bytes = new byte[headersLength];
                        SPUnityCurlGetHeaders(_curl._nativeClient, _connectionId, bytes);
                        headers = System.Text.Encoding.ASCII.GetString(bytes);
                    }
                    return headers;
                }
            }

            public byte[] Body
            {
                get
                {
                    byte[] body = null;
                    var bodyLength = SPUnityCurlGetBodyLength(_curl._nativeClient, _connectionId);
                    if(bodyLength > 0)
                    {
                        body = new byte[bodyLength];
                        SPUnityCurlGetBody(_curl._nativeClient, _connectionId, body);
                    }
                    else
                    {
                        body = new byte[0];
                    }
                    return body;
                }
            }

            public int ErrorCode
            {
                get
                {
                    return SPUnityCurlGetErrorCode(_curl._nativeClient, _connectionId);
                }
            }

            public string Error
            {
                get
                {
                    int errorLength = CurlBridge.SPUnityCurlGetErrorLength(_curl._nativeClient, _connectionId);
                    var bytes = new byte[errorLength];
                    SPUnityCurlGetError(_curl._nativeClient, _connectionId, bytes);
                    var error = System.Text.Encoding.ASCII.GetString(bytes);
                    return error;
                }
            }

            public void Dispose()
            {
                SPUnityCurlDestroyConn(_curl._nativeClient, _connectionId);
            }
        }

        #endregion

        #region Native interface

        #if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        const string PluginModuleName = "SPUnityPlugins";
        #elif UNITY_ANDROID && !UNITY_EDITOR
        const string PluginModuleName = "sp_unity_curl";
        #elif (UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
        const string PluginModuleName = "__Internal";
        #elif UNITY_EDITOR_LINUX || UNITY_STANDALONE_LINUX
        const string PluginModuleName = "sp_unity_curl";
        #else
        const string PluginModuleName = "none";
        #endif

        [DllImport(PluginModuleName)]
        static extern UIntPtr SPUnityCurlCreate(bool enableHttp2);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlDestroy(UIntPtr client);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlCreateConn(UIntPtr client);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlDestroyConn(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlSend(UIntPtr client, RequestStruct data);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlUpdate(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetError(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetBody(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetHeaders(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetResponseCode(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetErrorCode(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetErrorLength(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetBodyLength(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetHeadersLength(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetConnectTime(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetTotalTime(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetDownloadSize(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetDownloadSpeed(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlOnApplicationPause(UIntPtr client, bool pause);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlSetConfig(UIntPtr client, string name);

        #endregion
    }
}
