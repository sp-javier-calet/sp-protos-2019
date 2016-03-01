using System;
using System.Collections;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Crash;
using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.ServerSync;
using SocialPoint.Utils;

namespace SocialPoint.ServerEvents
{
    public class SocialPointEventTracker : IEventTracker
    {
        public delegate void RequestSetupDelegate(HttpRequest req, string Uri);

        const string TrackingAuthorizedUri = "track";
        const string TrackingUnautorizedUri = "unauthorized/track";

        const string EventNameFunnel = "game.funnel";
        const string EventNameLevel = "game.level_up";
        const string EventNameGameStart = "game.start";
        const string EventNameGameOpen = "game.open";
        const string EventNameGameLoading = "game.loading";
        const string EventNameGameLoaded = "game.loaded";
        const string EventNameGameBackground = "game.background";
        const string EventNameGameRestart = "game.restart";
        const string EventNameResourceEarning = "economy.{0}_earning";
        const string EventNameResourceSpending = "economy.{0}_spending";

        const int SessionLostErrorStatusCode = 482;
        const int StartEventNum = 1;
        static readonly string[] DefaultUnauthorizedEvents = {
            EventNameGameStart,
            EventNameGameOpen,
            EventNameGameBackground,
            EventNameGameLoading,
            EventNameGameLoaded,
            EventNameGameRestart,
            "errors.*"
        };

        public const int DefaultMaxOutOfSyncInterval = 0;
        public const int DefaultSendInterval = 5;
        public const float DefaultTimeout = 30.0f;
        public const float DefaultBackoffMultiplier = 1.1f;

        public RequestSetupDelegate RequestSetup;

        public event EventDataSetupDelegate DataSetup;
        public event Action SyncChange;
        public event EventTrackerErrorDelegate GeneralError;

        public int MaxOutOfSyncInterval = DefaultMaxOutOfSyncInterval;
        public int SendInterval = DefaultSendInterval;
        public float Timeout = DefaultTimeout;
        public float BackoffMultiplier = DefaultBackoffMultiplier;
        public List<string> UnauthorizedEvents;

        public IHttpClient HttpClient;
        public IDeviceInfo DeviceInfo;
        public ICommandQueue CommandQueue;

        List<Event> _pendingEvents;
        ICoroutineRunner _runner;
        IEnumerator _updateCoroutine;
        bool _sending;
        int _lastEventNum;
        long _lastSendTimestamp;
        bool _synced;
        bool _sendPending;
        long _syncTimestamp;
        float _currentTimeout;
        float _currentSendInterval;
        bool _gameStartTracked;
        bool _gameLoadedTracked;
        IHttpConnection _httpConn;

        public DateTime SyncTime
        {
            get
            {
                return TimeUtils.GetTime(SyncTimestamp);
            }
        }

        public long SyncTimestamp
        {
            get
            {
                return _syncTimestamp;
            }
        }

        IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }

            set
            {
                if(_appEvents != null)
                {
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                if(_appEvents != null)
                {
                    ConnectAppEvents(_appEvents);
                }
            }
        }

        public BreadcrumbManager BreadcrumbManager
        {
            get;
            set;
        }

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.OpenedFromSource += OnOpenedFromSource;
            appEvents.WillGoBackground.Add(0, OnAppWillGoBackground);
            appEvents.WillGoBackground.Add(-100, OnAppGoBackground);
            appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
            appEvents.GameWillRestart.Add(0, OnGameWillRestart);
            appEvents.GameWillRestart.Add(-100, OnGameRestart);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.OpenedFromSource -= OnOpenedFromSource;
            appEvents.WillGoBackground.Remove(OnAppWillGoBackground);
            appEvents.WillGoBackground.Remove(OnAppGoBackground);
            appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
            appEvents.GameWillRestart.Remove(OnGameWillRestart);
            appEvents.GameWillRestart.Remove(OnGameRestart);
        }

        void OnGameWillRestart()
        {
            TrackGameRestart();
        }

        void OnGameRestart()
        {
            if(Running)
            {
                Send();
                Stop();
                Reset();
            }
        }

        void OnGameWasLoaded()
        {
            if(!Running)
            {
                Start();
            }
            TrackGameLoaded();
        }

        void OnOpenedFromSource(AppSource source)
        {
            TrackGameOpen(source);
        }

        void OnAppWillGoBackground()
        {
            TrackGameBackground();
        }

        void OnAppGoBackground()
        {
            Send();
        }

        #endregion

        public SocialPointEventTracker(ICoroutineRunner runner, bool autoStart = true)
        {
            _runner = runner;
            UnauthorizedEvents = new List<string>(DefaultUnauthorizedEvents);
            _pendingEvents = new List<Event>();
            Reset();
            SetStartValues();
            if(autoStart)
            {
                Start();
            }
        }

        public void Reset()
        {
            _lastEventNum = StartEventNum;
            foreach(var ev in _pendingEvents)
            {
                ev.Num = Event.NoNum;
            }
            if(_httpConn != null)
            {
                _httpConn.Release();
                _httpConn = null;
            }
        }

        void SetStartValues()
        {
            _lastSendTimestamp = CurrentTimestamp;
            _currentTimeout = Timeout;
            _currentSendInterval = SendInterval;
            _syncTimestamp = CurrentTimestamp;
            _synced = true;
            _sending = false;
            _sendPending = false;
        }

        void OnTimeOffsetChanged(TimeSpan diff)
        {
            var dt = (long)diff.TotalSeconds;
            _lastSendTimestamp += dt;
            _syncTimestamp += dt;
        }

        void TrackEventByRequest(string eventName, bool isUrgent, AttrDic data, ErrorDelegate del = null)
        {
            if(data == null)
            {
                data = new AttrDic();
            }
            if(DataSetup != null)
            {
                DataSetup(data);
            }
            var e = new Event(eventName, data, del);
            if(isUrgent)
            {
                e.Retries = 0;
            }
            _pendingEvents.Add(e);
        }


        void TrackEventByCommand(string eventName, AttrDic data, ErrorDelegate del = null)
        {
            if(data == null)
            {
                data = new AttrDic();
            }
            if(DataSetup != null)
            {
                DataSetup(data);
            }
            AddHardwareData(data);
            var eventCommand = new EventCommand(eventName, data);
            CommandQueue.Add(eventCommand, (attr, err) => del(err));
        }

        public void TrackSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            TrackEventByRequest(eventName, false, data, del);
        }

        public void TrackUrgentSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            TrackEventByRequest(eventName, true, data, del);
            Send();
        }

        public void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            if(BreadcrumbManager != null)
            {
                BreadcrumbManager.Log(string.Format("{0} {1}", eventName, data));
            }
            if(CommandQueue == null || IsEventUnauthorized(eventName))
            {
                TrackEventByRequest(eventName, false, data, del);
            }
            else
            {
                TrackEventByCommand(eventName, data, del);
            }
        }

        long CurrentTimestamp
        {
            get
            {
                // do not use 'TimeUtils.Timestamp' nor 'TimeUtils.Now' because they apply a server offset
                return TimeUtils.GetTimestamp(DateTime.Now);
            }
        }

        long CurrentSyncedTimestamp
        {
            get
            {
                // dependant on server offset
                return TimeUtils.Timestamp;
            }
        }

        int CurrentSyncedOffset
        {
            get
            {
                return (int)TimeUtils.Offset.TotalSeconds;
            }
        }

        public void Start()
        {
            if(_updateCoroutine == null)
            {
                SetStartValues();
                _updateCoroutine = UpdateCoroutine();
                _runner.StartCoroutine(_updateCoroutine);
            }
            TrackGameStart();
        }

        public void Stop()
        {
            if(_updateCoroutine != null)
            {
                _runner.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        bool Running
        {
            get
            {
                return _updateCoroutine != null;
            }
        }

        virtual public void Dispose()
        {
            Stop();
            Reset();
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
        }

        IEnumerator UpdateCoroutine()
        {
            while(true)
            {
                Update();
                yield return true;
            }
        }

        void Update()
        {
            if(_lastSendTimestamp + (long)_currentSendInterval < CurrentTimestamp)
            {
                Send();
            }
        }

        public bool Send()
        {
            if(!_sending)
            {
                _sending = true;
                int count = 2;
                Action step = () => {
                    count--;
                    if(count == 0)
                    {
                        AfterSend();
                    }
                };
                DoSend(false, step);
                DoSend(true, step);
                return true;
            }
            else
            {
                _sendPending = true;
                return false;
            }
        }

        void AfterSend()
        {
            _sending = false;
            if(_sendPending)
            {
                _sendPending = false;
                Send();
            }
        }

        void AddHardwareData(AttrDic data)
        {
            var client = data.ContainsKey("client") ? data.Get("client").AssertDic : new AttrDic();
            data.Set("client", client);
            var mobile = new AttrDic();
            client.Set("mobile", mobile);
            mobile.SetValue("uid", DeviceInfo.Uid);
            mobile.SetValue("device", DeviceInfo.String);
            mobile.SetValue("language", DeviceInfo.AppInfo.Language);
            mobile.SetValue("country", DeviceInfo.AppInfo.Country);
            mobile.SetValue("adid", DeviceInfo.AdvertisingId);
            mobile.SetValue("adid_enabled", DeviceInfo.AdvertisingIdEnabled);
            mobile.SetValue("rooted", DeviceInfo.Rooted);
            mobile.SetValue("os", DeviceInfo.PlatformVersion);
            #if ADMIN_PANEL
            mobile.SetValue("admin_panel", true);
            #endif
        }

        bool IsEventUnauthorized(string evName)
        {
            foreach(var pattern in UnauthorizedEvents)
            {
                if(StringUtils.GlobMatch(pattern, evName))
                {
                    return true;
                }
            }
            return false;
        }

        void DoSend(bool auth, Action finish = null)
        {
            var evs = new AttrList();
            var sentEvents = new List<Event>();
            foreach(var ev in _pendingEvents)
            {
                var evauth = !IsEventUnauthorized(ev.Name);
                if(auth == evauth)
                {
                    if(auth && ev.Num == Event.NoNum)
                    {
                        ev.Num = _lastEventNum;
                        _lastEventNum++;
                    }
                    sentEvents.Add(ev);
                    ev.OnStart();
                    evs.Add(ev.ToAttr());
                }
            }

            if(sentEvents.Count == 0)
            {
                if(finish != null)
                {
                    finish();
                }
                return;
            }

            var data = new AttrDic();
            var common = new AttrDic();
            data.Set("common", common);
            common.SetValue("plat", DeviceInfo.Platform);
            var version = DeviceInfo.AppInfo.ShortVersion + "-" + DeviceInfo.AppInfo.Version;
            common.SetValue("ver", version);
            common.SetValue("ts", CurrentSyncedTimestamp);
            common.SetValue("dts", CurrentSyncedOffset);
            AddHardwareData(common);
            data.Set("events", evs);
            
            SendData(new JsonAttrSerializer().Serialize(data), auth, sentEvents, finish);
        }

        void SendData(byte[] data, bool auth, List<Event> sentEvents, Action finish = null)
        {
            var req = new HttpRequest();
            var uri = auth ? TrackingAuthorizedUri : TrackingUnautorizedUri;
            if(RequestSetup != null)
            {
                try
                {
                    RequestSetup(req, uri);
                }
                catch(Exception e)
                {
                    CatchException(e);
                }
            }
            req.Body = data;
            if(Math.Abs(req.Timeout) < Single.Epsilon)
            {
                req.Timeout = Timeout;
            }
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.CompressBody = true;
            _httpConn = HttpClient.Send(req, resp => {
                OnHttpResponse(resp, sentEvents);
                if(finish != null)
                {
                    finish();
                }
            });
            if(!auth)
            {
                // do not cancel if unauthorized
                _httpConn = null;
            }
            _lastSendTimestamp = CurrentTimestamp;
        }

        bool CheckSync(HttpResponse resp)
        {
            bool oldconn = _synced;
            _synced = !resp.HasRecoverableError;
            if(oldconn != _synced)
            {
                _syncTimestamp = CurrentTimestamp;
                if(SyncChange != null)
                {
                    SyncChange();
                }
            }
            
            if(!_synced && MaxOutOfSyncInterval > 0 && _syncTimestamp + MaxOutOfSyncInterval < CurrentTimestamp && GeneralError != null)
            {
                GeneralError(EventTrackerErrorType.OutOfSync, new Error("Too much time passed without sync."));
            }
            
            return _synced;
        }

        void OnHttpResponse(HttpResponse resp, List<Event> sentEvents)
        {
            bool synced = CheckSync(resp);
            ApplyBackoff(synced);
            var error = resp.Error;
            if(resp.HasError)
            {
                try
                {
                    var data = new JsonAttrParser().Parse(resp.Body).AsDic;
                    var dataErr = AttrUtils.GetError(data);
                    if(dataErr != null)
                    {
                        error = dataErr;
                    }
                }
                catch(Exception)
                {
                }
            }
            foreach(var ev in sentEvents)
            {
                if(synced || ev != null && !ev.CanRetry)
                {
                    if(ev != null && ev.ResponseDelegate != null)
                    {
                        ev.ResponseDelegate(error);
                    }
                    _pendingEvents.Remove(ev);
                }
            }
            if(synced && error != null && error.HasError && GeneralError != null)
            {
                if(error.Code == SessionLostErrorStatusCode)
                {
                    GeneralError(EventTrackerErrorType.SessionLost, error);
                }
                else
                {
                    GeneralError(EventTrackerErrorType.HttpResponse, error);
                }
            }
        }

        void ApplyBackoff(bool success)
        {
            if(success)
            {
                var transTime = CurrentTimestamp - _lastSendTimestamp;
                _currentTimeout = Math.Max(transTime, Timeout);
            }
            else
            {
                _currentTimeout *= BackoffMultiplier;
            }
        }


        public void TrackFunnel(FunnelOperation op)
        {
            var data = op.AdditionalData ?? new AttrDic();
            var funnel = new AttrDic();
            data.Set("funnel", funnel);
            funnel.SetValue("step", op.Step);
            funnel.SetValue("type", op.Type);
            funnel.SetValue("auto_completed", op.AutoCompleted);

            if(op.System)
            {
                TrackSystemEvent(EventNameFunnel, data);
            }
            else
            {
                TrackEvent(EventNameFunnel, data);
            }
        }

        public void TrackLevelUp(int lvl, AttrDic data = null)
        {
            if(data == null)
            {
                data = new AttrDic();
            }
            var game = new AttrDic();
            data.Set("game", game);
            var basic = new AttrDic();
            game.Set("basic", basic);
            basic.SetValue("level", lvl);
            TrackEvent(EventNameLevel, data);
        }

        public void TrackGameStart()
        {
            if(!_gameStartTracked)
            {
                _gameStartTracked = true;
                TrackSystemEvent(EventNameGameStart);
            }
        }

        void TrackGameLoaded()
        {
            if(!_gameLoadedTracked)
            {
                _gameLoadedTracked = true;
                TrackSystemEvent(EventNameGameLoaded);
            }
        }

        void TrackGameRestart()
        {
            TrackSystemEvent(EventNameGameRestart);
        }

        public void TrackGameBackground()
        {
            TrackSystemEvent(EventNameGameBackground);
        }

        void TrackGameOpen(AppSource source)
        {
            var data = new AttrDic();
            var origin = new AttrDic();
            data.Set("origin", origin);

            string sourceType = "app";
            string detail = "";

            if(source.IsCustomScheme)
            {
                sourceType = source.Scheme;
                detail = source.Query;
            }
            else if(!source.Empty)
            {
                sourceType = "url";
                detail = source.ToString();
            }

            origin.SetValue("source", sourceType);
            origin.SetValue("detail", detail);

            TrackSystemEvent(EventNameGameOpen, data);
        }

        public void TrackResource(ResourceOperation op)
        {
            string name;
            name = op.Amount >= 0 ? EventNameResourceEarning : EventNameResourceSpending;
            name = string.Format(name, op.Resource);

            var data = op.AdditionalData ?? new AttrDic();
            var operation = new AttrDic();
            data.Set("operation", operation);
            operation.SetValue("category", op.Category);
            operation.SetValue("subcategory", op.Subcategory);
            operation.SetValue("amount", Math.Abs(op.Amount));
            operation.SetValue("potential_amount", Math.Abs(op.PotentialAmount));
            operation.SetValue("lost_amount", Math.Abs(op.LostAmount));
            operation.SetValue("type", op.Resource);

            TrackEvent(name, data);
        }

        void CatchException(Exception e)
        {
            DebugUtils.LogException(e);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            if(GeneralError != null)
            {
                GeneralError(EventTrackerErrorType.Exception, new Error(e.ToString()));
            }
            #endif
        }
    }
}
