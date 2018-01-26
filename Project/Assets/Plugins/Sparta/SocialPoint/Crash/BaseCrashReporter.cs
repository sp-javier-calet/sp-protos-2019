using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Utils;
using SocialPoint.Login;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SocialPoint.Crash
{
    /*
     * Crash reporter Base implementation
     */
    public class BaseCrashReporter : ICrashReporter , IUpdateable
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
            const string AttrKeyHandled = "handled";

            public SocialPointExceptionLog(string uuid, string log, string stacktrace, IDeviceInfo deviceInfo, UInt64 userId, bool handled)
                : base(deviceInfo, userId)
            {
                var exception = new AttrDic();
                exception.Set(AttrKeyUuid, new AttrString(uuid));
                exception.Set(AttrKeyLog, new AttrString(log));
                exception.Set(AttrKeyStacktrace, new AttrString(stacktrace));
                exception.Set(AttrKeyLoadedLevelName, new AttrString(SceneManager.GetActiveScene().name));
                exception.Set(AttrKeyHandled, new AttrBool(handled));
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
            string _crashVersion;
            readonly string _message = "APP KILLED IN FOREGROUND BECAUSE OF LOW MEMORY.";

            public OutOfMemoryReport(long timestamp, string crashVersion)
            {
                _timestamp = timestamp;
                _crashVersion = crashVersion;
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

            public override string CrashVersion
            {
                get
                {
                    return _crashVersion;
                }
            }
        }

        #endregion

        enum ReportSendType
        {
            BeforeLogin,
            AfterLogin
        }

        public delegate void TrackEventDelegate(string eventName, AttrDic data = null, ErrorDelegate del = null);

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
        const string AttrKeyCrashBuildId = "crash_build_id";

        // Player preferences keys
        const string WasOnBackgroundPreferencesKey = "app_gone_background";
        const string LastMemoryWarningPreferencesKey = "last_memory_warning";
        const string LastAppVersionKey = "last_app_version";
        const string CrashReporterEnabledPreferencesKey = "crash_reporter_enabled";

        // Events
        const string ExceptionEventName = "errors.unity_exception";
        const string CrashEventName = "errors.mobile_crash_triggered";

        IHttpClient _httpClient;
        IDeviceInfo _deviceInfo;
        readonly FileAttrStorage _exceptionStorage;
        FileAttrStorage _crashStorage;
        List<Report> _pendingReports;
        HashSet<string> _uniqueExceptions;

        protected IBreadcrumbManager _breadcrumbManager;

        public TrackEventDelegate TrackEvent;
        public ILoginData LoginData;

        public const float DefaultSendInterval = 20.0f;
        public const bool DefaultExceptionLogActive = true;
        public const bool DefaultErrorLogActive = true;
        public const bool DefaultEnableSendingCrashesBeforeLogin = false;
        public const int DefaultNumRetriesBeforeSendingCrashBeforeLogin = 3;

        bool _wasActiveInLastSession;
        bool _appWasUpdated;
        bool _memoryWarningReceivedThisSession;
        bool _exceptionLogActive = DefaultExceptionLogActive;
        bool _errorLogActive = DefaultErrorLogActive;
        bool _enableSendingCrashesBeforeLogin = DefaultEnableSendingCrashesBeforeLogin;
        int _numRetriesBeforeSendingCrashBeforeLogin = DefaultNumRetriesBeforeSendingCrashBeforeLogin;

        public float SendInterval
        {
            get{ return _currentSendInterval; }
            set
            { 
                _currentSendInterval = value; 
            }
        }

        IUpdateScheduler _updateScheduler;
        IAlertView _alertViewPrototype;

        float _currentSendInterval = DefaultSendInterval;
        bool _sending;
        bool _running;

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

        // Exceptions limit
        public const int DefaultExceptionsBatchSize = 50;
        int _exceptionsBatchSize = DefaultExceptionsBatchSize;

        public int ExceptionsBatchSize
        {
            get { return _exceptionsBatchSize; }
            set { _exceptionsBatchSize = value; }
        }

        public const int DefaultMaxStoredExceptions = 200;
        int _maxStoredExceptions = DefaultMaxStoredExceptions;

        public int MaxStoredExceptions
        {
            get { return _maxStoredExceptions; }
            set { _maxStoredExceptions = value; }
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

        static string LastAppVersion
        {
            get
            {
                return PlayerPrefs.GetString(LastAppVersionKey, String.Empty);
            }
            set
            {
                PlayerPrefs.SetString(LastAppVersionKey, value);
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
                if(LoginData != null)
                {
                    var userId = LoginData.UserId;
                    if(userId != 0)
                    {
                        _storedUserId = userId;
                    }
                }

                return _storedUserId;
            }
        }

        public BaseCrashReporter(IUpdateScheduler updateScheduler, IHttpClient client, 
                                 IDeviceInfo deviceInfo, IBreadcrumbManager breadcrumbManager = null, IAlertView alertView = null)
        {
            _updateScheduler = updateScheduler;
            _running = false;
            _httpClient = client;
            _deviceInfo = deviceInfo;
            _alertViewPrototype = alertView;

            _exceptionStorage = new FileAttrStorage(FileUtils.Combine(PathsManager.AppPersistentDataPath, "logs/exceptions"));
            _crashStorage = new FileAttrStorage(FileUtils.Combine(PathsManager.AppPersistentDataPath, "logs/crashes"));
           
            //only used when crash detected
            _breadcrumbManager = breadcrumbManager;
            if(_breadcrumbManager == null)
            {
                _breadcrumbManager = new EmptyBreadcrumbManager();
            }

            _uniqueExceptions = new HashSet<string>();

            _pendingReports = new List<Report>();

            _wasActiveInLastSession = !WasOnBackground && WasEnabled;

            CheckAppVersion();
        }

        public bool IsEnabled
        {
            get
            {
                return _running;
            }
        }

        public void Enable()
        {
            if(IsEnabled)
            {
                return;
            }

            WasEnabled = true;
            Application.logMessageReceived += HandleLog;

            if(_updateScheduler != null)
            { 
                _updateScheduler.Add(this, UpdateableTimeMode.GameTimeUnscaled, SendInterval);
                _running = true;
            }

            OnEnable();
        }

        protected virtual void OnEnable()
        {
        }

        public void Disable()
        {
            WasEnabled = false;
            Application.logMessageReceived -= HandleLog;

            if(_updateScheduler != null)
            { 
                _updateScheduler.Remove(this);
                _running = false;
            }

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

        public void ReportHandledException(Exception e)
        {
            Log.w("Reporting Handled Exception: " + e);
            TrackException(e.ToString(), e.StackTrace, true);
        }

        public void ClearUniqueExceptions()
        {
            _uniqueExceptions.Clear();
        }

        void CheckAppVersion()
        {
            //Check if updated app
            string lastAppVersion = LastAppVersion;
            string currentVersion = _deviceInfo.AppInfo.Version;
            bool newApp = String.IsNullOrEmpty(lastAppVersion);
            _appWasUpdated = (lastAppVersion != currentVersion) && !newApp;

            //Breadcrumb for version
            _breadcrumbManager.Log("App Version: " + currentVersion);
            if(_appWasUpdated)
            {
                _breadcrumbManager.Log("App Was Updated. Last Version: " + lastAppVersion);
            }

            //Update saved version data
            LastAppVersion = _deviceInfo.AppInfo.Version;
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

        public bool HasBreadcrumbException
        {
            get{ return (_breadcrumbManager != null && _breadcrumbManager.LogException != null); }
        }

        protected void ReadPendingCrashes()
        {
            _pendingReports = GetPendingCrashes();

            if(_pendingReports.Count > 0)
            {
                for(int i = 0, _pendingReportsCount = _pendingReports.Count; i < _pendingReportsCount; i++)
                {
                    Report report = _pendingReports[i];
                    AddRetry(report.Uuid);
                }
            }
            else
            {
                // If there are no new crashes, we can check some saved status to detect other crashes.
                // But if app was just updated, ignore some checks because app may have been killed to start it with new version.
                if(!_appWasUpdated)
                {
                    //Check for a memory crash
                    Report memoryCrashReport = CheckMemoryCrash();
                    if(memoryCrashReport != null)
                    {
                        _pendingReports.Add(memoryCrashReport);
                        AddRetry(memoryCrashReport.Uuid);
                    }
                }
            }

            if(HasCrashLogs)
            {
                for(int i = 0, _crashStorageStoredKeysLength = _crashStorage.StoredKeys.Length; i < _crashStorageStoredKeysLength; i++)
                {
                    var log = _crashStorage.StoredKeys[i];
                    AddRetry(log);
                }
            }
        }

        void SendCrashesAfterLogin(Action callback = null)
        {
            DebugUtils.Assert(IsEnabled, "CrashReporter should be already enabled after login"); 
            SendCrashesWithSafeCallback(ReportSendType.AfterLogin, callback);
        }

        public void SendCrashesBeforeLogin(Action callback)
        {
            DebugUtils.Assert(IsEnabled, "CrashReporter should be enabled before login"); 
            SendCrashesWithSafeCallback(ReportSendType.BeforeLogin, callback);
        }

        void SendCrashesWithSafeCallback(ReportSendType reportSendType, Action callback)
        {
            SendCrashes(reportSendType, () => {
                if(callback != null)
                {
                    callback();
                }
            });
        }

        void SendCrashes(ReportSendType reportSendType, Action callback)
        {
            var steps = new StepCallbackBuilder(callback);

            SendTrackedCrashes(reportSendType, steps.Add());
            SendPendingCrashes(reportSendType, steps.Add());

            //Clear last session data only after all crashs types were tracked (Before and After login)
            if(reportSendType == ReportSendType.AfterLogin)
            {
                ClearLastSessionInfo();
            }

            steps.Ready();
        }

        void SendTrackedCrashes(ReportSendType reportSendType, Action callback)
        {
            if(HasCrashLogs)
            {
                var steps = new StepCallbackBuilder(callback);

                for(int i = 0, _crashStorageStoredKeysLength = _crashStorage.StoredKeys.Length; i < _crashStorageStoredKeysLength; i++)
                {
                    var log = _crashStorage.StoredKeys[i];
                    if(reportSendType == GetReportSendType(log))
                    {
                        SendCrashLog(log, steps.Add());
                    }
                }

                steps.Ready();
            }
            else if(callback != null)
            {
                callback();
            }
        }

        void SendPendingCrashes(ReportSendType reportSendType, Action callback)
        {
            if(_pendingReports.Count > 0)
            {
                var steps = new StepCallbackBuilder(callback);

                for(int i = 0, _pendingReportsCount = _pendingReports.Count; i < _pendingReportsCount; i++)
                {
                    Report report = _pendingReports[i];
                    if(reportSendType == GetReportSendType(report.Uuid))
                    {
                        //trackcrash will create the log if is success
                        TrackCrash(report, steps.Add());
                    }
                }
                steps.Ready();
            }
            else
            {
                if(callback != null)
                {
                    callback();
                }
            }
        }

        void ClearLastSessionInfo()
        {
            // Clear last memory warning timestamp if it is from last session 
            if(!_memoryWarningReceivedThisSession)
            {
                LastMemoryWarningTimestamp = 0;
            }

            // Set foreground status
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
            if(_breadcrumbManager.HasOldBreadcrumb &&
               _wasActiveInLastSession)
            {
                memoryCrashReport = new OutOfMemoryReport(LastMemoryWarningTimestamp, _deviceInfo.AppInfo.Version);
            }

            return memoryCrashReport;
        }

        void SendExceptionLogs()
        {
            if(HasExceptionLogs)
            {
                /* Send a max of MaxExceptionsRequestSize exceptions per request, 
                 * to avoid big http requests and timeouts */
                var storedKeys = _exceptionStorage.StoredKeys;
                int len = Math.Min(storedKeys.Length, ExceptionsBatchSize);
                var keysToSend = new string[len];
                Array.Copy(storedKeys, keysToSend, len);

                SendExceptions(keysToSend);
            }

            if(HasBreadcrumbException)
            {
                ReportHandledException(_breadcrumbManager.LogException);
                _breadcrumbManager.LogException = null;
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
            if(LoginData == null)
            {
                return;
            }
            var req = new HttpRequest();
            try
            {
                LoginData.SetupHttpRequest(req, UriException);
            }
            catch(Exception e)
            {
                CatchException(e);
            }
            req.AddHeader(HttpRequest.ContentTypeHeader, HttpRequest.ContentTypeJson);
            var exceptionLogs = new AttrList();
            for(int i = 0, storedKeysLength = storedKeys.Length; i < storedKeysLength; i++)
            {
                var storedKey = storedKeys[i];
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
        }

        void SendCrashLog(string log, Action callback)
        {
            if(LoginData == null)
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
                LoginData.SetupHttpRequest(req, UriCrash);
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
                for(int i = 0, storedKeysLength = storedKeys.Length; i < storedKeysLength; i++)
                {
                    var key = storedKeys[i];
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
            bool dotrack = type == LogType.Exception && _exceptionLogActive;
			dotrack |= type == LogType.Error && _errorLogActive;
            
			if(dotrack)
            {
				bool handled = type == LogType.Error;
                TrackException(logString, stackTrace, handled);
            }

            if(_alertViewPrototype != null && type == LogType.Exception)
            {
				CreateAlertView(logString, stackTrace, type, dotrack);
            }
        }

        /// <summary>
        /// Creates an alert view/popup if needed/allowed. (Depends on LogType and DEBUG compilation mode)
        /// </summary>
        void CreateAlertView(string logString, string stackTrace, LogType type, bool exceptionTracked)
        {
#if DEBUG
            try
            {                    
                var alert = (IAlertView)_alertViewPrototype.Clone();
                alert.Title = type.ToString();
                alert.Message = logString + "\n" + stackTrace;
                alert.Signature = "Exception tracked by Crash Reporter? " + exceptionTracked;
                alert.Buttons = new []{ "OK" };
                alert.Show(result => alert.Dispose());
            }
            catch(Exception e)
            {
                Log.e("Exception while creating Alert View - " + e.Message);
            }
#endif
        }

        void TrackException(string logString, string stackTrace, bool handled = false)
        {
            string exceptionHashSource = logString + stackTrace;
            if(_uniqueExceptions.Contains(exceptionHashSource))
            {
                return;
            }
            
            string uuid = RandomUtils.GetUuid();
            if(_exceptionStorage.StoredKeys.Length < MaxStoredExceptions)
            {
                var exception = new SocialPointExceptionLog(uuid, logString, stackTrace, _deviceInfo, UserId, handled);
                _exceptionStorage.Save(uuid, exception);
            }

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
                mobile.SetValue(AttrKeyType, handled ? 1 : 0);

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
                mobile.SetValue(AttrKeyCrashBuildId, report.CrashVersion);

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
            string oldBreadcrumbs = _breadcrumbManager.OldBreadcrumb;

            var crashLog = new SocialPointCrashLog(report, _deviceInfo, UserId, oldBreadcrumbs);
            _crashStorage.Save(report.Uuid, crashLog);

            // Try to send current crash and remove crash data. 
            // The CrashLog is stored and can be sent again if fails
            SendCrashLog(report.Uuid, callback);

            report.Remove(); // we remove the report in order to not track it again :)
        }

        #region App Events

        void ConnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning += OnMemoryWarning;
            appEvents.WillGoBackground.Add(0, OnWillGoBackground);
            appEvents.WasOnBackground.Add(0, OnWillGoForeground);
            SceneManager.sceneLoaded += OnSceneLoaded;
            appEvents.ApplicationQuit += OnApplicationQuit;
            appEvents.GameWasLoaded.Add(0, OnGameWasLoaded);
        }

        void DisconnectAppEvents(IAppEvents appEvents)
        {
            appEvents.ReceivedMemoryWarning -= OnMemoryWarning;
            appEvents.WillGoBackground.Remove(OnWillGoBackground);
            appEvents.WasOnBackground.Remove(OnWillGoForeground);
            SceneManager.sceneLoaded -= OnSceneLoaded;
            appEvents.ApplicationQuit -= OnApplicationQuit;
            appEvents.GameWasLoaded.Remove(OnGameWasLoaded);
        }

        void OnMemoryWarning()
        {
            // Store memory warning timestamp
            LastMemoryWarningTimestamp = TimeUtils.Timestamp;
            _memoryWarningReceivedThisSession = true;

            _breadcrumbManager.Log("Memory Warning");
            _breadcrumbManager.DumpToFile();
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ClearUniqueExceptions();
        }

        void OnGameWasLoaded()
        {
            SendCrashesAfterLogin();
        }

        void OnApplicationQuit()
        {
            _breadcrumbManager.RemoveData();
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
            Log.x(e);
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

        #region IUpdateable implementation

        public void Update()
        {
            SendExceptionLogs();
        }

        #endregion
    }
}
