using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Connection;
using SocialPoint.Login;
using SocialPoint.WAMP.Caller;

namespace SocialPoint.Matchmaking
{
    public class WampMatchmakingClient : IMatchmakingClient, IDisposable
    {
        List<IMatchmakingClientDelegate> _delegates;
        ConnectionManager _wamp;
        ILoginData _login;
        CallRequest _req;

        const string RoomParameter = "room";
        const string UserIdParameter = "user_id";

        const string ErrorAttrKey = "error";
        const string StatusAttrKey = "status";
        const string WaitingStatus = "waiting";
        const string WaitingTimeAttrKey = "estimated_time";
        const string ResultAttrKey = "result";

        const int SuccessNotification = 502;
        const int TimeoutNotification = 503;

        public string Room{ get; set; }

        public WampMatchmakingClient(ILoginData login, ConnectionManager wamp)
        {
            _delegates = new List<IMatchmakingClientDelegate>();
            _wamp = wamp;
            _login = login;
        }

        public void Dispose()
        {
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

        const string MethodName = "matchmaking.match.start";

        public void Start()
        {
            _wamp.OnError += OnWampError;
            _wamp.OnNotificationReceived += OnWampNotificationReceived;
            if(!_wamp.IsConnected)
            {
                _wamp.OnConnected += OnWampConnected;
                _wamp.Connect();
            }
            else
            {
                DoStart();
            }
        }

        void OnWampConnected()
        {
            _wamp.OnConnected -= OnWampConnected;
            DoStart();
        }

        void DoStart()
        {
            var kwargs = new AttrDic();
            if(_login != null)
            {
                kwargs.SetValue(UserIdParameter, _login.UserId.ToString());
            }
            if(!string.IsNullOrEmpty(Room))
            {
                kwargs.SetValue(RoomParameter, Room);
            }
            if(_req != null)
            {
                _req.Dispose();
            }
            _req = _wamp.Call(MethodName, Attr.InvalidList, kwargs, OnStartResult);
        }

        void OnStartResult(Error error, AttrList args, AttrDic kwargs)
        {
            if(!Error.IsNullOrEmpty(error))
            {
                OnError(error);
                return;
            }
            AttrDic attr = null;
            if(kwargs != null && kwargs.ContainsKey(ResultAttrKey))
            {
                attr = kwargs.Get(ResultAttrKey).AsDic;
            }
            if(attr != null && attr.ContainsKey(StatusAttrKey))
            {
                var status = attr.GetValue(StatusAttrKey).ToString();
                if(status != WaitingStatus)
                {
                    OnError(new Error("Got unknown status: " + status));
                    return;
                }
                var waitTime = attr.GetValue(WaitingTimeAttrKey).ToInt();
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnWaiting(waitTime);
                }
            }
            else if(attr != null && attr.ContainsKey(ErrorAttrKey))
            {
                OnError(new Error("Got error: "+attr.GetValue(ErrorAttrKey).ToString()));
            }
            else
            {
                OnError(new Error("Got unknown data: "+kwargs));
            }
        }

        void OnWampNotificationReceived(int type, string topic, AttrDic attr)
        {
            if(type == NotificationType.MatchmakingSuccessNotification)
            {
                var match = new Match();
                match.ParseAttrDic(attr);
                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnMatched(match, false);
                }
            }
            else if(type == TimeoutNotification)
            {
                OnError(new Error(MatchmakingClientErrorCode.Timeout, "Timeout"));
            }
        }

        public void Stop()
        {
            _wamp.OnError -= OnWampError;
            _wamp.OnNotificationReceived -= OnWampNotificationReceived;
            if(_req != null)
            {
                _req.Dispose();
                _req = null;
            }
        }

        public void Clear()
        {
        }

        void OnWampError(Error err)
        {
            OnError(err);
        }

        void OnError(Error err)
        {
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }
    }

}