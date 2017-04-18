using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Connection;
using SocialPoint.Login;
using SocialPoint.WAMP.Caller;
using DS.Common.Events;

namespace SocialPoint.Matchmaking
{
    public class WampMatchmakingClient : IMatchmakingClient, IDisposable
    {
        readonly List<IMatchmakingClientDelegate> _delegates;
        ConnectionManager _wamp;
        ILoginData _login;
        CallRequest _startRequest;
        CallRequest _stopRequest;
        CallRequest _reconnectRequest;

        const string RoomParameter = "room";
        const string UserIdParameter = "user_id";

        const string ErrorAttrKey = "error";
        const string StatusAttrKey = "status";
        const string WaitingStatus = "waiting";
        const string WaitingTimeAttrKey = "estimated_time";
        const string ResultAttrKey = "result";
        const string SuccessAttrKey = "success";
        const string MatchFoundAttrKey = "found";

        const int SuccessNotification = 502;
        const int TimeoutNotification = 503;

        public string Room{ get; set; }

        public bool IsConnected
        {
            get
            {
                return _wamp.IsConnected;
            }
        }

        bool _reconnected;
        bool _waitingReconnectedBattle;

        public WampMatchmakingClient(ILoginData login, ConnectionManager wamp)
        {
            _delegates = new List<IMatchmakingClientDelegate>();
            _wamp = wamp;
            _login = login;
            _wamp.OnClosed += OnConnectionClosed;
            _reconnected = false;
        }

        void OnConnectionClosed()
        {
            _wamp.OnConnected += OnReconnected;
        }

        void OnReconnected()
        {
            _wamp.OnConnected -= OnReconnected;
            if(_reconnectRequest != null)
            {
                _reconnectRequest.Dispose();
            }
            var dic = new AttrDic();
            if(_login != null)
            {
                dic.SetValue(UserIdParameter, _login.UserId.ToString());
            }
            _reconnectRequest = _wamp.Call(MatchmakingGetMethodName, Attr.InvalidList, dic, OnReconnectResult);
        }

        public void AddDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Add(dlg);
        }

        public void RemoveDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegates.Remove(dlg);
        }

        const string MatchmakingStartMethodName = "matchmaking.match.start";
        const string MatchmakingStopMethodName = "matchmaking.match.stop";
        const string MatchmakingGetMethodName = "matchmaking.match.find_active_match";

        public void Start()
        {
            _reconnected = false;
            _wamp.OnError += OnWampError;
            _wamp.OnNotificationReceived += OnWampNotificationReceived;
            if(!_wamp.IsConnected)
            {
                _wamp.OnConnected += OnWampConnected;
                _wamp.Connect();
            }
            else
            {
                CheckStart();
            }
        }

        void OnWampConnected()
        {
            _wamp.OnConnected -= OnWampConnected;
            CheckStart();
        }

        void CheckStart()
        {
            if(_reconnectRequest != null)
            {
                _reconnectRequest.Dispose();
            }
            var dic = new AttrDic();
            if(_login != null)
            {
                dic.SetValue(UserIdParameter, _login.UserId.ToString());
            }
            _reconnectRequest = _wamp.Call(MatchmakingGetMethodName, Attr.InvalidList, dic, OnCheckStartResponse);
        }

        void OnCheckStartResponse(Error error, AttrList args, AttrDic kwargs)
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
            if(attr != null)
            { 
                bool found = attr.GetValue(MatchFoundAttrKey).ToBool();
                if(!found)
                {
                    DoStart();
                }
            }
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
            if(_startRequest != null)
            {
                _startRequest.Dispose();
            }
            _startRequest = _wamp.Call(MatchmakingStartMethodName, Attr.InvalidList, kwargs, OnStartResult);
            _waitingReconnectedBattle = true;
            ServiceLocator.EventDispatcher.Raise(new MatckmakerStateChangedEvent(1010, "mm_request_ok"));
        }

        void OnReconnectResult(Error error, AttrList args, AttrDic kwargs)
        {
            if(!Error.IsNullOrEmpty(error))
            {
                OnError(error);
                return;
            }
            if(_waitingReconnectedBattle)
            {
                _reconnected = true;
                Stop();
            }
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
                ServiceLocator.EventDispatcher.Raise(new MatckmakerStateChangedEvent(1020, "waiting_time_received"));
            }
            else if(attr != null && attr.ContainsKey(ErrorAttrKey))
            {
                OnError(new Error("Got error: " + attr.GetValue(ErrorAttrKey).ToString()));
            }
            else
            {
                OnError(new Error("Got unknown data: " + kwargs));
            }
        }

        public void Stop()
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
            if(_stopRequest != null)
            {
                _stopRequest.Dispose();
            }
            _stopRequest = _wamp.Call(MatchmakingStopMethodName, Attr.InvalidList, kwargs, OnStopResult);
            ServiceLocator.EventDispatcher.Raise(new MatckmakerStateChangedEvent(1001, "initializing_cancel"));
        }

        void OnStopResult(Error error, AttrList args, AttrDic kwargs)
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
            if(attr != null)
            {
                if(attr.ContainsKey(ErrorAttrKey))
                {
                    OnError(new Error("Got error: "+attr.GetValue(ErrorAttrKey).ToString()));
                    return;
                }
                else if(attr.ContainsKey(SuccessAttrKey))
                {
                    bool stopped = attr.GetValue(SuccessAttrKey).ToBool();

                    if(stopped)
                    {
                        DisposeStartRequest();
                    }
                    DisposeStopRequest();
                    if(_reconnected)
                    {
                        _wamp.OnError -= OnWampError;
                        _wamp.OnNotificationReceived -= OnWampNotificationReceived;
                        _wamp.OnConnected -= OnWampConnected;
                        Start();
                    }
                    else
                    {
                        for(var i = 0; i < _delegates.Count; i++)
                        {
                            _delegates[i].OnStopped(stopped);
                        }
                    }

                    return;
                }
            }
            OnError(new Error("Got unknown data: "+kwargs));
        }

        void OnWampNotificationReceived(int type, string topic, AttrDic attr)
        {
            if(type == NotificationType.MatchmakingSuccessNotification)
            {
                _waitingReconnectedBattle = false;
                DisposeStopRequest();

                var match = new Match();
                match.ParseAttrDic(attr);

                for(var i = 0; i < _delegates.Count; i++)
                {
                    _delegates[i].OnMatched(match);
                }
                ServiceLocator.EventDispatcher.Raise(new MatckmakerStateChangedEvent(1030, "match_response_ok_real"));
            }
            else if(type == TimeoutNotification)
            {
                ServiceLocator.EventDispatcher.Raise(new MatckmakerStateChangedEvent(1031, "match_response_ok_ai"));
                OnError(new Error(MatchmakingClientErrorCode.Timeout, new JsonAttrSerializer().SerializeString( attr)));
            }
        }

        public void Dispose()
        {
            _wamp.OnError -= OnWampError;
            _wamp.OnNotificationReceived -= OnWampNotificationReceived;
            _wamp.OnClosed -= OnConnectionClosed;
            DisposeStartRequest();
            DisposeStopRequest();
        }

        void DisposeStartRequest()
        {
            if(_startRequest != null)
            {
                _startRequest.Dispose();
                _startRequest = null;
            }
        }

        void DisposeStopRequest()
        {
            if(_stopRequest != null)
            {
                _stopRequest.Dispose();
                _stopRequest = null;
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
