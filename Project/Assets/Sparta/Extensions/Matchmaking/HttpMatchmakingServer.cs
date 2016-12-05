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

        public string BaseUrl;

        public bool Enabled
        {
            get
            {
                return !string.IsNullOrEmpty(BaseUrl);
            }
        }

        public HttpMatchmakingServer(IHttpClient httpClient, string baseUrl=null)
        {
            _delegates = new List<IMatchmakingServerDelegate>();
            _httpClient = httpClient;
            BaseUrl = baseUrl;
            _parser = new JsonAttrParser();
        }

        const string InfoUri = "/get_match";
        const string EndUri = "/battle_end";
        const string ResultUri = "/api/v3/battle/end";
        const string UsersParam = "users";
        const string MatchIdParam = "match_id";
        const string PlayerIdParam = "token_{0}";

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
            for (var i=0; i<playerIds.Count; i++)
            {
                req.AddQueryParam(string.Format(PlayerIdParam, i+1), playerIds[i]);
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
            Attr attr = null;
            if(resp.Body != null)
            {
                attr = _parser.Parse(resp.Body);
            }
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatchInfoReceived(attr);
            }
        }

        void OnError(Error err)
        {
            for(var i=0; i< _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        public void NotifyResult(string matchId, AttrDic userData)
        {
            var req = CreateRequest(EndUri);
            req.AddQueryParam(string.Empty, matchId);
            _httpClient.Send(req);
            req = CreateRequest(ResultUri);
            req.AddQueryParam(UsersParam, userData);
            _httpClient.Send(req);
        }

        HttpRequest CreateRequest(string uri)
        {
            if(string.IsNullOrEmpty(BaseUrl))
            {
                throw new InvalidOperationException("Base url not configured.");
            }
            return new HttpRequest(BaseUrl + uri);
        }

    }
}
