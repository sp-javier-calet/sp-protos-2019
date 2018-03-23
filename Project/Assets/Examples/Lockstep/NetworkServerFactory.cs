using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Lockstep;
using SocialPoint.Matchmaking;
using SocialPoint.Network;
using SocialPoint.Network.ServerEvents;
using SocialPoint.Utils;

namespace Examples.Lockstep
{
    public class NetworkServerFactory : INetworkServerGameFactory
    {
        const string HttpMaxRetriesConfig = "HttpMaxRetries";
        const string HttpTimeoutConfig = "HttpTimeout";
        const string BaseBackendUrl = "BaseBackendUrl";

        const string HttpRequestMethodParam = "request_method";
        const string HttpRequestUrlParam = "request_url";
        const string HttpRequestBodyParam = "request_body";
        const string HttpResponseCodeParam = "response_code";
        const string HttpResponseBodyParam = "response_body";
        const string HttpResponseErrorParam = "response_error";
        const string HttpResponseHeadersParam = "response_headers";
        const string HttpRetryFailedLog = "Lockstep: Http Request Failed";

        const int RetryLogHttpRequestBodyMaxLength = 1024;

        int _httpTimeout;
        string _baseBackendUrl;
        Dictionary<string, string> _config;
        IUpdateScheduler _updateScheduler;
        UpdateCoroutineRunner _coroutineRunner;
        WebRequestHttpClient _innerClient;
        HttpServerEventTracker _eventTracker;

        long GetConfig(Dictionary<string, string> config, string k, long def)
        {
            string str;
            if(config.TryGetValue(k, out str))
            {
                long l;
                if(long.TryParse(str, out l))
                {
                    return l;
                }
            }
            return def;
        }

        int GetConfig(Dictionary<string, string> config, string k, int def)
        {
            string str;
            if (config.TryGetValue(k, out str))
            {
                int l;
                if (int.TryParse(str, out l))
                {
                    return l;
                }
            }
            return def;
        }

        WebRequestHttpClient InnerClient
        {
            get
            {
                if(_innerClient == null)
                {
                    _innerClient = new WebRequestHttpClient(CoroutineRunner);
                }
                return _innerClient;
            }
        }

        UpdateCoroutineRunner CoroutineRunner
        {
            get
            {
                if (_coroutineRunner == null)
                {
                    _coroutineRunner = new UpdateCoroutineRunner(_updateScheduler);
                }
                return _coroutineRunner;
            }
        }

        public object Create(LockstepNetworkServer server, Dictionary<string, string> config, IUpdateScheduler updateScheduler)
        {
            _config = config;
            _updateScheduler = updateScheduler;

            var gameConfig = new Config();
            gameConfig.Duration = GetConfig(_config, "Duration", gameConfig.Duration);
            gameConfig.ManaSpeed = GetConfig(_config, "ManaSpeed", gameConfig.ManaSpeed);
            gameConfig.MaxMana = GetConfig(_config, "MaxMana", gameConfig.MaxMana);
            gameConfig.UnitCost = GetConfig(_config, "UnitCost", gameConfig.UnitCost);

            _httpTimeout = GetConfig(_config, HttpTimeoutConfig, 0);
            _baseBackendUrl = _config[BaseBackendUrl];

            return new ServerBehaviour(server, gameConfig);
        }

        public IMatchmakingServer CreateMatchmakingServer()
        {
            var mmHttpClient = new RetryHttpClient(InnerClient);
            SetupRetryClient(mmHttpClient);
            mmHttpClient.RetryFailed += HttpRetryFailed;

            return new HttpMatchmakingServer(mmHttpClient, () => { return _baseBackendUrl; });
        }

        public HttpServerEventTracker CreateHttpServerEventTracker()
        {
            var trackHttpClient = new RetryHttpClient(InnerClient);
            SetupRetryClient(trackHttpClient);

            _eventTracker = new HttpServerEventTracker(_updateScheduler, trackHttpClient);
            return _eventTracker;
        }

        void SetupRetryClient(RetryHttpClient client)
        {
            client.MaxRetries = GetConfig(_config, HttpMaxRetriesConfig, 0);
            client.RequestSetup += HttpRequestSetup;
        }

        void HttpRequestSetup(HttpRequest req)
        {
            SocialPoint.Base.DebugUtils.Assert(_httpTimeout >= 0, "The timeout cannot be negative");

            req.Timeout = _httpTimeout;
        }

        void HttpRetryFailed(HttpRequest req, HttpResponse resp)
        {
            // Logs the request and response to make it easier to understand why the retries failed.

            if(_eventTracker == null)
            {
                return;
            }

            var data = new AttrDic();
            data.SetValue(HttpRequestMethodParam, req.Method.ToString());
            data.SetValue(HttpRequestUrlParam, req.Url.ToString());
            data.SetValue(HttpResponseCodeParam, resp.StatusCode);
            if (!SocialPoint.Base.Error.IsNullOrEmpty(resp.Error))
            {
                data.SetValue(HttpResponseErrorParam, resp.Error.ToString());
            }
            data.SetValue(HttpResponseHeadersParam, resp.ToStringHeaders());

            ParseBodyData(req.Body, data, HttpRequestBodyParam);
            ParseBodyData(resp.Body, data, HttpResponseBodyParam);
            _eventTracker.SendLog(new Log(LogLevel.Error, HttpRetryFailedLog, data));
        }

        void ParseBodyData(byte[] body, AttrDic dict, string param)
        {
            if (body != null && body.Length > 0)
            {
                string bodyData = System.Text.Encoding.UTF8.GetString(body);
                if (bodyData.Length > RetryLogHttpRequestBodyMaxLength)
                {
                    bodyData = bodyData.Substring(0, RetryLogHttpRequestBodyMaxLength);
                }
                dict.SetValue(param, bodyData);
            }
        }
    }
}