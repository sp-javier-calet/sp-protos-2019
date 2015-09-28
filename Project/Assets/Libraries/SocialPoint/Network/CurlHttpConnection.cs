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
        HttpRequest  _request;
        HttpResponseDelegate _delegate;
        string _error;
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

        public CurlHttpConnection(int connectionId, HttpRequest req, HttpResponseDelegate del)
        {
            _connectionId = connectionId;
            _request = req;
            _delegate = del;
            _dataReceived = false;
            Send(_connectionId, _request);
        }
 
        public HttpResponse getResponse()
        {   
            Dictionary<string, string> headersData = new Dictionary<string, string>();

            if(_headers != null)
            {
                string[] lines = _headers.Split(new char[]{kHeaderEnd});
                for(int i = 1; i<lines.Length; i++)
                {
                    if(lines[i].Length < 3)
                    {
                        continue;
                    }
                    string[] head = lines[i].Split(new char[]{kHeaderSeparator});
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
            else if(r.HasError)
            {
                r.Error = new Error(_respCode, _error);
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

        static CurlBridge.RequestStruct CreateRequestStruct(HttpRequest request, int id=0)
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
                    && urlPath.EndsWith(@"/"))
                {
                    urlPath = urlPath.Substring(0, urlPath.Length - 1);
                }
                // Uri.Query returns the query string with a leading '?'
                // Curl automatically appends a '?' between the path and the
                // query string so we need to remove it to avoid ending with
                // a  '??'' in the query string
                if(!string.IsNullOrEmpty(queryParamsStr)
                    && queryParamsStr.StartsWith(@"?"))
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
            int errorLength = CurlBridge.SPUnityCurlGetErrorLength(_connectionId);
            int HeadersLength = CurlBridge.SPUnityCurlGetHeadersLength(_connectionId);

            _connectTime = CurlBridge.SPUnityCurlGetConnectTime(_connectionId);
            _totalTime = CurlBridge.SPUnityCurlGetTotalTime(_connectionId);
            _downloadSize = CurlBridge.SPUnityCurlGetDownloadSize(_connectionId);
            _downloadSpeed = CurlBridge.SPUnityCurlGetDownloadSpeed(_connectionId);

            _respCode = CurlBridge.SPUnityCurlGetCode(_connectionId);
            
            // Treat response code 0 as a connection error
            if(_respCode == 0)
            {
                _respCode = (int)HttpResponse.StatusCodeType.ConnectionFailedError;
            }
            
            _error = String.Empty;
            if(errorLength > 0)
            {
                byte[] bytes = new byte[errorLength];
                CurlBridge.SPUnityCurlGetError(_connectionId, bytes);
                _error = System.Text.Encoding.ASCII.GetString(bytes);
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
            
            if(_delegate != null)
            {
                _delegate(resp);
            }
        }
    }
}
