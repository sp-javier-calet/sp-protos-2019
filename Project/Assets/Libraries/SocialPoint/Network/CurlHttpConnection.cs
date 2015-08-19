using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using SocialPoint.Network;
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
        DateTime _startTime;
        double _connectionTime;
        double _transferTime;
        HttpRequest  _request;
        HttpResponseDelegate _delegate;
        string _error;
        const char kHeaderEnd = '\n';
        const char kHeaderSeparator = ':';

        public override IEnumerator Update()
        {
            _connectionTime = GetSecondsSinceStart();
            while(!_dataReceived)
            {
                int isFinished = CurlBridge.SPUnityCurlUpdate(_connectionId);
                if(isFinished == 1)
                {
                    _transferTime = GetSecondsSinceStart() - _connectionTime;
                    ReceiveData();
                    break;
                }
                yield return null;
            }
        }

        private double GetSecondsSinceStart()
        {
            return (TimeUtils.Now.ToLocalTime() - _startTime).TotalSeconds;
        }

        public CurlHttpConnection(int connectionId, HttpRequest req, HttpResponseDelegate del)
        {
            _connectionId = connectionId;
            _request = req;
            _delegate = del;
            _dataReceived = false;
            _startTime = TimeUtils.Now.ToLocalTime();
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
            if(r.HasError)
            {
                r.Error = new Error(_respCode, _error);
            }
            r.OriginalBody = new Data(_body);
            r.DownloadSize = _downloadSize;
            r.DownloadSpeed = _downloadSpeed;

            r.ConnectionDuration = _connectionTime;
            r.TransferDuration = _transferTime;
            
            return r;
        }

        public override void Cancel()
        {
            if(_delegate != null)
            {
                Delegate[] data = _delegate.GetInvocationList();

                for(int k = 0; k < data.Length; k++)
                {
                    HttpResponseDelegate item = data[k] as HttpResponseDelegate;
                    _delegate -= item;
                }
            }
        }

        static CurlBridge.RequestStruct CreateRequestStruct(HttpRequest request, int id=0)
        {
            var data = new CurlBridge.RequestStruct();
            // Separate the path and the query string
            string urlPath = request.Url.GetLeftPart(UriPartial.Path);
            string queryParamsStr = request.Url.Query;

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

            data.Id = id;
            data.Url = urlPath;
            data.Query = queryParamsStr;
            data.Method = request.Method.ToString();
            data.Timeout = (int)request.Timeout;
            data.ActivityTimeout = (int)request.ActivityTimeout;
            data.Proxy = request.Proxy;
            data.Headers = request.ToStringHeaders();
            data.Body = request.Body.Bytes;
            data.BodyLength = request.Body.Bytes != null ? request.Body.Bytes.Length : 0;
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
