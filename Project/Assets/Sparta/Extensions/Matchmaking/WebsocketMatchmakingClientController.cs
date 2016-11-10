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
    public class WebsocketMatchmakingClientController : IMatchmakingClientController, IDisposable, INetworkMessageReceiver, INetworkClientDelegate
    {
        enum Status
        {
            Idle,
            Connecting,
            Waiting,
            Finished
        };

        ILoginData _loginData;
        ulong _userId;
        IWebSocketClient _websocket;
        string _baseUrl;
        IAttrParser _parser;
        List<IMatchmakingClientDelegate> _delegates;
        Status _status;

        const string RoomParameter = "room";
        const string UserParameter = "user_id";

        const string StatusAttrKey = "status";
        const string WaitingStatus = "waiting";
        const string WaitingTimeAttrKey = "estimated_time";
        const string MatchIdAttrKey = "match_id";
        const string MatchInfoAttrKey = "PlayerInfo";
        const string PlayerIdAttrKey = "token";

        public WebsocketMatchmakingClientController(ILoginData loginData, IWebSocketClient websocket)
        {
            _status = Status.Idle;
            _delegates = new List<IMatchmakingClientDelegate>();
            _loginData = loginData;
            _websocket = websocket;
            _baseUrl = websocket.Url;
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

        public void Start()
        {
            var req = new HttpRequest(_baseUrl);
            if(_loginData != null)
            {
                _userId = _loginData.UserId;
            }
            _status = Status.Connecting;
            req.AddQueryParam(UserParameter, _userId.ToString());
            _websocket.Url = req.Url.ToString();
            _websocket.Connect();
        }

        public void Stop()
        {
            _status = Status.Finished;
            _websocket.Disconnect();
        }

        public void Clear()
        {
        }

        void INetworkMessageReceiver.OnMessageReceived(NetworkMessageData data, IReader reader)
        {
            var body = reader.ReadString();
            var attr = _parser.ParseString(body).AsDic;
            if(attr.ContainsKey(StatusAttrKey))
            {
                var status = attr.GetValue(StatusAttrKey).ToString();
                if(status != WaitingStatus)
                {
                    OnError(new Error("Got unknown status: "+data));
                    return;
                }
                _status = Status.Waiting;
                var waitTime = attr.GetValue(WaitingTimeAttrKey).ToInt();
                for(var i=0; i<_delegates.Count; i++)
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
                    Info = attr.Get(MatchInfoAttrKey),
                    PlayerId = attr.GetValue(PlayerIdAttrKey).ToString()
                };
                for(var i=0; i<_delegates.Count; i++)
                {
                    _delegates[i].OnMatched(match);
                }
            }
            else
            {
                OnError(new Error("Got unknown data: "+data));
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