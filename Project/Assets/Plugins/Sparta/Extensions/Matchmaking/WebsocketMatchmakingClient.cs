using SocialPoint.Login;
using SocialPoint.Network;
using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Utils;
using System;
using System.Collections.Generic;

namespace SocialPoint.Matchmaking
{
    public class WebsocketMatchmakingClient : IMatchmakingClient, IDisposable, INetworkMessageReceiver, INetworkClientDelegate
    {
        enum Status
        {
            Idle,
            Connecting,
            Waiting,
            Finished
        }

        ILoginData _loginData;
        ulong _userId;
        IWebSocketClient _websocket;
        IAttrParser _parser;
        List<IMatchmakingClientDelegate> _delegates;
        Status _status;

        public string Room{ get; set; }

        const string UserParameter = "user_id";
        const string RoomParameter = "room";
        const string ExtraDataParameter = "extra_data";

        const string ErrorAttrKey = "error";
        const string StatusAttrKey = "status";
        const string WaitingStatus = "waiting";
        const string TimeoutStatus = "timeout";
        const string WaitingTimeAttrKey = "estimated_time";
        const string MatchIdAttrKey = "match_id";
        const string GameInfoAttrKey = "PlayerInfo";
        const string ServerInfoAttrKey = "server";
        const string PlayerIdAttrKey = "token";

        public WebsocketMatchmakingClient(ILoginData loginData, IWebSocketClient websocket)
        {
            _status = Status.Idle;
            _delegates = new List<IMatchmakingClientDelegate>();
            _loginData = loginData;
            _websocket = websocket;
            _parser = new JsonAttrParser();
            _websocket.AddDelegate(this);
            _websocket.RegisterReceiver(this);
        }

        public void Dispose()
        {
            _websocket.RemoveDelegate(this);
            _websocket.RegisterReceiver(null);
            Stop();
        }

        public void AddDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        public void Start(AttrDic extraData, bool searchForActiveMatch, string connectId)
        {
            _status = Status.Connecting;
            UpdateUrlParameters(extraData);
            _websocket.Connect();

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStart();
            }
        }

        public void Stop()
        {
            _status = Status.Finished;
            _websocket.Disconnect();

            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStopped(true);
            }
        }

        public void Clear()
        {
        }

        void UpdateUrlParameters(AttrDic extraData)
        {
            var urls = _websocket.Urls;
            for(var i = 0; i < urls.Length; ++i)
            {
                var baseUrl = urls[i];
                var req = new HttpRequest(baseUrl);
                if(_loginData != null)
                {
                    _userId = _loginData.UserId;
                }
                req.AddQueryParam(UserParameter, _userId.ToString());
                if(!string.IsNullOrEmpty(Room))
                {
                    req.AddQueryParam(RoomParameter, Room);
                }
                if(extraData != null && extraData.Count > 0)
                {
                    req.AddQueryParam(ExtraDataParameter, extraData);
                }
                urls[i] = req.Url.ToString();
            }

            _websocket.Urls = urls;
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            var body = reader.ReadString();
            var attr = _parser.ParseString(body).AsDic;
            if(attr.ContainsKey(StatusAttrKey))
            {
                var status = attr.GetValue(StatusAttrKey).ToString();
                if(status == TimeoutStatus)
                {

                    OnError(new Error(MatchmakingClientErrorCode.Timeout, "Match request timed out."));
                    return;
                }
                if(status != WaitingStatus)
                {
                    OnError(new Error("Got unknown status: " + data));
                    return;
                }
                _status = Status.Waiting;
                var waitTime = attr.GetValue(WaitingTimeAttrKey).ToInt();
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnWaiting(waitTime);
                }
            }
            else if(attr.ContainsKey(MatchIdAttrKey))
            {
                _websocket.Disconnect();
                _status = Status.Finished;
                var match = new Match {
                    Id = attr.GetValue(MatchIdAttrKey).ToString(),
                    GameInfo = attr.Get(GameInfoAttrKey),
                    ServerInfo = attr.Get(ServerInfoAttrKey),
                    PlayerId = attr.GetValue(PlayerIdAttrKey).ToString()
                };
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnMatched(match);
                }
            }
            else if(attr.ContainsKey(ErrorAttrKey))
            {
                OnError(new Error("Got error: " + attr.GetValue(ErrorAttrKey).ToString()));
            }
            else
            {
                OnError(new Error("Got unknown data: " + data));
            }
        }

        void INetworkClientDelegate.OnClientConnected()
        {
        }

        void INetworkClientDelegate.OnClientDisconnected()
        {
            if(_status != Status.Finished)
            {
                OnError(new Error("Disconnected without finding match."));
            }
            else
            {
                _status = Status.Idle;
            }
        }

        void INetworkClientDelegate.OnMessageReceived(NetworkMessageData data)
        {
        }

        void INetworkClientDelegate.OnNetworkError(Error err)
        {
            OnError(err);
        }

        void OnError(Error err)
        {
            _status = Status.Finished;
            _websocket.Disconnect();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }
    }

}
