using SocialPoint.Network;
using SocialPoint.Attributes;
using SocialPoint.Base;
using System.Collections.Generic;
using System;
using SocialPoint.Utils;

namespace SocialPoint.Matchmaking
{
    
    public class HttpMatchmakingServer : IMatchmakingServer
    {
        IHttpClient _httpClient;
        IAttrParser _parser;
        List<IMatchmakingServerDelegate> _delegates;

        HttpRequest _infoRequest;
        HttpRequest _notifyRequest;
        HttpResponse _infoResponse;
        HttpResponse _notifyResponse;

        readonly Func<string> _getBaseUrl;

        public bool Enabled
        {
            get
            {
                return  _getBaseUrl != null && !string.IsNullOrEmpty(_getBaseUrl());
            }
        }

        public string Version { get; set; }

        public AttrDic ClientsVersions { get; set; }

        public HttpRequest InfoRequest
        {
            get
            {
                return _infoRequest;
            }
        }

        public HttpRequest NotifyRequest
        {
            get
            {
                return _notifyRequest;
            }
        }

        public HttpResponse InfoResponse
        {
            get
            {
                return _infoResponse;
            }
        }

        public HttpResponse NotifyResponse
        {
            get
            {
                return _notifyResponse;
            }
        }

        public HttpMatchmakingServer(IHttpClient httpClient, Func<string> getBaseUrlCallback)
        {
            _delegates = new List<IMatchmakingServerDelegate>();
            _httpClient = httpClient;
            _getBaseUrl = getBaseUrlCallback;
            _parser = new JsonAttrParser();
        }

        const string InfoUri = "/get_match";
        const string EndUri = "/end_match";
        const string MatchIdParam = "match_id";
        const string VersionParam = "version";
        const string ClientsVersionsParam = "clients_versions";
        const string PlayersParam = "players";
        const string CustomDataParam = "custom_data";

        public void AddDelegate(IMatchmakingServerDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(IMatchmakingServerDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void LoadInfo(string matchId, List<string> playerIds)
        {
            _infoRequest = CreateRequest(InfoUri);
            _infoRequest.AddQueryParam(MatchIdParam, matchId);
            if(!string.IsNullOrEmpty(Version))
            {
                _infoRequest.AddQueryParam(VersionParam, Version);
            }
            if(ClientsVersions != null)
            {
                _infoRequest.AddQueryParam(ClientsVersionsParam, ClientsVersions);
            }
            _httpClient.Send(_infoRequest, OnInfoReceived);
        }

        void OnInfoReceived(HttpResponse resp)
        {
            _infoResponse = resp;
            if(resp.HasError)
            {
                OnError(resp.Error);
                return;
            }

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatchInfoReceived(resp.Body);
            }
        }

        void OnError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        public void NotifyResults(string matchId, AttrDic userData, AttrDic customData)
        {
            _notifyRequest = CreateRequest(EndUri);
            _notifyRequest.Method = HttpRequest.MethodType.POST;
            _notifyRequest.AddParam(MatchIdParam, matchId);
            _notifyRequest.AddParam(PlayersParam, userData);
            _notifyRequest.AddParam(CustomDataParam, customData);
            _httpClient.Send(_notifyRequest, resp => OnResultReceived(resp, userData));
        }

        void OnResultReceived(HttpResponse resp, AttrDic userData)
        {
            _notifyResponse = resp;
            if(resp.HasError)
            {
                OnError(resp.Error);
                return;
            }
            try
            {
                AttrDic attr = null;
                if(resp.Body != null && resp.Body.Length != 0)
                {
                    attr = _parser.Parse(resp.Body).AsDic;
                }
                if(attr == null || attr.Count == 0)
                {
                    attr = userData;
                }
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnResultsReceived(attr);
                }
            }
            catch(Exception e)
            {
                OnError(new Error(e.ToString()));
                return;
            }
        }

        HttpRequest CreateRequest(string uri)
        {
            string baseUrl = string.Empty;
            if(_getBaseUrl != null)
            {
                baseUrl = _getBaseUrl();
            }
            if(string.IsNullOrEmpty(baseUrl))
            {
                throw new InvalidOperationException("Base url not configured.");
            }
            return new HttpRequest(StringUtils.CombineUri(baseUrl, "/matchmaking" + uri));
        }

    }
}
