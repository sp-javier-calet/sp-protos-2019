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
        const string MatchmakingStartMethodName = "matchmaking.match.start";
        const string MatchmakingStopMethodName = "matchmaking.match.cancel";
        const string MatchmakingGetMethodName = "matchmaking.match.find_active_match";

        const string RoomParameter = "room";
        const string UserIdParameter = "user_id";
        const string ConnectIdParameter = "connect_id";
        const string ExtraDataParameter = "extra_data";

        const string ErrorAttrKey = "error";
        const string StatusAttrKey = "status";
        const string WaitingStatus = "waiting";
        const string WaitingTimeAttrKey = "estimated_time";
        const string ResultAttrKey = "result";
        const string SuccessAttrKey = "success";
        const string MatchFoundAttrKey = "found";

        public string Room{ get; set; }

        public bool IsConnected
        {
            get
            {
                return _wamp.IsConnected;
            }
        }

        readonly List<IMatchmakingClientDelegate> _delegates;
        readonly List<IMatchmakingClientDelegate> _delegatesToAdd;
        readonly List<IMatchmakingClientDelegate> _delegatesToRemove;
        ConnectionManager _wamp;
        ILoginData _login;
        CallRequest _startRequest;
        CallRequest _stopRequest;
        string _connectionID = string.Empty;
        AttrDic _extraData;

        bool _searchForActiveMatchAndPersistOnReconnect;

        public WampMatchmakingClient(ConnectionManager wamp, ILoginData login)
        {
            _delegates = new List<IMatchmakingClientDelegate>();
            _delegatesToAdd = new List<IMatchmakingClientDelegate>();
            _delegatesToRemove = new List<IMatchmakingClientDelegate>();
            _wamp = wamp;
            _login = login;

            _wamp.OnNotificationReceived += OnWampNotificationReceived;
            _wamp.OnError += OnWampError;
        }

        void OnWampConnected()
        {
            _wamp.OnConnected -= OnWampConnected;
            CheckStart();
        }

        void OnConnectionClosed()
        {
            _wamp.OnClosed -= OnConnectionClosed;
            if(_startRequest != null)
            {
                if(_searchForActiveMatchAndPersistOnReconnect)
                {
                    _wamp.OnConnected += OnReconnected;
                }
                else
                {
                    DisposeStartRequest();
                }
            }
        }

        public void Start(AttrDic extraData, bool searchForActiveMatchAndPersistOnReconnect, string connectId)
        {
            _extraData = extraData;
            _searchForActiveMatchAndPersistOnReconnect = searchForActiveMatchAndPersistOnReconnect;
            _connectionID = connectId;

            DispatchOnStartEvent();

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

        void OnReconnected()
        {
            _wamp.OnConnected -= OnReconnected;
            CheckStart();
        }

        void CheckStart()
        {
            _wamp.OnClosed += OnConnectionClosed;

            DisposeStartRequest();

            if(_searchForActiveMatchAndPersistOnReconnect)
            {
                var dic = new AttrDic();
                dic.SetValue(UserIdParameter, _login.UserId.ToString());
                _wamp.Call(MatchmakingGetMethodName, Attr.InvalidList, dic, OnCheckStartResponse);
            }
            else
            {
                SearchOpponent();
            }
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
                    SearchOpponent();
                }
            }
            #if ADMIN_PANEL
            if(kwargs == null || attr == null)
                OnError(new Error("BAD MM RESPONSE: " + kwargs));
            #endif
        }

        void OnWampNotificationReceived(int type, string topic, AttrDic attr)
        {
            switch(type)
            {
            case NotificationType.MatchmakingWaitingTimeNotification:
                {
                    var waitTime = attr.GetValue(WaitingTimeAttrKey).ToInt();
                    DispatchOnWaitingEvent(waitTime);
                }
                break;
            case NotificationType.MatchmakingSuccessNotification:
                {
                    UnregisterEvents();
                    DisposeStartRequest();
                    DisposeStopRequest();

                    var match = new Match();
                    match.ParseAttrDic(attr);
                    DispatchOnMatchEvent(match);
                }
                break;
            case NotificationType.MatchmakingTimeoutNotification:
                {
                    UnregisterEvents();
                    DisposeStopRequest();
                    OnError(new Error(MatchmakingClientErrorCode.Timeout, new JsonAttrSerializer().SerializeString(attr)));
                }
                break;
            case NotificationType.MatchmakingCanceled:
                {
                    UnregisterEvents();
                    DispatchOnStoppedEvent(true);
                }
                break;
            }
        }

        void SearchOpponent()
        {
            DispatchOnSearchOpponentEvent();

            var kwargs = new AttrDic();
            if(_extraData != null && _extraData.Count > 0)
            {
                kwargs.Set(ExtraDataParameter, _extraData);
            }
            kwargs.SetValue(UserIdParameter, _login.UserId.ToString());
            if(!string.IsNullOrEmpty(Room))
            {
                kwargs.SetValue(RoomParameter, Room);
            }
            kwargs.SetValue(ConnectIdParameter, _connectionID);

            DisposeStartRequest();
            _startRequest = _wamp.Call(MatchmakingStartMethodName, Attr.InvalidList, kwargs, OnSearchOpponentResult);
        }

        void OnSearchOpponentResult(Error error, AttrList args, AttrDic kwargs)
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
                DispatchOnWaitingEvent(waitTime);
            }
            else if(attr != null && attr.ContainsKey(ErrorAttrKey))
            {
                OnError(new Error("Got error: " + attr.GetValue(ErrorAttrKey)));
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
                    OnError(new Error("Got error: " + attr.GetValue(ErrorAttrKey)));
                    return;
                }
                else if(attr.ContainsKey(SuccessAttrKey))
                {
                    bool stopped = attr.GetValue(SuccessAttrKey).ToBool();

                    if(stopped)
                    {
                        UnregisterEvents();
                        DisposeStartRequest();
                    }
                    DisposeStopRequest();
                    DispatchOnStoppedEvent(stopped);
                    return;
                }
            }
            OnError(new Error("Got unknown data: " + kwargs));
        }

        void UnregisterEvents()
        {
            _wamp.OnClosed -= OnConnectionClosed;
            _wamp.OnConnected -= OnWampConnected;
            _wamp.OnConnected -= OnReconnected;
        }

        public void Dispose()
        {
            _wamp.OnNotificationReceived -= OnWampNotificationReceived;
            _wamp.OnError -= OnWampError;
            UnregisterEvents();
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
            DispatchOnErrorEvent(err);
        }

        void SyncDelegates()
        {
            for(int i = 0; i < _delegatesToRemove.Count; ++i)
            {
                _delegates.Remove(_delegatesToRemove[i]);
            }
            _delegatesToRemove.Clear();

            _delegates.AddRange(_delegatesToAdd);
            _delegatesToAdd.Clear();
        }

        void DispatchOnStartEvent()
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStart();
            }
        }

        void DispatchOnSearchOpponentEvent()
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnSearchOpponent();
            }
        }

        void DispatchOnErrorEvent(Error err)
        {
            if(err == null)
            {
                return;
            }
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnError(err);
            }
        }

        void DispatchOnMatchEvent(Match match)
        {
            if(match == null)
            {
                return;
            }
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnMatched(match);
            }
        }

        void DispatchOnWaitingEvent(int time)
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnWaiting(time);
            }
        }

        void DispatchOnStoppedEvent(bool stopped)
        {
            SyncDelegates();
            for(var i = 0; i < _delegates.Count; i++)
            {
                _delegates[i].OnStopped(stopped);
            }
        }

        public void AddDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegatesToAdd.Add(dlg);
        }

        public void RemoveDelegate(IMatchmakingClientDelegate dlg)
        {
            _delegatesToRemove.Add(dlg);
        }
    }
}
