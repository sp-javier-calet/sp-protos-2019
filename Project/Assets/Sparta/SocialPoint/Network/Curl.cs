#if (UNITY_ANDROID || UNITY_IOS || UNITY_TVOS) && !UNITY_EDITOR
#define UNITY_DEVICE
#endif 
#if UNITY_DEVICE || UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX || UNITY_STANDALONE_LINUX || UNITY_EDITOR_LINUX
#define CURL_SUPPORTED
#endif


using System;
using System.Runtime.InteropServices;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    /// <summary>
    /// Wrapper for the native curl implementation
    /// </summary>
    public sealed class Curl : IDisposable
    {

        /// <summary>
        /// Static method to ask if Curl implementation is available in the current platform
        /// </summary>
        public const bool IsSupported
        #if CURL_SUPPORTED
        = true;
        #else
        = false;
        #endif

        UIntPtr _nativeClient;

        UIntPtr NativeClient
        {
            get
            {
                if(!IsValidInstance)
                {
                    throw new NullReferenceException("Native object reference not set or already disposed");
                }

                return _nativeClient;
            }
        }

        public bool IsValidInstance
        {
            get
            {
                return _nativeClient != default(UIntPtr);
            }
        }

        public Curl(bool enableHttp2)
        {
            _nativeClient = SPUnityCurlCreate(enableHttp2);
        }

        public void SetConfig(string config)
        {
            SPUnityCurlSetConfig(NativeClient, config);
        }

        public Connection CreateConnection()
        {
            var id = SPUnityCurlCreateConn(NativeClient);
            return new Connection(this, id);
        }

        public void Update()
        {
            SPUnityCurlUpdate(NativeClient);
        }

        public void Dispose()
        {
            SPUnityCurlDestroy(NativeClient);
            _nativeClient = default(UIntPtr);
        }

        public bool Pause
        {
            set
            {
                SPUnityCurlOnApplicationPause(NativeClient, value);
            }
        }

        public bool Verbose
        {
            set
            {
                SPUnityCurlSetVerbose(NativeClient, value);
            }
        }

        public string VersionInfo
        {
            get
            {
                var info = String.Empty;
                var infoLength = SPUnityCurlGetVersionInfoLength(NativeClient);
                if(infoLength > 0)
                {
                    var bytes = new byte[infoLength];
                    SPUnityCurlGetVersionInfo(NativeClient, bytes);
                    info = System.Text.Encoding.ASCII.GetString(bytes);
                }
                return info;
            }
        }

        static int GetResponseErrorCode(int code)
        {
            switch((CurlError)code)
            {
            case CurlError.CURLE_URL_MALFORMAT:
            case CurlError.CURLE_UNSUPPORTED_PROTOCOL:
            case CurlError.CURLE_FAILED_INIT:
                return (int)HttpResponse.StatusCodeType.BadRequestError;
            case CurlError.CURLE_COULDNT_RESOLVE_PROXY:
            case CurlError.CURLE_COULDNT_RESOLVE_HOST:
            case CurlError.CURLE_COULDNT_CONNECT:
            case CurlError.CURLE_REMOTE_ACCESS_DENIED:
            case CurlError.CURLE_RECV_ERROR:
            case CurlError.CURLE_SEND_ERROR:
            case CurlError.CURLE_HTTP_RETURNED_ERROR:
            case CurlError.CURLE_TOO_MANY_REDIRECTS:
            case CurlError.CURLE_REMOTE_FILE_EXISTS:
            case CurlError.CURLE_REMOTE_DISK_FULL:
            case CurlError.CURLE_GOT_NOTHING:
            case CurlError.CURLE_SSL_CONNECT_ERROR:
            case CurlError.CURLE_PEER_FAILED_VERIFICATION:
                return (int)HttpResponse.StatusCodeType.ConnectionFailedError;
            case CurlError.CURLE_SSL_ENGINE_NOTFOUND:
            case CurlError.CURLE_SSL_CERTPROBLEM:
            case CurlError.CURLE_SSL_CIPHER:
            case CurlError.CURLE_SSL_CACERT:
            case CurlError.CURLE_SSL_ENGINE_SETFAILED:
            case CurlError.CURLE_USE_SSL_FAILED:
            case CurlError.CURLE_SSL_ENGINE_INITFAILED:
            case CurlError.CURLE_SSL_PINNEDPUBKEYNOTMATCH:
                return (int)HttpResponse.StatusCodeType.SSLError;
            case CurlError.CURLE_OPERATION_TIMEDOUT:
                return (int)HttpResponse.StatusCodeType.TimeOutError;
            case CurlError.CURLE_ABORTED_BY_CALLBACK:
                return (int)HttpResponse.StatusCodeType.CancelledError;
            default:
                return HttpResponse.MinClientUnknownErrorStatusCode + code;
            }
        }

        #region Inner Connection class

        public class Connection : IDisposable
        {
            readonly Curl _curl;
            readonly int _connectionId;

            public bool Streamed;

            public Connection(Curl curl, int connection)
            {
                _curl = curl;
                _connectionId = connection;
            }

            /// <summary>
            /// Perform the curl if the connection is still active.
            /// Returns true if the petition finishes.
            /// </summary>
            public bool Update()
            {
                return SPUnityCurlUpdateConn(_curl.NativeClient, _connectionId);
            }

            public int Send(UnmanagedMarshaledObject<RequestStruct> request)
            {
                return SPUnityCurlSend(_curl.NativeClient, request);
            }

            public int SendStreamMessage(UnmanagedMarshaledObject<MessageStruct> msg)
            {
                return SPUnityCurlSendStreamMessage(_curl.NativeClient, _connectionId, msg);
            }

            public int Id
            {
                get
                {
                    return _connectionId;
                }
            }

            public bool Finished
            {
                get
                {
                    return SPUnityCurlIsFinished(_curl.NativeClient, _connectionId);
                }
            }

            public double ConnectTime
            {
                get
                {
                    return SPUnityCurlGetConnectTime(_curl.NativeClient, _connectionId);
                }
            }

            public double TotalTime
            {
                get
                {
                    return SPUnityCurlGetTotalTime(_curl.NativeClient, _connectionId);
                }
            }

            public double DownloadSize
            {
                get
                {
                    return SPUnityCurlGetDownloadSize(_curl.NativeClient, _connectionId);
                }
            }

            public double DownloadSpeed
            {
                get
                {
                    return SPUnityCurlGetDownloadSpeed(_curl.NativeClient, _connectionId);
                }
            }

            public int Code
            {
                get
                {   
                    return SPUnityCurlGetResponseCode(_curl.NativeClient, _connectionId);
                }
            }

            public string Headers
            {
                get
                {
                    var headers = String.Empty;
                    var headersLength = SPUnityCurlGetHeadersLength(_curl.NativeClient, _connectionId);
                    if(headersLength > 0)
                    {
                        var bytes = new byte[headersLength];
                        SPUnityCurlGetHeaders(_curl.NativeClient, _connectionId, bytes);
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
                    var bodyLength = SPUnityCurlGetBodyLength(_curl.NativeClient, _connectionId);
                    if(bodyLength > 0)
                    {
                        body = new byte[bodyLength];
                        SPUnityCurlGetBody(_curl.NativeClient, _connectionId, body);
                    }
                    else
                    {
                        body = new byte[0];
                    }
                    return body;
                }
            }

            public byte[] Incoming
            {
                get
                {
                    byte[] message = null;
                    var len = SPUnityCurlGetStreamMessageLenght(_curl.NativeClient, _connectionId);
                    if(len > 0)
                    {
                        message = new byte[len];
                        SPUnityCurlGetStreamMessage(_curl.NativeClient, _connectionId, message);
                    }

                    return message;
                }
            }

            public int ErrorCode
            {
                get
                {
                    return SPUnityCurlGetErrorCode(_curl.NativeClient, _connectionId);
                }
            }

            public string ErrorMessage
            {
                get
                {
                    int errorLength = Curl.SPUnityCurlGetErrorLength(_curl.NativeClient, _connectionId);
                    var bytes = new byte[errorLength];
                    SPUnityCurlGetError(_curl.NativeClient, _connectionId, bytes);
                    var error = System.Text.Encoding.ASCII.GetString(bytes);
                    return error;
                }
            }

            public Error Error
            {
                get
                {
                    Error error = null;
                    var code = ErrorCode;
                    if(code != 0)
                    {
                        error = new Error(GetResponseErrorCode(code), ErrorMessage);
                    }
                    return error;
                }
            }

            public void Dispose()
            {
                SPUnityCurlDestroyConn(_curl.NativeClient, _connectionId);
            }
        }

        #endregion

        #region Native interface

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

            override public string ToString()
            {
                var builder = StringUtils.StartBuilder();
                builder.AppendFormat("Id: {0}\n", Id);
                builder.AppendFormat("Url: {0}\n", Url);
                builder.AppendFormat("Query: {0}\n", Query);
                builder.AppendFormat("Method: {0}\n", Method);
                builder.AppendFormat("Timeout: {0}\n", Timeout);
                builder.AppendFormat("ActivityTimeout: {0}\n", ActivityTimeout);
                builder.AppendFormat("Proxy: {0}\n", Proxy);
                builder.AppendFormat("Headers: {0}\n", Headers);
                if(Body != null)
                {
                    builder.AppendFormat("Body: {0}\n", System.Text.Encoding.ASCII.GetString(Body));
                }
                else
                {
                    builder.AppendLine("Body: null");
                }
                builder.AppendFormat("BodyLength: {0}\n", BodyLength);
                return StringUtils.FinishBuilder(builder);
            }
        };

        [StructLayout(LayoutKind.Sequential)]
        public struct MessageStruct
        {
            [MarshalAs(UnmanagedType.LPArray)]
            public byte[] Message;
            public int MessageLength;

            override public string ToString()
            {
                var builder = StringUtils.StartBuilder();
                if(Message != null)
                {
                    builder.AppendFormat("Message: {0}\n", System.Text.Encoding.ASCII.GetString(Message));
                }
                else
                {
                    builder.AppendLine("Message: null");
                }
                builder.AppendFormat("MessageLength: {0}\n", MessageLength);
                return StringUtils.FinishBuilder(builder);
            }
        };

        /// <summary>
        /// Curl errors. This is tied to the native Curl library.
        /// </summary>
        enum CurlError
        {
            CURLE_OK = 0,
            CURLE_UNSUPPORTED_PROTOCOL,
            CURLE_FAILED_INIT,
            CURLE_URL_MALFORMAT,
            CURLE_NOT_BUILT_IN,
            CURLE_COULDNT_RESOLVE_PROXY,
            CURLE_COULDNT_RESOLVE_HOST,
            CURLE_COULDNT_CONNECT,
            CURLE_FTP_WEIRD_SERVER_REPLY,
            CURLE_REMOTE_ACCESS_DENIED,
            CURLE_FTP_ACCEPT_FAILED,
            CURLE_FTP_WEIRD_PASS_REPLY,
            CURLE_FTP_ACCEPT_TIMEOUT,
            CURLE_FTP_WEIRD_PASV_REPLY,
            CURLE_FTP_WEIRD_227_FORMAT,
            CURLE_FTP_CANT_GET_HOST,
            CURLE_HTTP2,
            CURLE_FTP_COULDNT_SET_TYPE,
            CURLE_PARTIAL_FILE,
            CURLE_FTP_COULDNT_RETR_FILE,
            CURLE_OBSOLETE20,
            CURLE_QUOTE_ERROR,
            CURLE_HTTP_RETURNED_ERROR,
            CURLE_WRITE_ERROR,
            CURLE_OBSOLETE24,
            CURLE_UPLOAD_FAILED,
            CURLE_READ_ERROR,
            CURLE_OUT_OF_MEMORY,
            CURLE_OPERATION_TIMEDOUT,
            CURLE_OBSOLETE29,
            CURLE_FTP_PORT_FAILED,
            CURLE_FTP_COULDNT_USE_REST,
            CURLE_OBSOLETE32,
            CURLE_RANGE_ERROR,
            CURLE_HTTP_POST_ERROR,
            CURLE_SSL_CONNECT_ERROR,
            CURLE_BAD_DOWNLOAD_RESUME,
            CURLE_FILE_COULDNT_READ_FILE,
            CURLE_LDAP_CANNOT_BIND,
            CURLE_LDAP_SEARCH_FAILED,
            CURLE_OBSOLETE40,
            CURLE_FUNCTION_NOT_FOUND,
            CURLE_ABORTED_BY_CALLBACK,
            CURLE_BAD_FUNCTION_ARGUMENT,
            CURLE_OBSOLETE44,
            CURLE_INTERFACE_FAILED,
            CURLE_OBSOLETE46,
            CURLE_TOO_MANY_REDIRECTS,
            CURLE_UNKNOWN_OPTION,
            CURLE_TELNET_OPTION_SYNTAX,
            CURLE_OBSOLETE50,
            CURLE_PEER_FAILED_VERIFICATION,
            CURLE_GOT_NOTHING,
            CURLE_SSL_ENGINE_NOTFOUND,
            CURLE_SSL_ENGINE_SETFAILED,
            CURLE_SEND_ERROR,
            CURLE_RECV_ERROR,
            CURLE_OBSOLETE57,
            CURLE_SSL_CERTPROBLEM,
            CURLE_SSL_CIPHER,
            CURLE_SSL_CACERT,
            CURLE_BAD_CONTENT_ENCODING,
            CURLE_LDAP_INVALID_URL,
            CURLE_FILESIZE_EXCEEDED,
            CURLE_USE_SSL_FAILED,
            CURLE_SEND_FAIL_REWIND,
            CURLE_SSL_ENGINE_INITFAILED,
            CURLE_LOGIN_DENIED,
            CURLE_TFTP_NOTFOUND,
            CURLE_TFTP_PERM,
            CURLE_REMOTE_DISK_FULL,
            CURLE_TFTP_ILLEGAL,
            CURLE_TFTP_UNKNOWNID,
            CURLE_REMOTE_FILE_EXISTS,
            CURLE_TFTP_NOSUCHUSER,
            CURLE_CONV_FAILED,
            CURLE_CONV_REQD,
            CURLE_SSL_CACERT_BADFILE,
            CURLE_REMOTE_FILE_NOT_FOUND,
            CURLE_SSH,
            CURLE_SSL_SHUTDOWN_FAILED,
            CURLE_AGAIN,
            CURLE_SSL_CRL_BADFILE,
            CURLE_SSL_ISSUER_ERROR,
            CURLE_FTP_PRET_FAILED,
            CURLE_RTSP_CSEQ_ERROR,
            CURLE_RTSP_SESSION_ERROR,
            CURLE_FTP_BAD_FILE_LIST,
            CURLE_CHUNK_FAILED,
            CURLE_NO_CONNECTION_AVAILABLE,
            CURLE_SSL_PINNEDPUBKEYNOTMATCH,
            CURLE_SSL_INVALIDCERTSTATUS,
            CURL_LAST
        }

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
        static extern int SPUnityCurlSend(UIntPtr client, IntPtr data);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlSendStreamMessage(UIntPtr client, int id, IntPtr data);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityCurlUpdate(UIntPtr client);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityCurlUpdateConn(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetError(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetBody(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetHeaders(UIntPtr client, int id, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetVersionInfo(UIntPtr client, byte[] data);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlGetStreamMessage(UIntPtr client, int id, byte[] data);

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
        static extern int SPUnityCurlGetVersionInfoLength(UIntPtr client);

        [DllImport(PluginModuleName)]
        static extern int SPUnityCurlGetStreamMessageLenght(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern bool SPUnityCurlIsFinished(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetConnectTime(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetTotalTime(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetDownloadSize(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern double SPUnityCurlGetDownloadSpeed(UIntPtr client, int id);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlOnApplicationPause(UIntPtr client, bool pause);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlSetConfig(UIntPtr client, string name);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCurlSetVerbose(UIntPtr client, bool verbose);

        #endregion
    }
}
