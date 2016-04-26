using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
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

    public class CurlHttpConnection : BaseYieldHttpConnection
    {
        byte[] _body;
        public string _headers;
        int _respCode;
        int _connectionId;
        bool _dataReceived;
        double _downloadSize;
        double _downloadSpeed;
        double _connectTime;
        double _totalTime;
        HttpRequest _request;
        Error _error;
        bool _cancelled;
        const char kHeaderEnd = '\n';
        const char kHeaderSeparator = ':';

        public override IEnumerator Update()
        {
            while(!_dataReceived)
            {
                int isFinished = CurlBridge.SPUnityCurlUpdate(_connectionId);
                if(isFinished == 1)
                {
                    ReceiveData();
                    break;
                }
                yield return null;
            }
        }

        public CurlHttpConnection(int connectionId, HttpRequest req, HttpResponseDelegate del) :
            base(del)
        {
            _connectionId = connectionId;
            _request = req;
            _dataReceived = false;
            Send(_connectionId, _request);
        }


        int GetResponseErrorCode(int code)
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

        public HttpResponse getResponse()
        {   
            Dictionary<string, string> headersData = new Dictionary<string, string>();

            if(_headers != null)
            {
                string[] lines = _headers.Split(new char[]{ kHeaderEnd });
                for(int i = 1; i < lines.Length; i++)
                {
                    if(lines[i].Length < 3)
                    {
                        continue;
                    }
                    string[] head = lines[i].Split(new char[]{ kHeaderSeparator });
                    if(head.Length >= 2)
                    {
                        headersData.Add(head[0].Trim(), head[1].Trim());
                    }
                }
            }

            HttpResponse r = new HttpResponse(_respCode, headersData);
            if(_cancelled)
            {
                r.Error = new Error((int)HttpResponse.StatusCodeType.CancelledError, "Connection was cancelled");
            }
            else if(!Error.IsNullOrEmpty(_error))
            {
                r.Error = _error;
                r.StatusCode = _error.Code;
            }
            else if(r.HasError)
            {
                r.Error = new Error(r.StatusCode, "HTTP Server responded with error code.");
            }
            r.OriginalBody = _body;
            r.DownloadSize = _downloadSize;
            r.DownloadSpeed = _downloadSpeed;

            r.ConnectionDuration = _connectTime;
            r.TransferDuration = _totalTime - _connectTime;
            
            return r;
        }

        public override void Cancel()
        {
            _cancelled = true;
            ReceiveData();
        }

        static CurlBridge.RequestStruct CreateRequestStruct(HttpRequest request, int id = 0)
        {
            var data = new CurlBridge.RequestStruct();
            var urlPath = string.Empty;
            var queryParamsStr = string.Empty;
            if(request.Url != null)
            {
                // Separate the path and the query string
                urlPath = request.Url.GetLeftPart(UriPartial.Path);
                queryParamsStr = request.Url.Query;

                // Uri.GetLeftPart(UriPartial.Path) returns a path with a trailing
                // slash that  we need to remove
                if(!string.IsNullOrEmpty(urlPath)
                    && StringUtils.EndsWith(urlPath, @"/"))
                {
                    urlPath = urlPath.Substring(0, urlPath.Length - 1);
                }
                // Uri.Query returns the query string with a leading '?'
                // Curl automatically appends a '?' between the path and the
                // query string so we need to remove it to avoid ending with
                // a  '??'' in the query string
                if(!string.IsNullOrEmpty(queryParamsStr)
                    && StringUtils.StartsWith(queryParamsStr, @"?"))
                {
                    queryParamsStr = queryParamsStr.Substring(1);
                }
            }

            data.Id = id;
            data.Url = urlPath;
            data.Query = queryParamsStr;
            data.Method = request.Method.ToString();
            data.Timeout = (int)request.Timeout;
            data.ActivityTimeout = (int)request.ActivityTimeout;
            data.Proxy = request.Proxy;
            data.Headers = request.ToStringHeaders();
            data.Body = request.Body;
            data.BodyLength = request.Body != null ? request.Body.Length : 0;
            return data;
        }

        private void Send(int id, HttpRequest req)
        {
            var data = CreateRequestStruct(req, id);
            int ok = CurlBridge.SPUnityCurlSend(data);
            if(ok == 0)
            {
                ReceiveData();
            }
        }

        private void ReceiveData()
        {
            _dataReceived = true;
            int bodyLength = CurlBridge.SPUnityCurlGetBodyLength(_connectionId);
            int HeadersLength = CurlBridge.SPUnityCurlGetHeadersLength(_connectionId);

            _connectTime = CurlBridge.SPUnityCurlGetConnectTime(_connectionId);
            _totalTime = CurlBridge.SPUnityCurlGetTotalTime(_connectionId);
            _downloadSize = CurlBridge.SPUnityCurlGetDownloadSize(_connectionId);
            _downloadSpeed = CurlBridge.SPUnityCurlGetDownloadSpeed(_connectionId);
            _respCode = CurlBridge.SPUnityCurlGetResponseCode(_connectionId);            
            _error = null;
            int errorCode = CurlBridge.SPUnityCurlGetErrorCode(_connectionId);
            if(errorCode != 0)
            {
                int errorLength = CurlBridge.SPUnityCurlGetErrorLength(_connectionId);
                byte[] bytes = new byte[errorLength];
                CurlBridge.SPUnityCurlGetError(_connectionId, bytes);
                _error = new Error(
                    GetResponseErrorCode(errorCode),
                    System.Text.Encoding.ASCII.GetString(bytes));
            }
            
            _body = new byte[0];
            if(bodyLength > 0)
            {
                _body = new byte[bodyLength];
                CurlBridge.SPUnityCurlGetBody(_connectionId, _body);
            }
            _headers = String.Empty;
            if(HeadersLength > 0)
            {
                byte[] bytes = new byte[HeadersLength];
                CurlBridge.SPUnityCurlGetHeaders(_connectionId, bytes);
                _headers = System.Text.Encoding.ASCII.GetString(bytes);
            }

            CurlBridge.SPUnityCurlDestroyConn(_connectionId);
            
            HttpResponse resp = null;
            try
            {
                resp = getResponse();
            }
            catch(Exception e)
            {
                UnityEngine.Debug.LogError(e.ToString());
                return;
            }
            
            OnResponse(resp);
        }
    }
}
