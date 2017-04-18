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

        HttpRequest _notifyRequest = null;

        const string MatchMakingUri = "matchmaking";

        public string BaseUrl;

        public bool Enabled
        {
            get
            {
                return !string.IsNullOrEmpty(BaseUrl);
            }
        }

        public string Version { get; set; }

        public HttpRequest NotifyRequest
        {
            get
            {
                return _notifyRequest;
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
        const string EndUri = "/end_match";
        const string MatchIdParam = "match_id";
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

        public void LoadInfo(string matchId)
        {
            var req = CreateRequest(StringUtils.CombineUri(MatchMakingUri,InfoUri));
            req.AddQueryParam(MatchIdParam, matchId);
            if (!string.IsNullOrEmpty(Version))
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
            for(var i=0; i< _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        public void NotifyResults(string matchId, AttrDic userData)
        {
            _notifyRequest = CreateRequest(StringUtils.CombineUri(MatchMakingUri, EndUri));
            _notifyRequest.Method = HttpRequest.MethodType.POST;
            _notifyRequest.AddParam(MatchIdParam, matchId);
            _notifyRequest.AddParam(PlayersParam, userData);
            _httpClient.Send(_notifyRequest, (resp) => OnResultReceived(resp, userData));
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
            if(string.IsNullOrEmpty(BaseUrl))
            {
                throw new InvalidOperationException("Base url not configured.");
            }
            return new HttpRequest(StringUtils.CombineUri(BaseUrl, uri));
        }

    }
}
