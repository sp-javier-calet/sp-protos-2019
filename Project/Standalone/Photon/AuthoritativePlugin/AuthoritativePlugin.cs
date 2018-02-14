using Photon.Hive.Plugin;
using SocialPoint.Attributes;
using SocialPoint.Matchmaking;
using SocialPoint.Network;
using SocialPoint.Network.ServerEvents;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Multiplayer
{
    public class AuthoritativePlugin : NetworkServerPlugin
    {
        protected override bool Full
        {
            get
            {
                return _netServer != null && _netServer.Full;
            }
        }

        protected override int MaxPlayers
        {
            get
            {
                return _netServer != null ? _netServer.MaxPlayers : 0;
            }
        }

        protected override int UpdateInterval
        {
            get
            {
                return _updateInterval;
            }
        }

        NetworkServerSceneController _netServer;

        HttpMatchmakingServer _matchmaking;

        object _game;
        int _updateInterval = 70;
        int _httpTimeout;

        const string MaxPlayersConfig = "MaxPlayers";
        const string GameAssemblyNameConfig = "GameAssemblyName";
        const string CheatPasswordConfig = "CheatPassword";
        const string GameTypeConfig = "GameType";
        const string UsePluginHttpClient = "UsePluginHttpClient";
        const string HttpMaxRetriesConfig = "HttpMaxRetries";
        const string HttpTimeoutConfig = "HttpTimeout";

        const string HttpRequestMethodParam = "request_method";
        const string HttpRequestUrlParam = "request_url";
        const string HttpRequestBodyParam = "request_body";
        const string HttpResponseCodeParam = "response_code";
        const string HttpResponseBodyParam = "response_body";
        const string HttpResponseErrorParam = "response_error";
        const string HttpResponseHeadersParam = "response_headers";

        const string HttpRetryFailedLog = "Http Request Failed";

        const int RetryLogHttpRequestBodyMaxLength = 1024;

        public AuthoritativePlugin(string name="Authoritative") : base(name)
        {
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, error) =>
            {
                return true;
            };
        }

        public override bool SetupInstance(IPluginHost host, Dictionary<string, string> config, out string errorMsg)
        {
            if(!base.SetupInstance(host, config, out errorMsg))
            {
                return false;
            }

            var usePluginHttpClient = GetConfigOption(config, UsePluginHttpClient, false);

            IHttpClient innerHttpClient = null;

            if(usePluginHttpClient)
            {
                innerHttpClient = new PluginHttpClient(PluginHost);
            }
            else
            {
                innerHttpClient = new ImmediateWebRequestHttpClient();
            }

            _httpTimeout = GetConfigOption(config, HttpTimeoutConfig, 0);

            var mmHttpClient = new RetryHttpClient(innerHttpClient);
            mmHttpClient.MaxRetries = GetConfigOption(config, HttpMaxRetriesConfig, 0);
            mmHttpClient.RequestSetup += HttpRequestSetup;
            mmHttpClient.RetryFailed += HttpRetryFailed;

            var innerTracksHttpClient = new WebRequestHttpClient(new UpdateCoroutineRunner(TrackingsHttpClientScheduler));
            var trackHttpClient = new RetryHttpClient(innerTracksHttpClient);
            trackHttpClient.MaxRetries = GetConfigOption(config, HttpMaxRetriesConfig, 0);
            trackHttpClient.RequestSetup += HttpRequestSetup;

            Func<string> getBaseUrlCallback = () => { return BaseBackendUrl; };

            _matchmaking = new HttpMatchmakingServer(mmHttpClient, getBaseUrlCallback);
            PluginEventTracker = new HttpServerEventTracker(UpdateScheduler, trackHttpClient);
            PluginEventTracker.Start();

            string gameAssembly;
            string gameType;
            if (config.TryGetValue(GameAssemblyNameConfig, out gameAssembly) && config.TryGetValue(GameTypeConfig, out gameType))
            {
                try
                {
                    var factory = (INetworkServerGameFactory)CreateInstanceFromAssembly(gameAssembly, gameType);
                    _game = factory.Create(this, _fileManager, _matchmaking, UpdateScheduler, config);
                    var netServerProvider = _game as INetworkServerSceneControllerProvider;
                    if(netServerProvider != null)
                    {
                        _netServer = netServerProvider.NetworkServerSceneController;
                    }
                    else
                    {
                        _netServer = new NetworkServerSceneController(this, new NetworkSceneContext());
                    }
                }
                catch (Exception e)
                {
                    errorMsg = e.Message;
                }
            }

            _netServer.SendMetric = PluginEventTracker.SendMetric;
            _netServer.SendLog = PluginEventTracker.SendLog;
            _netServer.SendTrack = PluginEventTracker.SendTrack;

            _netServer.ServerConfig.MaxPlayers = (byte)GetConfigOption(config, MaxPlayersConfig, _netServer.ServerConfig.MaxPlayers);
            _netServer.ServerConfig.MetricSendInterval = GetConfigOption(config, MetricSendIntervalConfig, _netServer.ServerConfig.MetricSendInterval);
            _netServer.ServerConfig.UsePluginHttpClient = usePluginHttpClient;
            _netServer.ServerConfig.GetBackendUrlCallback = getBaseUrlCallback;
            config.TryGetValue(MetricEnvironmentConfig, out _netServer.ServerConfig.MetricEnvironment);
            
            if(PluginEventTracker != null)
            {
                PluginEventTracker.Environment = _netServer.ServerConfig.MetricEnvironment;
                PluginEventTracker.GetBaseUrlCallback = _netServer.ServerConfig.GetBackendUrlCallback;
                PluginEventTracker.Platform = "PhotonPlugin";
                PluginEventTracker.UpdateCommonTrackData += (data) => { data.SetValue("ver", AppVersion); };
            }

            return string.IsNullOrEmpty(errorMsg);
        }

        void HttpRequestSetup(SocialPoint.Network.HttpRequest req)
        {
            if(_httpTimeout != 0)
            {
                req.Timeout = _httpTimeout;
            }
        }

        void HttpRetryFailed(SocialPoint.Network.HttpRequest req, SocialPoint.Network.HttpResponse resp)
        {
            // Logs the request and response to make it easier to understand why the retries failed.

            var data = new AttrDic();
            data.SetValue(HttpRequestMethodParam, req.Method.ToString());
            data.SetValue(HttpRequestUrlParam, req.Url.ToString());
            data.SetValue(HttpResponseCodeParam, resp.StatusCode);
            if(resp.Error != null)
            {
                data.SetValue(HttpResponseErrorParam, resp.Error.ToString());
            }
            data.SetValue(HttpResponseHeadersParam, resp.ToStringHeaders());

            if(req.Body != null && req.Body.Length > 0)
            {
                string body = System.Text.Encoding.UTF8.GetString(req.Body);
                if(body.Length > RetryLogHttpRequestBodyMaxLength)
                {
                    body = body.Substring(0, RetryLogHttpRequestBodyMaxLength);
                }
                data.SetValue(HttpRequestBodyParam, body);
            }
            if(resp.Body != null && resp.Body.Length > 0)
            {
                string body = System.Text.Encoding.UTF8.GetString(resp.Body);
                if(body.Length > RetryLogHttpRequestBodyMaxLength)
                {
                    body = body.Substring(0, RetryLogHttpRequestBodyMaxLength);
                }
                data.SetValue(HttpResponseBodyParam, body);
            }
            PluginEventTracker.SendLog(new Log(LogLevel.Error, HttpRetryFailedLog, data));
        }

        protected override void Update(float dt)
        {
            base.Update(dt);
            _netServer.Update(dt);
        }
    }
}
