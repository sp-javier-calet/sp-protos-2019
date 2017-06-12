using SocialPoint.Network;
using SocialPoint.Attributes;
using SocialPoint.Base;
using System.Collections.Generic;
using System;

namespace SocialPoint.Matchmaking
{
    
    public class HttpMatchmakingServer : IMatchmakingServer
    {
        IHttpClient _httpClient;
        IAttrParser _parser;
        List<IMatchmakingServerDelegate> _delegates;

        Func<string> _getBaseUrl;

        public bool Enabled
        {
            get
            {
                return  _getBaseUrl != null && !string.IsNullOrEmpty(_getBaseUrl());
            }
        }

        public string Version { get; set; }

        public HttpMatchmakingServer(IHttpClient httpClient, Func<string> getBaseUrlCallback)
        {
            _delegates = new List<IMatchmakingServerDelegate>();
            _httpClient = httpClient;
            _getBaseUrl = getBaseUrlCallback;
            _parser = new JsonAttrParser();
        }

        const string InfoUri = "/get_match";
        const string EndUri = "/end_match";
        const string UsersParam = "users";
        const string MatchIdParam = "match_id";
        const string PlayerIdParam = "player{0}_token";
        const string VersionParam = "version";
        const string PlayersParam = "players";

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
            var req = CreateRequest(InfoUri);
            req.AddQueryParam(MatchIdParam, matchId);
            for(var i = 0; i < playerIds.Count; i++)
            {
                req.AddQueryParam(string.Format(PlayerIdParam, i + 1), playerIds[i]);
            }
            if(!string.IsNullOrEmpty(Version))
            {
                req.AddQueryParam(VersionParam, Version);
            }
            _httpClient.Send(req, OnInfoReceived);
        }

        void OnInfoReceived(HttpResponse resp)
        {
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

        public void NotifyResults(string matchId, AttrDic userData)
        {
            var req = CreateRequest(EndUri);
            req.Method = HttpRequest.MethodType.POST;
            req.AddParam(MatchIdParam, matchId);
            req.AddParam(PlayersParam, userData);
            _httpClient.Send(req, (resp) => OnResultReceived(resp, userData));
        }

        void OnResultReceived(HttpResponse resp, AttrDic userData)
        {
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
            return new HttpRequest(baseUrl + uri);
        }

    }
}
