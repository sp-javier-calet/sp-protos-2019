using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.AppEvents;

using SocialPoint.ServerSync;

namespace SocialPoint.Events
{
    public class SocialPointEventTracker : IEventTracker
    {
        public delegate void RequestSetupDelegate(HttpRequest req, string Uri);

        public delegate string GetSessionIdDelegate();

        private const string TrackingAuthorizedUri = "track";
        private const string TrackingUnautorizedUri = "unauthorized/track";
        
        private const string EventNameFunnel = "game.funnel";
        private const string EventNameLevel = "game.level_up";
        private const string EventNameGameOpen = "game.open";
        private const string EventNameGameStart = "game.start";
        private const string EventNameLoading = "game.loading";
        private const string EventNameGameBackground = "game.background";
        private const string EventNameResourceEarning = "economy.{0}_earning";
        private const string EventNameResourceSpending = "economy.{0}_spending";
        private const string EventNameMonetizationTransactionStart = "monetization.transaction_start";

        private const int MinServerErrorStatusCode = 500;
        private const int SessionLostErrorStatusCode = 482;
        private const int StartEventNum = 1;
        private static readonly string[] DefaultUnauthorizedEvents = {
            EventNameGameStart,
            EventNameGameOpen,
            EventNameGameBackground,
            EventNameLoading,
            "errors.*"
        };

        public const int DefaultMaxOutOfSyncInterval = 0;
        public const int DefaultSendInterval = 5;
        public const float DefaultTimeout = 30.0f;
        public const float DefaultBackoffMultiplier = 1.1f;

        public RequestSetupDelegate RequestSetup;
        public GetSessionIdDelegate GetSessionId;

        public event EventDataSetupDelegate DataSetup = delegate {};
        public event Action SyncChange = delegate {};
        public event EventTrackerErrorDelegate GeneralError = delegate {};

        public int MaxOutOfSyncInterval = DefaultMaxOutOfSyncInterval;
        public int SendInterval = DefaultSendInterval;
        public float Timeout = DefaultTimeout;
        public float BackoffMultiplier = DefaultBackoffMultiplier;
        public List<string> UnauthorizedEvents;

        public IHttpClient HttpClient;
        public IDeviceInfo DeviceInfo;
        public ICommandQueue CommandQueue;

        List<Events.Event> _pendingEvents;
        MonoBehaviour _behaviour;
        Coroutine _updateCoroutine;
        bool _sending;
        int _lastEventNum;
        long _lastSendTimestamp;
        bool _synced;
        long _syncTimestamp;
        float _currentTimeout;
        float _currentSendInterval;
        bool _gameStartTracked;
        IHttpConnection _httpConn;

        private string SessionId
        {
            get
            {
                if(GetSessionId == null)
                {
                    return null;
                }
                return GetSessionId();
            }
        }

        private bool IsLoggedIn
        {
            get
            {
                return !string.IsNullOrEmpty(SessionId);
            }
        }

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

        private IAppEvents _appEvents;

        public IAppEvents AppEvents
        {
            get
            {
                return _appEvents;
            }

            set
            {
                if(value == null)
                {
                    throw new ArgumentNullException("_appEvents", "_appEvents cannot be null or empty!");
                }
                if(_appEvents != null)
                {
                    DisconnectAppEvents(_appEvents);
                }
                _appEvents = value;
                ConnectAppEvents(_appEvents);
            }
        }

        #region App Events

        private void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.OpenedFromSource += OnOpenedFromSource;
            appEvents.WillGoBackground += OnAppWillGoBackground;
            appEvents.GoBackground += OnAppGoBackground;

        }

        private void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.OpenedFromSource -= OnOpenedFromSource;
            appEvents.WillGoBackground -= OnAppWillGoBackground;
            appEvents.GoBackground -= OnAppGoBackground;

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

        public SocialPointEventTracker(MonoBehaviour behaviour)
        {
            _behaviour = behaviour;
            UnauthorizedEvents = new List<string>(DefaultUnauthorizedEvents);
            _pendingEvents = new List<Event>();
            Reset();
            SetStartValues();
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
                _httpConn.Cancel();
                _httpConn = null;
            }
        }

        void SetStartValues()
        {
            _lastSendTimestamp = CurrentTimestamp;
            _currentTimeout = Timeout;
            _currentSendInterval = SendInterval;
            _syncTimestamp = CurrentTimestamp;
            _synced = false;
            _sending = false;
        }

        void OnTimeOffsetChanged(TimeSpan diff)
        {
            var dt = (long)diff.TotalSeconds;
            _lastSendTimestamp += dt;
            _syncTimestamp += dt;
        }

        private void TrackEventByRequest(string eventName, AttrDic data, ErrorDelegate del = null)
        {
            if(data == null)
            {
                data = new AttrDic();
            }
            DataSetup(data);
            var e = new Event(eventName, data, del);
            _pendingEvents.Add(e);
        }


        private void TrackEventByCommand(string eventName, AttrDic data, ErrorDelegate del = null)
        {
            if(data == null)
            {
                data = new AttrDic();
            }
            DataSetup(data);
            AddHardwareData(data);
            var eventCommand = new EventCommand(eventName, data);
            CommandQueue.Add(eventCommand, del == null ? null : new PackedCommand.FinishDelegate(del));
        }

        public void TrackSystemEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            TrackEventByRequest(eventName, data, del);
        }

        public void TrackEvent(string eventName, AttrDic data = null, ErrorDelegate del = null)
        {
            if(IsLoggedIn && CommandQueue != null)
            {
                TrackEventByCommand(eventName, data, del);
            }
            else
            {
                TrackEventByRequest(eventName, data, del);
            }
        }

        public DateTime CurrentTime
        {
            get
            {
                return TimeUtils.Now;
            }
        }


        public long CurrentTimestamp
        {
            get
            {
                return TimeUtils.Timestamp;
            }
        }

        public void Start()
        {
            if(RequestSetup == null)
            {
                throw new MissingComponentException("Request setup callback not assigned.");
            }
            if(_updateCoroutine == null)
            {
                SetStartValues();
                _updateCoroutine = _behaviour.StartCoroutine(UpdateCoroutine());
            }
            TrackGameStart();
        }

        public void Stop()
        {
            if(_updateCoroutine != null)
            {
                _behaviour.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }
        }

        public void Dispose()
        {
            Stop();
            Reset();
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
                        _sending = false;
                    }
                };
                DoSend(false, step);
                DoSend(true, step);
                return true;
            }
            return false;
        }

        void AddHardwareData(AttrDic data)
        {
            var client = new AttrDic();
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
        }

        private bool WildcardMatch(string s, string wildcard)
        {
            var pattern = "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$";
            var regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(s);
        }

        bool IsEventAuthorized(Event ev)
        {
            foreach(var pattern in UnauthorizedEvents)
            {
                if(WildcardMatch(ev.Type, pattern))
                {
                    return false;
                }
            }
            return true;
        }

        void DoSend(bool auth, Action finish = null)
        {
            var evs = new AttrList();
            List<Event> sentEvents = new List<Event>();
            foreach(var ev in _pendingEvents)
            {
                var evauth = IsEventAuthorized(ev);
                if(auth == evauth)
                {
                    if(auth && ev.Num == Event.NoNum)
                    {
                        ev.Num = _lastEventNum;
                        _lastEventNum++;
                    }
                    sentEvents.Add(ev);
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

            AttrDic data = new AttrDic();
            var common = new AttrDic();
            data.Set("common", common);
            common.SetValue("plat", DeviceInfo.Platform);
            var version = DeviceInfo.AppInfo.ShortVersion + "-" + DeviceInfo.AppInfo.Version;
            common.SetValue("ver", version);
            common.SetValue("ts", CurrentTimestamp);
            AddHardwareData(common);
            data.Set("events", evs);
            
            SendData(new JsonAttrSerializer().Serialize(data), auth, sentEvents, finish);
        }

        void SendData(Data data, bool auth, List<Event> sentEvents, Action finish = null)
        {
            HttpRequest req = new HttpRequest();
            var uri = auth ? TrackingAuthorizedUri : TrackingUnautorizedUri;
            RequestSetup(req, uri);
            req.Body = data;
            if(req.Timeout == 0.0f)
            {
                req.Timeout = Timeout;
            }
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.CompressBody = true;
            _httpConn = HttpClient.Send(req, (HttpResponse resp) => {
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
            _synced = !resp.HasConnectionError && resp.StatusCode < MinServerErrorStatusCode;
            if(oldconn != _synced)
            {
                _syncTimestamp = CurrentTimestamp;
                SyncChange();
            }
            
            if(!_synced && MaxOutOfSyncInterval > 0 && _syncTimestamp + MaxOutOfSyncInterval < CurrentTimestamp)
            {
                GeneralError(EventTrackerErrorType.OutOfSync, new Error("Too much time passed without sync."));
            }
            
            return _synced;
        }

        void OnHttpResponse(HttpResponse resp, List<Event> sentEvents)
        {
            bool success = CheckSync(resp);
            ApplyBackoff(success);
            if(success)
            {
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
                    if(ev != null && ev.ResponseDelegate != null)
                    {
                        ev.ResponseDelegate(error);
                    }

                    if(error != null && error.HasError)
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

                    _pendingEvents.Remove(ev);
                }
            }
            sentEvents.Clear();
        }

        void ApplyBackoff(bool success)
        {
            if(success)
            {
                var transTime = CurrentTimestamp - _lastSendTimestamp;
                _currentTimeout = Mathf.Max(transTime, Timeout);
            }
            else
            {
                _currentTimeout *= BackoffMultiplier;
            }
        }


        public void TrackFunnel(FunnelOperation op)
        {

            var data = op.AdditionalData;
            if(data == null)
            {
                data = new AttrDic();
            }
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

        public void TrackPurchaseStart(PurchaseStartOperation op)
        {
            AttrDic data = op.AdditionalData ?? new AttrDic();

            AttrDic order = new AttrDic();
            data.Set("order", order);
            order.SetValue("transaction_id", op.TransactionId);
            order.SetValue("product_id", op.ProductId);
            order.SetValue("payment_provider", op.PaymentProvider);
            order.SetValue("amount_gross", op.AmountGross);
            order.SetValue("offer", op.OfferName);
            order.SetValue("resource_type", op.ResourceName);
            order.SetValue("resource_amount", op.ResourceAmount);

            TrackSystemEvent(EventNameMonetizationTransactionStart, data);
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

        public void TrackGameBackground()
        {
            TrackSystemEvent(EventNameGameBackground);
        }

        void TrackGameOpen(AppSource source)
        {
            AttrDic data = new AttrDic();
            var origin = new AttrDic();
            data.Set("origin", origin);

            string sourceType = "app";
            string detail = "";

            if(source.IsCustomScheme())
            {
                sourceType = source.Scheme;
                detail = source.QueryString;
            }
            else if(!source.Empty)
            {
                sourceType = "url";
                detail = source.Uri;
            }

            origin.SetValue("source", sourceType);
            origin.SetValue("detail", detail);

            TrackSystemEvent(EventNameGameOpen, data);
        }

        public void TrackResource(ResourceOperation op)
        {
            string name;
            if(op.Amount >= 0)
            {
                name = EventNameResourceEarning;
            }
            else
            {
                name = EventNameResourceSpending;
            }
            name = string.Format(name, op.Resource);

            var data = op.AdditionalData;
            if(data == null)
            {
                data = new AttrDic();
            }
            var operation = new AttrDic();
            data.Set("operation", operation);
            operation.SetValue("category", op.Category);
            operation.SetValue("subcategory", op.Subcategory);
            operation.SetValue("amount", Mathf.Abs(op.Amount));
            operation.SetValue("potential_amount", Mathf.Abs(op.PotentialAmount));
            operation.SetValue("lost_amount", Mathf.Abs(op.LostAmount));
            operation.SetValue("type", op.Resource);

            TrackEvent(name, data);
        }

    }
}
