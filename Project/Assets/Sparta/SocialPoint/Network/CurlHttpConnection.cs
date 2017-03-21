using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.Base;
using SocialPoint.Network;
using SocialPoint.Utils;

namespace SocialPoint.Network
{
    public sealed class CurlHttpConnection : BaseYieldHttpConnection
    {
        byte[] _body;
        public string _headers;
        int _respCode;
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
        const string kSlash = @"/";
        const string kQuestionMark = @"?";

        readonly Curl.Connection _connection;
        UnmanagedMarshaledObject<Curl.RequestStruct> _requestStruct;

        public override IEnumerator Update()
        {
            while(!_dataReceived)
            {
                bool finished = _connection.Update();
                if(finished)
                {
                    ReceiveData();
                    break;
                }

                yield return null;
            }
        }

        public CurlHttpConnection(Curl.Connection connection, HttpRequest req, HttpResponseDelegate del) :
            base(del)
        {
            _connection = connection;
            _request = req;
            _dataReceived = false;
            Send(_connection, _request);
        }

        public HttpResponse GetResponse()
        {   
            var headersData = new Dictionary<string, string>();

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

            var r = new HttpResponse(_respCode, headersData);
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

        static UnmanagedMarshaledObject<Curl.RequestStruct> CreateMarshaledRequestStruct(HttpRequest request, int connectionId = 0)
        {
            return new UnmanagedMarshaledObject<Curl.RequestStruct>(CreateRequestStruct(request, connectionId));
        }

        static Curl.RequestStruct CreateRequestStruct(HttpRequest request, int connectionId = 0)
        {
            var data = new Curl.RequestStruct();
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
                   && StringUtils.EndsWith(urlPath, kSlash))
                {
                    urlPath = urlPath.Substring(0, urlPath.Length - 1);
                }
                // Uri.Query returns the query string with a leading '?'
                // Curl automatically appends a '?' between the path and the
                // query string so we need to remove it to avoid ending with
                // a  '??'' in the query string
                if(!string.IsNullOrEmpty(queryParamsStr)
                   && StringUtils.StartsWith(queryParamsStr, kQuestionMark))
                {
                    queryParamsStr = queryParamsStr.Substring(1);
                }
            }

            data.Id = connectionId;
            data.Url = urlPath ?? string.Empty;
            data.Query = queryParamsStr ?? string.Empty;
            data.Method = request.Method.ToString() ?? string.Empty;
            data.Timeout = (int)request.Timeout;
            data.ActivityTimeout = (int)request.ActivityTimeout;
            data.Proxy = request.Proxy ?? string.Empty;
            data.Headers = request.ToStringHeaders() ?? string.Empty;
            data.Body = request.Body;
            data.BodyLength = request.Body != null ? request.Body.Length : 0;
            return data;
        }

        void Send(Curl.Connection connection, HttpRequest req)
        {
            _requestStruct = CreateMarshaledRequestStruct(req, connection.Id);
            int ok = connection.Send(_requestStruct);
            if(ok == 0)
            {
                ReceiveData();
            }
        }

        void ReceiveData()
        {
            _dataReceived = true;
            _connectTime = _connection.ConnectTime;
            _totalTime = _connection.TotalTime;
            _downloadSize = _connection.DownloadSize;
            _downloadSpeed = _connection.DownloadSize;
            _respCode = _connection.Code;
            _error = _connection.Error;
            _body = _connection.Body;
            _headers = _connection.Headers;
            _connection.Dispose();

            _requestStruct = null;
            
            HttpResponse resp;
            try
            {
                resp = GetResponse();
            }
            catch(Exception e)
            {
                Log.x(e);
                return;
            }
            
            OnResponse(resp);
        }
    }
}
