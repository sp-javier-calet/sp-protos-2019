using System;
using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Crash;
using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Utils;
using System.Text;

namespace SocialPoint.ServerEvents
{
    public sealed class SocialPointEventTracker : IEventTracker, IUpdateable
    {
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

        public ILoginData LoginData;

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
        IUpdateScheduler _updateScheduler;
        bool _sending;
        int _lastEventNum;
        long _lastSendTimestamp;
        bool _synced;
        bool _sendPending;
        long _syncTimestamp;
        float _currentTimeout;
        bool _gameStartTracked;
        bool _gameLoadedTracked;
        bool _running;
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

        public IBreadcrumbManager BreadcrumbManager
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
            if(_running)
            {
                Send();
                Stop();
                Reset();
            }
        }

        void OnGameWasLoaded()
        {
            if(!_running)
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

        public SocialPointEventTracker(IUpdateScheduler updateScheduler, bool autoStart = true)
        {
            _updateScheduler = updateScheduler;
            UnauthorizedEvents = new List<string>(DefaultUnauthorizedEvents);
            _pendingEvents = new List<Event>();
            _running = false;
            Reset();
            if(autoStart)
            {
                Start();
            }
        }

        public void Reset()
        {
            _lastEventNum = StartEventNum;
            for(int i = 0, _pendingEventsCount = _pendingEvents.Count; i < _pendingEventsCount; i++)
            {
                var ev = _pendingEvents[i];
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
            if(del == null)
            {
                CommandQueue.Add(eventCommand);
            }
            else
            {
                CommandQueue.Add(eventCommand, (attr, err) => del(err));
            }
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
                StringBuilder stringBuilder = StringUtils.StartBuilder();
                stringBuilder.AppendFormat(eventName, data);

                BreadcrumbManager.Log(StringUtils.FinishBuilder(stringBuilder));
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

        static long CurrentTimestamp
        {
            get
            {
                // do not use 'TimeUtils.Timestamp' nor 'TimeUtils.Now' because they apply a server offset
                return TimeUtils.GetTimestamp(DateTime.Now);
            }
        }

        static long CurrentSyncedTimestamp
        {
            get
            {
                // dependant on server offset
                return TimeUtils.Timestamp;
            }
        }

        static int CurrentSyncedOffset
        {
            get
            {
                return (int)TimeUtils.Offset.TotalSeconds;
            }
        }

        public void Start()
        {
            if(_running)
            {
                return;
            }

            SetStartValues();

            if(_updateScheduler != null)
            {
                _updateScheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, SendInterval);
                _running = true;
            }

            TrackGameStart();
        }

        public void Stop()
        {
            if(_updateScheduler != null)
            {
                _updateScheduler.Remove(this);
                _running = false;
            }
        }

        public void Dispose()
        {
            Stop();
            Reset();
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
        }

        #region IUpdateable implementation

        public void Update()
        {
            Send();
        }

        #endregion

        public bool Send()
        {
            if(!_sending)
            {
                _sending = true;

                var steps = new StepCallbackBuilder(AfterSend);

                DoSend(false, steps.Add());
                DoSend(true, steps.Add());

                steps.Ready();

                return true;
            }
            _sendPending = true;
            return false;
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
            mobile.SetValue("version", DeviceInfo.AppInfo.Version);
            #if ADMIN_PANEL
            mobile.SetValue("admin_panel", true);
            #endif
        }

        bool IsEventUnauthorized(string evName)
        {
            for(int i = 0, UnauthorizedEventsCount = UnauthorizedEvents.Count; i < UnauthorizedEventsCount; i++)
            {
                var pattern = UnauthorizedEvents[i];
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
            for(int i = 0, _pendingEventsCount = _pendingEvents.Count; i < _pendingEventsCount; i++)
            {
                var ev = _pendingEvents[i];
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
            if(auth && (LoginData == null || string.IsNullOrEmpty(LoginData.SessionId)))
            {
                // no session, we wait
                if(finish != null)
                {
                    finish();
                }
                return;
            }

            var req = new HttpRequest();

            if(LoginData != null)
            {
                try
                {
                    var uri = auth ? TrackingAuthorizedUri : TrackingUnautorizedUri;
                    LoginData.SetupHttpRequest(req, uri);
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
            for(int i = 0, sentEventsCount = sentEvents.Count; i < sentEventsCount; i++)
            {
                var ev = sentEvents[i];
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

            if(source.IsCustomScheme && !source.IsOpenFromIcon)
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

            StringBuilder stringBuilder = StringUtils.StartBuilder();
            stringBuilder.AppendFormat(name, op.ResourceName);
            name = StringUtils.FinishBuilder(stringBuilder);

            var data = op.AdditionalData ?? new AttrDic();
            var operation = op.AdditionalData != null && op.AdditionalData.ContainsKey("operation") ? op.AdditionalData["operation"].AsDic : new AttrDic();
            data.Set("operation", operation);
            operation.SetValue("category", op.Category);
            operation.SetValue("subcategory", op.Subcategory);
            operation.SetValue("amount", Math.Abs(op.Amount));
            operation.SetValue("potential_amount", Math.Abs(op.PotentialAmount));
            operation.SetValue("lost_amount", Math.Abs(op.LostAmount));
            operation.SetValue("type", op.ResourceName);

            var item = data.ContainsKey("item") ? data["item"].AsDic : new AttrDic();
            data.Set("item", item);
            item.SetValue("reference", op.ItemId);

            TrackEvent(name, data);
        }

        void CatchException(Exception e)
        {
            Log.x(e);
            #if UNITY_EDITOR
            DebugUtils.Stop();
            #else
            if(GeneralError != null)
            {
                GeneralError(EventTrackerErrorType.Exception, new Error(e.ToString()));
            }
            #endif
        }
    }
}
