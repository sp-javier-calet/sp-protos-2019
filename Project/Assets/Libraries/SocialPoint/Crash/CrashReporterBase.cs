using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Linq;

using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.Utils;
using SocialPoint.IO;

using SPDebug = SocialPoint.Base.Debug;

namespace SocialPoint.Crash
{
    /*
     * Crash reporter Base implementation
     */
    public class CrashReporterBase : ICrashReporter
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

                AttrDic device = new AttrDic();
                device.Set(AttrKeyOS, new AttrString(deviceInfo.PlatformVersion));
                device.Set(AttrKeyModel, new AttrString(deviceInfo.Model));
                device.Set(AttrKeyLanguage, new AttrString(deviceInfo.Language));
                Set(AttrKeyDevice, device);

                AttrDic client = new AttrDic();
                client.Set(AttrKeyVersion, new AttrString(deviceInfo.AppInfo.Version));
                client.Set(AttrKeyBundle, new AttrString(deviceInfo.AppInfo.ShortVersion + "-" + deviceInfo.AppInfo.Version));
                client.Set(AttrKeyClientLanguage, new AttrString(deviceInfo.AppInfo.Language));
                Set(AttrKeyClient, client);
            }
        }

        protected class SocialPointCrashLog : SocialPointLog
        {
            private const string AttrKeyCrash = "crash";
            private const string AttrKeyUuid = "uuid";
            private const string AttrKeyStacktrace = "stacktrace";
            private const string AttrKeyBreadcrumb = "breadcrumb";
            private const string AttrKeyLogcat = "logcat";
            private const string AttrKeyCrashBuildId = "crash_build_id";
            private const string AttrKeyRealCrashTime = "real_crash_time";

            public SocialPointCrashLog(Report report, IDeviceInfo deviceInfo, UInt64 userId, string breadcrumb = null)
                : base(deviceInfo, userId)
            {
                AttrDic crash = new AttrDic();
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
            private const string AttrKeyException = "exception";
            private const string AttrKeyUuid = "uuid";
            private const string AttrKeyLog = "log";
            private const string AttrKeyStacktrace = "stacktrace";

            public SocialPointExceptionLog(string uuid, string log, string stacktrace, IDeviceInfo deviceInfo, UInt64 userId)
                : base(deviceInfo, userId)
            {
                AttrDic exception = new AttrDic();
                exception.Set(AttrKeyUuid, new AttrString(uuid));
                exception.Set(AttrKeyLog, new AttrString(log));
                exception.Set(AttrKeyStacktrace, new AttrString(stacktrace));
                Set(AttrKeyException, exception);
            }
        }

        #endregion

        #region Crash Reports

        /* 
         * Internal report class
         */
        protected abstract class Report
        {
            public string Uuid { get; set; }

            public Report()
            {
                Uuid = RandomUtils.GetUuid();
            }

            protected bool _outOfMemory = false;

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

            public virtual string CrashVersion { get { return ""; } }

            public virtual string StackTrace   { get { return ""; } }

            public virtual string Log          { get { return ""; } }
        }

        protected class OutOfMemoryReport : Report
        {
            private long _timestamp;
            private string _message = "APP KILLED IN FOREGROUND BECAUSE OF LOW MEMORY.";

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
            public override long GetTimestamp()
            {
                return TimeUtils.Timestamp;
            }
            
            public override string GetStackTrace()
            {
                return "test stacktrace";
            }
            
            public override string GetLog()
            {
                return "test log";
            }
        }
#endif

        #endregion

        public delegate void RequestSetupDelegate(HttpRequest req,string Uri);

        public delegate void TrackEventDelegate(string eventName,AttrDic data = null,ErrorDelegate del = null);

        public delegate UInt64 GetUserIdDelegate();

        private const string UriCrash = "crash";
        private const string UriException = "exceptions";
        private const string AttrKeyUuid = "uuid";
        private const string AttrKeyBuildId = "build_id";
        private const string AttrKeyContent = "content";
        private const string AttrKeyBreadcrumb = "breadcrumb";
        private const string AttrKeyLogcat = "logcat";
        private const string AttrKeyError = "error";
        private const string AttrKeyUnityException = "unity_exception";
        private const string AttrKeyMobile = "mobile";
        private const string AttrKeyMessage = "message";
        private const string AttrKeyType = "type";
        private const string AttrKeyTimestamp = "crash_timestamp";

        // Player preferences keys
        private const string WasOnBackgroundPreferencesKey = "app_gone_background";
        private const string LastMemoryWarningPreferencesKey = "last_memory_warning";
        private const string CrashReporterEnabledPreferencesKey = "crash_reporter_enabled";

        // Events
        private const string ExceptionEventName = "errors.unity_exception";
        private const string CrashEventName = "errors.mobile_crash_triggered";

        private IHttpClient _httpClient;
        private IDeviceInfo _deviceInfo;
        private PersistentAttrStorage _exceptionStorage;
        private PersistentAttrStorage _crashStorage;
        private BreadcrumbManager _breadcrumbManager;
        private HashSet<string> _uniqueExceptions;
        public RequestSetupDelegate RequestSetup;
        public TrackEventDelegate TrackEvent;
        public GetUserIdDelegate GetUserId;

        public const float DefaultSendInterval = 20.0f;
        public const bool DefaultExceptionLogActive = true;
        public const bool DefaultErrorLogActive = true;

        bool _wasActiveInLastSession = false;
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
            set
            {
                _exceptionLogActive = value;
            }
        }

        public bool ErrorLogActive
        {
            set
            {
                _errorLogActive = value;
            }
        }

        private long LastMemoryWarningTimestamp
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

        private bool IsEnabled
        { 
            get
            {
                return PlayerPrefs.GetInt(CrashReporterEnabledPreferencesKey) > 0;
            }
            set
            {
                PlayerPrefs.SetInt(CrashReporterEnabledPreferencesKey, (value) ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private bool WasOnBackground
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

        private UInt64 _storedUserId = 0;

        private UInt64 UserId
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

        public CrashReporterBase(MonoBehaviour behaviour, IHttpClient client, 
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

            _wasActiveInLastSession = !WasOnBackground && IsEnabled;
        }

        public void Enable()
        {
            IsEnabled = true;
            LogCallbackHandler.RegisterLogCallback(HandleLog);
            OnEnable();
            Check();

            if(_updateCoroutine == null)
            {
                _updateCoroutine = _behaviour.StartCoroutine(UpdateCoroutine());
            }

#if CRASH_REPORTER_TEST_EVENTS
            TrackException(RandomUtils.GetUuid(), "testing exception");
            TrackCrash(new TestReport());
#endif
        }

        protected virtual void OnEnable()
        {
        }

        public void Disable()
        {
            IsEnabled = false;
            if(_updateCoroutine != null)
            {
                _behaviour.StopCoroutine(_updateCoroutine);
                _updateCoroutine = null;
            }

            LogCallbackHandler.RegisterLogCallback(HandleLog);
            OnDisable();
        }

        protected virtual void OnDisable()
        {
        }

        public void Destroy()
        {
            LogCallbackHandler.UnregisterLogCallback(HandleLog);
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
            GameObject go = new GameObject();
            go = null;
            go.transform.position = Vector3.zero;
        }

        private void Check()
        {
            CheckPendingCrashes();
            CheckLogs();
        }

        private void SetupCrashHttpRequest(HttpRequest req, string log)
        {
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.Body = new JsonAttrSerializer().Serialize(_crashStorage.Load(log));

            // Crash report requires platform parameter in order to be redirected to the proper S3 storage 
            req.AddQueryParam("platform", new AttrString(_deviceInfo.Platform));

            req.CompressBody = true;
        }

        private void SetupExceptionHttpRequest(HttpRequest req, string log)
        {
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            req.Body = new JsonAttrSerializer().Serialize(_exceptionStorage.Load(log));
            req.CompressBody = true;
        }

        private bool HasExceptionLogs
        {
            get{ return (_exceptionStorage != null && _exceptionStorage.StoredKeys.Length > 0); }
        }

        private bool HasCrashLogs
        {
            get{ return (_crashStorage != null && _crashStorage.StoredKeys.Length > 0); }
        }

        private void CheckPendingCrashes()
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

        private void ClearLastSessionInfo()
        {
            // Clear last memory warning timestamp and set foreground status
            LastMemoryWarningTimestamp = 0;
            WasOnBackground = false;
        }

        private Report CheckMemoryCrash()
        {
            Report memoryCrashReport = null;
            /* *
             * We can assume that we had a memory crash if 
             * the application was closed in foreground in the
             * last session and the BreadcrumbManager hadn't been 
             * cleaned (as in any clean stop. See OnApplicationQuit())
             * */
            if(_breadcrumbManager != null &&
               _breadcrumbManager.OldBreadCrumb != null &&
               _wasActiveInLastSession)
            {
                memoryCrashReport = new OutOfMemoryReport(LastMemoryWarningTimestamp);
            }

            return memoryCrashReport;
        }

        private void CheckLogs()
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

        private bool SendExceptions(string[] storedKeys)
        {
            if(!_sending)
            {
                _sending = true;
                DoSendExceptions(storedKeys);
                return true;
            }
            return false;
        }

        private void DoSendExceptions(string[] storedKeys)
        {
            HttpRequest req = new HttpRequest();
            if(RequestSetup != null)
            {
                RequestSetup(req, UriException);
                req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
                var exceptionLogs = new AttrList();
                foreach(var storedKey in storedKeys)
                {
                    exceptionLogs.Add(_exceptionStorage.Load(storedKey));
                }
                req.Body = new JsonAttrSerializer().Serialize(exceptionLogs);
                req.CompressBody = true;
                _httpClient.Send(req, (HttpResponse resp) => OnExceptionSend(resp, storedKeys));
                _lastSendTimestamp = TimeUtils.Timestamp;
            }
        }

        private void SendCrashLog(string log)
        {
            HttpRequest req = new HttpRequest();
            if(RequestSetup != null)
            {
                RequestSetup(req, UriCrash);
                SetupCrashHttpRequest(req, log);
                _httpClient.Send(req, (HttpResponse resp) => OnCrashSend(resp, log));
            }
        }

        private void OnExceptionSend(HttpResponse resp, string[] storedKeys)
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

        private void OnCrashSend(HttpResponse resp, string log)
        {
            if(!resp.HasError)
            {
                _crashStorage.Remove(log);
            }
        }

        private void HandleLog(string logString, string stackTrace, LogType type)
        {
            bool doHandleLog = false;
            if(type == LogType.Exception && _exceptionLogActive)
            {
                doHandleLog = true;
            }
            
            if(type == LogType.Error && _errorLogActive)
            {
                doHandleLog = true;
            }
            
            if(doHandleLog)
            {
                string exceptionHashSource = logString + stackTrace;
                if(!_uniqueExceptions.Contains(exceptionHashSource))
                {
                    string uuid = RandomUtils.GetUuid();
                    var exception = new SocialPointExceptionLog(uuid, logString, stackTrace, _deviceInfo, UserId);
                    _exceptionStorage.Save(uuid, exception);
                    TrackException(uuid, logString);
                    
                    _uniqueExceptions.Add(exceptionHashSource);
                }
            }
        }

        private void TrackException(string uuid, string logString)
        {
            if(TrackEvent != null)
            {
                var data = new AttrDic();
                var error = new AttrDic();
                data.Set(AttrKeyError, error);
                var mobile = new AttrDic();
                error.Set(AttrKeyUnityException, mobile);
                mobile.SetValue(AttrKeyUuid, uuid);
                mobile.SetValue(AttrKeyMessage, logString);
                mobile.SetValue(AttrKeyType, 0);

                TrackEvent(ExceptionEventName, data);
            }
        }

        private void TrackCrash(Report report)
        {
            if(TrackEvent != null)
            {
                var data = new AttrDic();
                var error = new AttrDic();
                data.Set(AttrKeyError, error);
                var mobile = new AttrDic();
                error.Set(AttrKeyMobile, mobile);
                mobile.SetValue(AttrKeyUuid, report.Uuid);
                mobile.SetValue(AttrKeyTimestamp, report.Timestamp);
                mobile.SetValue(AttrKeyType, report.OutOfMemory ? 1 : 0);

                TrackEvent(CrashEventName, data, (Error err) => {
                    if(err == null || !err.HasError)
                    {
                        CreateCrashLog(report);
                    }
                });
            }
            else
            {
                CreateCrashLog(report);
            }
        }

        private void CreateCrashLog(Report report)
        {
            // Create the log on our storage to be send
            string oldBreadcrumbs = "";
            if(_breadcrumbManager != null)
            {
                oldBreadcrumbs = _breadcrumbManager.OldBreadCrumb;
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

        private void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning += OnMemoryWarning;
            appEvents.WillGoBackground += OnWillGoBackground;
            appEvents.WasOnBackground += OnWillGoForeground;
            appEvents.LevelWasLoaded += OnLevelWasLoaded;
            appEvents.ApplicationQuit += OnApplicationQuit;
        }

        private void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning -= OnMemoryWarning;
            appEvents.WillGoBackground -= OnWillGoBackground;
            appEvents.WasOnBackground -= OnWillGoForeground;
            appEvents.LevelWasLoaded -= OnLevelWasLoaded;
            appEvents.ApplicationQuit -= OnApplicationQuit;
        }

        private void OnMemoryWarning()
        {
            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.Log("Memory Warning");
            }

            // Store memory warning timestamp
            LastMemoryWarningTimestamp = TimeUtils.Timestamp;
        }

        private void OnLevelWasLoaded(int level)
        {
            ClearUniqueExceptions();
        }

        private void OnApplicationQuit()
        {
            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.RemoveData();
            }
        }

        private void OnWillGoBackground()
        {
            WasOnBackground = true;
        }

        private void OnWillGoForeground()
        {
            WasOnBackground = false;
            if(HasExceptionLogs)
            {
                SendExceptions(_exceptionStorage.StoredKeys);
            }
        }

        #endregion
    }
}