using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.Crash
{
    /*
     * Crash reporter Base implementation
     */
    public class BaseCrashReporter : ICrashReporter
    {
        #region Stored logs

        /*
         * Log private classes
         */
        protected class SocialPointLog : AttrDic
        {
            protected const string AttrKeyPlatform = "platform";
            protected const string AttrKeyTimestamp = "ts";
            protected const string AttrKeyDevice = "device";
            protected const string AttrKeyOS = "os";
            protected const string AttrKeyModel = "model";
            protected const string AttrKeyLanguage = "language";
            protected const string AttrKeyClient = "client";
            protected const string AttrKeyVersion = "version";
            protected const string AttrKeyBundle = "bundle_version";
            protected const string AttrKeyClientLanguage = "language";
            protected const string AttrKeyUserId = "user_id";

            public SocialPointLog(IDeviceInfo deviceInfo, UInt64 userId)
            {
                Set(AttrKeyPlatform, new AttrString(deviceInfo.Platform));
                Set(AttrKeyTimestamp, new AttrLong(TimeUtils.Timestamp));
                Set(AttrKeyUserId, new AttrString(userId.ToString()));

                var device = new AttrDic();
                device.Set(AttrKeyOS, new AttrString(deviceInfo.PlatformVersion));
                device.Set(AttrKeyModel, new AttrString(deviceInfo.Model));
                device.Set(AttrKeyLanguage, new AttrString(deviceInfo.Language));
                Set(AttrKeyDevice, device);

                var client = new AttrDic();
                client.Set(AttrKeyVersion, new AttrString(deviceInfo.AppInfo.Version));
                client.Set(AttrKeyBundle, new AttrString(deviceInfo.AppInfo.ShortVersion + "-" + deviceInfo.AppInfo.Version));
                client.Set(AttrKeyClientLanguage, new AttrString(deviceInfo.AppInfo.Language));
                Set(AttrKeyClient, client);
            }
        }

        protected class SocialPointCrashLog : SocialPointLog
        {
            const string AttrKeyCrash = "crash";
            const string AttrKeyUuid = "uuid";
            const string AttrKeyStacktrace = "stacktrace";
            const string AttrKeyBreadcrumb = "breadcrumb";
            const string AttrKeyLogcat = "logcat";
            const string AttrKeyCrashBuildId = "crash_build_id";
            const string AttrKeyRealCrashTime = "real_crash_time";

            public SocialPointCrashLog(Report report, IDeviceInfo deviceInfo, UInt64 userId, string breadcrumb = null)
                : base(deviceInfo, userId)
            {
                var crash = new AttrDic();
                crash.Set(AttrKeyUuid, new AttrString(report.Uuid));

                crash.Set(AttrKeyStacktrace, new AttrString(report.StackTrace));
                crash.Set(AttrKeyLogcat, new AttrString(report.Log));
                crash.Set(AttrKeyBreadcrumb, new AttrString(breadcrumb));

                // Add actual crash data if available
                if(report.Timestamp > 0)
                {
                    crash.Set(AttrKeyRealCrashTime, new AttrLong(report.Timestamp));
                }

                if(report.CrashVersion.Length > 0)
                {
                    crash.Set(AttrKeyCrashBuildId, new AttrString(report.CrashVersion));
                }

                Set(AttrKeyCrash, crash);
            }
        }

        protected class SocialPointExceptionLog : SocialPointLog
        {
            const string AttrKeyException = "exception";
            const string AttrKeyUuid = "uuid";
            const string AttrKeyLog = "log";
            const string AttrKeyStacktrace = "stacktrace";
            const string AttrKeyLoadedLevelName = "loadedlevel";

            public SocialPointExceptionLog(string uuid, string log, string stacktrace, IDeviceInfo deviceInfo, UInt64 userId)
                : base(deviceInfo, userId)
            {
                var exception = new AttrDic();
                exception.Set(AttrKeyUuid, new AttrString(uuid));
                exception.Set(AttrKeyLog, new AttrString(log));
                exception.Set(AttrKeyStacktrace, new AttrString(stacktrace));
                exception.Set(AttrKeyLoadedLevelName, new AttrString(Application.loadedLevelName));
                Set(AttrKeyException, exception);
            }
        }

        #endregion

        #region Crash Reports

        /* 
         * Internal report class
         */
        protected class Report
        {
            public string Uuid { get; set; }

            public Report()
            {
                Uuid = RandomUtils.GetUuid();
            }

            protected bool _outOfMemory;

            public bool OutOfMemory
            {
                get
                {
                    return _outOfMemory;
                }
            }

            public virtual void Remove()
            {
            }

            public virtual long Timestamp      { get { return  0; } }

            public virtual string CrashVersion { get { return string.Empty; } }

            public virtual string StackTrace   { get { return string.Empty; } }

            public virtual string Log          { get { return string.Empty; } }
        }

        protected class OutOfMemoryReport : Report
        {
            long _timestamp;
            readonly string _message = "APP KILLED IN FOREGROUND BECAUSE OF LOW MEMORY.";

            public OutOfMemoryReport(long timestamp)
            {
                _timestamp = timestamp;
                if(_timestamp != 0)
                {
                    _message += string.Format(" RECEIVED MEMORY WARNING AT {0} ts: {1}", TimeUtils.GetDateTime(_timestamp).ToUniversalTime(), _timestamp);
                }
                else
                {
                    _message += " NO MEMORY WARNING TIMESTAMP";
                    _timestamp = TimeUtils.Timestamp;
                }

                _outOfMemory = true;
            }

            public override long Timestamp
            { 
                get
                {
                    return _timestamp;
                }
            }

            public override string StackTrace
            { 
                get
                {
                    return _message;
                }
            }
        }

        #endregion

        enum ReportSendType
        {
            BeforeLogin,
            AfterLogin
        }

        public delegate void RequestSetupDelegate(HttpRequest req, string Uri);

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

        public delegate UInt64 GetUserIdDelegate();

        const string UriCrash = "crash";
        const string UriException = "exceptions";
        const string AttrKeyCrashUuid = "uuid";
        const string AttrKeyBuildId = "build_id";
        const string AttrKeyContent = "content";
        const string AttrKeyCrashBreadcrumb = "breadcrumb";
        const string AttrKeyCrashLogcat = "logcat";
        const string AttrKeyError = "error";
        const string AttrKeyUnityException = "unity_exception";
        const string AttrKeyMobile = "mobile";
        const string AttrKeyMessage = "message";
        const string AttrKeyType = "type";
        const string AttrKeyCrashTimestamp = "crash_timestamp";

        // Player preferences keys
        const string WasOnBackgroundPreferencesKey = "app_gone_background";
        const string LastMemoryWarningPreferencesKey = "last_memory_warning";
        const string CrashReporterEnabledPreferencesKey = "crash_reporter_enabled";

        // Events
        const string ExceptionEventName = "errors.unity_exception";
        const string CrashEventName = "errors.mobile_crash_triggered";

        IHttpClient _httpClient;
        IDeviceInfo _deviceInfo;
        readonly PersistentAttrStorage _exceptionStorage;
        PersistentAttrStorage _crashStorage;
        List<Report> _pendingReports;
        BreadcrumbManager _breadcrumbManager;
        HashSet<string> _uniqueExceptions;
        public RequestSetupDelegate RequestSetup;
        public TrackEventDelegate TrackEvent;
        public GetUserIdDelegate GetUserId;

        public const float DefaultSendInterval = 20.0f;
        public const bool DefaultExceptionLogActive = true;
        public const bool DefaultErrorLogActive = true;
        public const bool DefaultEnableSendingCrashesBeforeLogin = false;
        public const int DefaultNumRetriesBeforeSendingCrashBeforeLogin = 3;

        bool _wasActiveInLastSession;
        bool _exceptionLogActive = DefaultExceptionLogActive;
        bool _errorLogActive = DefaultErrorLogActive;
        bool _enableSendingCrashesBeforeLogin = DefaultEnableSendingCrashesBeforeLogin;
        int _numRetriesBeforeSendingCrashBeforeLogin = DefaultNumRetriesBeforeSendingCrashBeforeLogin;

        public float SendInterval
        {
            get{ return _currentSendInterval; }
            set{ _currentSendInterval = value; }
        }

        MonoBehaviour _behaviour;
        Coroutine _updateCoroutine;
        float _currentSendInterval = DefaultSendInterval;
        long _lastSendTimestamp;
        bool _sending;

        public bool ExceptionLogActive
        {
            get
            {
                return _exceptionLogActive;
            }

            set
            {
                _exceptionLogActive = value;
            }
        }

        public bool ErrorLogActive
        {
            get
            {
                return _errorLogActive;
            }

            set
            {
                _errorLogActive = value;
            }
        }

        public bool EnableSendingCrashesBeforeLogin
        {
            get
            {
                return _enableSendingCrashesBeforeLogin;
            }

            set
            {
                _enableSendingCrashesBeforeLogin = value;
            }
        }

        public int NumRetriesBeforeSendingCrashBeforeLogin
        {
            get
            {
                return _numRetriesBeforeSendingCrashBeforeLogin;
            }

            set
            {
                _numRetriesBeforeSendingCrashBeforeLogin = value;
            }
        }

        static long LastMemoryWarningTimestamp
        {
            get
            {
                return Convert.ToInt64(PlayerPrefs.GetString(LastMemoryWarningPreferencesKey, "0"));
            }
            set
            {
                PlayerPrefs.SetString(LastMemoryWarningPreferencesKey, "" + value);
                PlayerPrefs.Save();
            }
        }

        public bool WasEnabled
        { 
            get
            {
                return PlayerPrefs.GetInt(CrashReporterEnabledPreferencesKey) > 0;
            }
            private set
            {
                PlayerPrefs.SetInt(CrashReporterEnabledPreferencesKey, (value) ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        static bool WasOnBackground
        {
            get
            {
                return PlayerPrefs.GetInt(WasOnBackgroundPreferencesKey) > 0;
            }
            set
            {
                PlayerPrefs.SetInt(WasOnBackgroundPreferencesKey, (value) ? 1 : 0);
                PlayerPrefs.Save();
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

        UInt64 _storedUserId;

        UInt64 UserId
        {
            get
            {
                /* Always try to refresh the user id using the provided delegate
                 * If not possible, use the last user id retrieved in the current
                 * game session.*/
                if(GetUserId != null)
                {
                    UInt64 userId = GetUserId();
                    if(userId != 0)
                    {
                        _storedUserId = userId;
                    }
                }

                return _storedUserId;
            }
        }

        public BaseCrashReporter(MonoBehaviour behaviour, IHttpClient client, 
                                 IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbManager = null)
        {
            _behaviour = behaviour;
            _httpClient = client;
            _deviceInfo = deviceInfo;

            _exceptionStorage = new PersistentAttrStorage(FileUtils.Combine(PathsManager.PersistentDataPath, "logs/exceptions"));
            _crashStorage = new PersistentAttrStorage(FileUtils.Combine(PathsManager.PersistentDataPath, "logs/crashes"));
           
            //only used when crash detected
            _breadcrumbManager = breadcrumbManager;

            _uniqueExceptions = new HashSet<string>();

            _pendingReports = new List<Report>();

            _wasActiveInLastSession = !WasOnBackground && WasEnabled;
        }

        public void Enable()
        {
            if(_updateCoroutine != null)
            {
                return;
            }

            WasEnabled = true;
            LogCallbackHandler.RegisterLogCallback(HandleLog);
            OnEnable();

            _updateCoroutine = _behaviour.StartCoroutine(UpdateCoroutine());
        }

        protected virtual void OnEnable()
        {
        }

        public void Disable()
        {
            WasEnabled = false;
            if(_updateCoroutine != null)
            {
                _behaviour.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }

            LogCallbackHandler.UnregisterLogCallback(HandleLog);
            OnDisable();
        }

        protected virtual void OnDisable()
        {
        }

        [Obsolete("Use Dispose()")]
        public void Destroy()
        {
            Dispose();
        }

        public void Dispose()
        {
            Disable();
            if(_appEvents != null)
            {
                DisconnectAppEvents(_appEvents);
            }
            OnDestroy();
        }

        protected virtual void OnDestroy()
        {
        }

        public void ClearUniqueExceptions()
        {
            _uniqueExceptions.Clear();
        }

        protected virtual List<Report> GetPendingCrashes()
        {
            return new List<Report>();
        }

        public virtual void ForceCrash()
        {
            // Null object exception
            GameObject go;
            go = null;
            go.transform.position = Vector3.zero;
        }

        void SetupCrashHttpRequest(HttpRequest req, string log)
        {
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.Body = new JsonAttrSerializer().Serialize(_crashStorage.Load(log));

            // Crash report requires platform parameter in order to be redirected to the proper S3 storage 
            req.AddQueryParam("platform", new AttrString(_deviceInfo.Platform));

            req.CompressBody = true;
        }

        void SetupExceptionHttpRequest(HttpRequest req, string log)
        {
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.Body = new JsonAttrSerializer().Serialize(_exceptionStorage.Load(log));
            req.CompressBody = true;
        }

        public bool HasExceptionLogs
        {
            get{ return (_exceptionStorage != null && _exceptionStorage.StoredKeys.Length > 0); }
        }

        public bool HasCrashLogs
        {
            get{ return (_crashStorage != null && _crashStorage.StoredKeys.Length > 0); }
        }

        protected void ReadPendingCrashes()
        {
            _pendingReports = GetPendingCrashes();

            if(_pendingReports.Count > 0)
            {
                foreach(Report report in _pendingReports)
                {
                    AddRetry(report.Uuid);
                }
            }
            else
            {
                // If there are no new crashes, we can check some saved status to detect a memory crash
                Report memoryCrashReport = CheckMemoryCrash();
                if(memoryCrashReport != null)
                {
                    AddRetry(memoryCrashReport.Uuid);
                }
            }

            if(HasCrashLogs)
            {
                foreach(var log in _crashStorage.StoredKeys)
                {
                    AddRetry(log);
                }
            }
        }

        void SendCrashesAfterLogin(Action callback = null)
        {
            SendCrashes(ReportSendType.AfterLogin, () => {
                if(callback != null)
                {
                    callback();
                }
            });
        }

        public void SendCrashesBeforeLogin(Action callback)
        {
            SendCrashes(ReportSendType.BeforeLogin, () => {
                if(callback != null)
                {
                    callback();
                }
            });
        }

        void SendCrashes(ReportSendType reportSendType, Action callback)
        {
            int count = 2;
            Action step = () => {
                count--;
                if(count <= 0 && callback != null)
                {
                    callback();
                }
            };
            SendTrackedCrashes(reportSendType, step);
            SendPendingCrashes(reportSendType, step);
        }

        void SendTrackedCrashes(ReportSendType reportSendType, Action callback)
        {
            int trackedCrashesToSend = TrackedCrashesToSend(reportSendType);

            if(trackedCrashesToSend > 0)
            {
                Action step = () => {
                    --trackedCrashesToSend;
                    if(trackedCrashesToSend == 0 && callback != null)
                    {
                        callback();
                    }
                };
                if(HasCrashLogs)
                {
                    foreach(var log in _crashStorage.StoredKeys)
                    {
                        if(reportSendType == GetReportSendType(log))
                        {
                            SendCrashLog(log, step);
                        }
                    }
                }
            }
            else if(callback != null)
            {
                callback();
            }
        }

        void SendPendingCrashes(ReportSendType reportSendType, Action callback)
        {
            int pendingCrashesToSend = PendingCrashesToSend(reportSendType);

            if(pendingCrashesToSend > 0)
            {
                Action step = () => {
                    --pendingCrashesToSend;
                    if(pendingCrashesToSend == 0 && callback != null)
                    {
                        callback();
                    }
                };
                if(_pendingReports.Count > 0)
                {
                    foreach(Report report in _pendingReports)
                    {
                        if(reportSendType == GetReportSendType(report.Uuid))
                        {
                            //trackcrash will create the log if is success
                            TrackCrash(report, step);
                        }
                    }
                }
                else
                {
                    // If there are no new crashes, we can check some saved status to detect a memory crash
                    Report memoryCrashReport = CheckMemoryCrash();
                    if(memoryCrashReport != null)
                    {
                        if(reportSendType == GetReportSendType(memoryCrashReport.Uuid))
                        {
                            TrackCrash(memoryCrashReport, step);
                        }
                    }
                }
            }
            else if(callback != null)
            {
                callback();
            }

            ClearLastSessionInfo();
        }

        int TrackedCrashesToSend(ReportSendType reportSendType)
        {
            int trackedCrashesToSend = 0;
            if(HasCrashLogs)
            {
                foreach(var log in _crashStorage.StoredKeys)
                {
                    if(reportSendType == GetReportSendType(log))
                    {
                        ++trackedCrashesToSend;
                    }
                }
            }
            return trackedCrashesToSend;
        }

        int PendingCrashesToSend(ReportSendType reportSendType)
        {
            int pendingCrashesToSend = 0;
            if(_pendingReports.Count > 0)
            {
                foreach(Report report in _pendingReports)
                {
                    if(reportSendType == GetReportSendType(report.Uuid))
                    {
                        ++pendingCrashesToSend;
                    }
                }
            }
            else
            {
                // If there are no new crashes, we can check some saved status to detect a memory crash
                Report memoryCrashReport = CheckMemoryCrash();
                if(memoryCrashReport != null)
                {
                    if(reportSendType == GetReportSendType(memoryCrashReport.Uuid))
                    {
                        ++pendingCrashesToSend;
                    }
                }
            }
            return pendingCrashesToSend;
        }

        static void ClearLastSessionInfo()
        {
            // Clear last memory warning timestamp and set foreground status
            LastMemoryWarningTimestamp = 0;
            WasOnBackground = false;
        }

        Report CheckMemoryCrash()
        {
            Report memoryCrashReport = null;
            /* *
             * We can assume that we had a memory crash if 
             * the application was closed in foreground in the
             * last session and the BreadcrumbManager hadn't been 
             * cleaned (as in any clean stop. See OnApplicationQuit())
             * */
            if(_breadcrumbManager != null &&
               _breadcrumbManager.OldBreadcrumb != null &&
               _wasActiveInLastSession)
            {
                memoryCrashReport = new OutOfMemoryReport(LastMemoryWarningTimestamp);
            }

            return memoryCrashReport;
        }

        void SendExceptionLogs()
        {
            if(HasExceptionLogs)
            {
                SendExceptions(_exceptionStorage.StoredKeys);
            }
        }

        bool SendExceptions(string[] storedKeys)
        {
            if(!_sending)
            {
                _sending = true;
                DoSendExceptions(storedKeys);
                return true;
            }
            return false;
        }

        void DoSendExceptions(string[] storedKeys)
        {
            if(RequestSetup == null)
            {
                return;
            }
            var req = new HttpRequest();
            try
            {
                RequestSetup(req, UriException);
            }
            catch(Exception e)
            {
                CatchException(e);
            }
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            var exceptionLogs = new AttrList();
            foreach(var storedKey in storedKeys)
            {
                try
                {
                    exceptionLogs.Add(_exceptionStorage.Load(storedKey));
                }
                catch(SerializationException)
                {
                }
            }
            req.Body = new JsonAttrSerializer().Serialize(exceptionLogs);
            req.CompressBody = true;
            _httpClient.Send(req, resp => OnExceptionSend(resp, storedKeys));
            _lastSendTimestamp = TimeUtils.Timestamp;
        }

        void SendCrashLog(string log, Action callback)
        {
            if(RequestSetup == null)
            {
                if(callback != null)
                {
                    callback();
                }
                return;
            }
            var req = new HttpRequest();
            try
            {
                RequestSetup(req, UriCrash);
            }
            catch(Exception e)
            {
                CatchException(e);
            }
            SetupCrashHttpRequest(req, log);
            _httpClient.Send(req, resp => OnCrashSend(resp, log, callback));
        }

        void OnExceptionSend(HttpResponse resp, string[] storedKeys)
        {
            _sending = false;
            if(!resp.HasError)
            {
                foreach(var key in storedKeys)
                {
                    _exceptionStorage.Remove(key);
                }
            }
        }

        void OnCrashSend(HttpResponse resp, string log, Action callback)
        {
            if(!resp.HasError)
            {
                _crashStorage.Remove(log);
                EraseRetryKey(log);
            }
            else
            {
                SubtractRetry(log);
            }

            if(callback != null)
            {
                callback();
            }
        }

        void HandleLog(string logString, string stackTrace, LogType type)
        {
            bool doHandleLog = false || type == LogType.Exception && _exceptionLogActive;
            
            doHandleLog |= type == LogType.Error && _errorLogActive;
            
            if(doHandleLog)
            {
                TrackException(logString, stackTrace);
            }
        }

        void TrackException(string logString, string stackTrace)
        {
            string exceptionHashSource = logString + stackTrace;
            if(_uniqueExceptions.Contains(exceptionHashSource))
            {
                return;
            }
            string uuid = RandomUtils.GetUuid();
            var exception = new SocialPointExceptionLog(uuid, logString, stackTrace, _deviceInfo, UserId);
            _exceptionStorage.Save(uuid, exception);
            _uniqueExceptions.Add(exceptionHashSource);

            if(TrackEvent != null)
            {
                var data = new AttrDic();
                var error = new AttrDic();
                data.Set(AttrKeyError, error);
                var mobile = new AttrDic();
                error.Set(AttrKeyUnityException, mobile);
                mobile.SetValue(AttrKeyCrashUuid, uuid);
                mobile.SetValue(AttrKeyMessage, logString);
                mobile.SetValue(AttrKeyType, 0);

                TrackEvent(ExceptionEventName, data);
            }
        }

        void TrackCrash(Report report, Action callback)
        {
            if(TrackEvent != null)
            {
                var data = new AttrDic();
                var error = new AttrDic();
                data.Set(AttrKeyError, error);
                var mobile = new AttrDic();
                error.Set(AttrKeyMobile, mobile);
                mobile.SetValue(AttrKeyCrashUuid, report.Uuid);
                mobile.SetValue(AttrKeyCrashTimestamp, report.Timestamp);
                mobile.SetValue(AttrKeyType, report.OutOfMemory ? 1 : 0);

                TrackEvent(CrashEventName, data, err => {
                    if(!Error.IsNullOrEmpty(err))
                    {
                        SubtractRetry(report.Uuid);
                        if(callback != null)
                        {
                            callback();
                        }
                    }
                    else
                    {
                        CreateCrashLog(report, callback);
                    }
                });
            }
            else
            {
                if(callback != null)
                {
                    callback();
                }
            }
        }

        void CreateCrashLog(Report report, Action callback)
        {
            // Create the log on our storage to be send
            string oldBreadcrumbs = "";
            if(_breadcrumbManager != null)
            {
                oldBreadcrumbs = _breadcrumbManager.OldBreadcrumb;
            }

            var crashLog = new SocialPointCrashLog(report, _deviceInfo, UserId, oldBreadcrumbs);
            _crashStorage.Save(report.Uuid, crashLog);

            // Try to send current crash and remove crash data. 
            // The CrashLog is stored and can be sent again if fails
            SendCrashLog(report.Uuid, callback);

            report.Remove(); // we remove the report in order to not track it again :)
        }

        IEnumerator UpdateCoroutine()
        {
            SendExceptionLogs();
            while(true)
            {
                Update();
                yield return true;
            }
        }

        void Update()
        {
            if(_lastSendTimestamp + (long)_currentSendInterval < TimeUtils.Timestamp)
            {
                SendExceptionLogs();
            }
        }

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning += OnMemoryWarning;
            appEvents.RegisterWillGoBackground(0, OnWillGoBackground);
            appEvents.WasOnBackground += OnWillGoForeground;
            appEvents.LevelWasLoaded += OnLevelWasLoaded;
            appEvents.ApplicationQuit += OnApplicationQuit;
            appEvents.RegisterGameWasLoaded(0, OnGameWasLoaded);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning -= OnMemoryWarning;
            appEvents.UnregisterWillGoBackground(OnWillGoBackground);
            appEvents.WasOnBackground -= OnWillGoForeground;
            appEvents.LevelWasLoaded -= OnLevelWasLoaded;
            appEvents.ApplicationQuit -= OnApplicationQuit;
            appEvents.UnregisterGameWasLoaded(OnGameWasLoaded);
        }

        void OnMemoryWarning()
        {
            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.Log("Memory Warning");
            }

            // Store memory warning timestamp
            LastMemoryWarningTimestamp = TimeUtils.Timestamp;
        }

        void OnLevelWasLoaded(int level)
        {
            ClearUniqueExceptions();
        }

        void OnGameWasLoaded()
        {
            Enable();
            SendCrashesAfterLogin();
        }

        void OnApplicationQuit()
        {
            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.RemoveData();
            }
        }

        static void OnWillGoBackground()
        {
            WasOnBackground = true;
        }

        void OnWillGoForeground()
        {
            WasOnBackground = false;
            SendExceptionLogs();
        }

        #endregion

        static void CatchException(Exception e)
        {
            Debug.LogException(e);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }

        #region PlayerPrefss

        ReportSendType GetReportSendType(string reportUuid)
        {
            var retries = GetRetries(reportUuid);
            var reportSendType = (EnableSendingCrashesBeforeLogin && (retries >= NumRetriesBeforeSendingCrashBeforeLogin)) ? ReportSendType.BeforeLogin : ReportSendType.AfterLogin;
            return reportSendType;
        }

        static string GetRetriesKey(string reportUuid)
        {
            string retriesKey = reportUuid + "_crash_retries";
            return retriesKey;
        }

        static int GetRetries(string reportUuid)
        {
            var retriesKey = GetRetriesKey(reportUuid);
            int retries = PlayerPrefs.GetInt(retriesKey);
            return retries;
        }

        static void AddRetry(string reportUuid, int retriesToAdd = 1)
        {
            var retriesKey = GetRetriesKey(reportUuid);
            var retries = GetRetries(reportUuid);
            PlayerPrefs.SetInt(retriesKey, retries + retriesToAdd);
            PlayerPrefs.Save(); 
        }

        static void SubtractRetry(string reportUuid, int retriesToRemove = 1)
        {
            var retriesKey = GetRetriesKey(reportUuid);
            var retries = GetRetries(reportUuid);
            PlayerPrefs.SetInt(retriesKey, Math.Max(retries - retriesToRemove, 0));
            PlayerPrefs.Save();
        }

        static void EraseRetryKey(string reportUuid)
        {
            var retriesKey = GetRetriesKey(reportUuid);
            PlayerPrefs.DeleteKey(retriesKey);
            PlayerPrefs.Save();
        }

        #endregion
    }
}
