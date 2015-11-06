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

        #if CRASH_REPORTER_TEST_EVENTS
        private class TestReport : Report
        {
            public override long Timestamp
            {
                get
                {
                    return TimeUtils.Timestamp;
                }
            }
            
            public override string StackTrace
            {
                get
                {
                    return "test stacktrace";
                }
            }
            
            public override string Log
            {
                get
                {
                    return "test log";
                }
            }
        }
#endif

        #endregion

        public delegate void RequestSetupDelegate(HttpRequest req,string Uri);

        public delegate void TrackEventDelegate(string eventName,AttrDic data = null,ErrorDelegate del = null);

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
        BreadcrumbManager _breadcrumbManager;
        HashSet<string> _uniqueExceptions;
        public RequestSetupDelegate RequestSetup;
        public TrackEventDelegate TrackEvent;
        public GetUserIdDelegate GetUserId;

        public const float DefaultSendInterval = 20.0f;
        public const bool DefaultExceptionLogActive = true;
        public const bool DefaultErrorLogActive = true;

        bool _wasActiveInLastSession;
        bool _exceptionLogActive = DefaultExceptionLogActive;
        bool _errorLogActive = DefaultErrorLogActive;

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

            _wasActiveInLastSession = !WasOnBackground && WasEnabled;
        }

        public void Enable()
        {
            WasEnabled = true;
            LogCallbackHandler.RegisterLogCallback(HandleLog);
            OnEnable();
            Check();

            if(_updateCoroutine == null)
            {
                _updateCoroutine = _behaviour.StartCoroutine(UpdateCoroutine());
            }

#if CRASH_REPORTER_TEST_EVENTS
            TrackException("testing exception log", "testing exception stack");
            TrackCrash(new TestReport());
#endif
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

        void Check()
        {
            CheckLogs();
            CheckPendingCrashes();
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

        void CheckPendingCrashes()
        {
            List<Report> pendingReports = GetPendingCrashes();
            if(pendingReports.Count > 0)
            {
                foreach(Report report in pendingReports)
                {
                    //trackcrash will create the log if is success
                    TrackCrash(report);
                }
            }
            else
            {
                // If there are no new crashes, we can check some saved status to detect a memory crash
                Report memoryCrashReport = CheckMemoryCrash();
                if(memoryCrashReport != null)
                {
                    TrackCrash(memoryCrashReport);
                }
            }

            ClearLastSessionInfo();
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

        void CheckLogs()
        {
            if(HasExceptionLogs)
            {
                SendExceptions(_exceptionStorage.StoredKeys);
            }

            if(HasCrashLogs)
            {
                foreach(var log in _crashStorage.StoredKeys)
                {
                    SendCrashLog(log);
                }
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

        void SendCrashLog(string log)
        {
            if(RequestSetup == null)
            {
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
            _httpClient.Send(req, resp => OnCrashSend(resp, log));
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

        void OnCrashSend(HttpResponse resp, string log)
        {
            if(!resp.HasError)
            {
                _crashStorage.Remove(log);
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

        void TrackCrash(Report report)
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

                TrackEvent(CrashEventName, data, err => CreateCrashLog(report));
            }
            else
            {
                CreateCrashLog(report);
            }
        }

        void CreateCrashLog(Report report)
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
            SendCrashLog(report.Uuid);
            report.Remove();
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
            if(_lastSendTimestamp + (long)_currentSendInterval < TimeUtils.Timestamp)
            {
                if(HasExceptionLogs)
                {
                    SendExceptions(_exceptionStorage.StoredKeys);
                }
            }
        }

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning += OnMemoryWarning;
            appEvents.WillGoBackground.Add(0, OnWillGoBackground);
            appEvents.WasOnBackground += OnWillGoForeground;
            appEvents.LevelWasLoaded += OnLevelWasLoaded;
            appEvents.ApplicationQuit += OnApplicationQuit;
            appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning -= OnMemoryWarning;
            appEvents.WillGoBackground.Remove(OnWillGoBackground);
            appEvents.WasOnBackground -= OnWillGoForeground;
            appEvents.LevelWasLoaded -= OnLevelWasLoaded;
            appEvents.ApplicationQuit -= OnApplicationQuit;
            appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
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
            if(_updateCoroutine == null)
            {
                Enable();
            }
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
            if(HasExceptionLogs)
            {
                SendExceptions(_exceptionStorage.StoredKeys);
            }
        }

        #endregion

        static void CatchException(Exception e)
        {
            Debug.LogException(e);
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #endif
        }
    }
}
