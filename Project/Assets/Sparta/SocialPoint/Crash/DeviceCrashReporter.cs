using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using SocialPoint.Hardware;
using SocialPoint.IO;
using SocialPoint.Network;
using SocialPoint.Alert;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;

namespace SocialPoint.Crash
{
    public class DeviceCrashReporter : BaseCrashReporter
    {
        class DeviceReport : Report
        {
            string CrashPath  { get; set; }

            string LogPath { get; set; }

            public DeviceReport(string fullCrashPath)
            {
                string fileName = Path.GetFileNameWithoutExtension(fullCrashPath);
                string logPath = DeviceCrashReporter.GetLogPathFromCrashPath(fullCrashPath);

                string[] separators = { DeviceCrashReporter.FileSeparator };
                string[] splitted = fileName.Split(separators, StringSplitOptions.None);

                _timestamp = long.Parse(splitted[0]);
                _crashVersion = (splitted.Length > 1) ? splitted[1] : "";

                CrashPath = fullCrashPath;
                LogPath = logPath;
            }

            static bool TryReadFile(string filePath, out byte[] content)
            {
                bool success = false;
                content = null;

                if(FileUtils.ExistsFile(filePath))
                {
                    try
                    {
                        content = FileUtils.ReadAllBytes(filePath);
                        success = true;
                    }
                    catch(Exception e)
                    {
                        content = Encoding.UTF8.GetBytes(string.Format("Error reading crash file {0} : {1}", filePath, e.Message));
                    }
                }
                return success;
            }

            public override void Remove()
            {
                if(FileUtils.ExistsFile(CrashPath))
                {
                    FileUtils.DeleteFile(CrashPath);
                }

                if(FileUtils.ExistsFile(LogPath))
                {
                    FileUtils.DeleteFile(LogPath);
                }
            }

            public override string StackTrace
            {
                get
                {
                    string stackTrace = null;
                    byte[] content;
                    if(TryReadFile(CrashPath, out content))
                    {
#if UNITY_ANDROID
                        // Base 64 encoding only for Android
                        int base64BufferSize = 4 * (int)Math.Ceiling(content.Length / 3.0);
                        var encoded = new char[base64BufferSize];
                        var size = Convert.ToBase64CharArray(content, 0, content.Length, encoded, 0);
                        content = Encoding.UTF8.GetBytes(encoded, 0, size);
#endif
                        stackTrace = Encoding.UTF8.GetString(content);
                    }
                    return stackTrace;
                }
            }

            long _timestamp;

            public override long Timestamp
            {
                get
                {
                    return _timestamp;
                }
            }

            string _crashVersion;

            public override string CrashVersion
            {
                get
                {
                    return _crashVersion;
                }
            }

            public override string Log
            {
                get
                {
                    string logContent = string.Empty;
                    byte[] content;
                    if(TryReadFile(LogPath, out content))
                    {
                        logContent = Encoding.UTF8.GetString(content);
                    }
                    return logContent;
                }
            }

            public override string ToString()
            {
                return "[Crash " + Uuid + " : " + Timestamp + " Paths [ " + CrashPath + " " + LogPath + " ]]";  
            }
        }

        #if UNITY_ANDROID
        const string PluginModuleName = "sp_unity_crash_reporter";
        #else
        const string PluginModuleName = "__Internal";
        #endif

        /* Native plugin interface */
        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporterEnable(UIntPtr ctx);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporterDisable(UIntPtr ctx);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporterForceCrash();

        [DllImport(PluginModuleName)]
        static extern UIntPtr SPUnityCrashReporterCreate(string crashPath, string version, string separator, string crashExtension, string logExtension, string gameObject);

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporterDestroy(UIntPtr ctx);

        public const string CrashesFolder = "/crashes/";
        public const string CrashExtension = ".crash";
        public const string LogExtension = ".logcat";
        public const string FileSeparator = "-";

        string _crashesBasePath;
        UIntPtr _nativeObject;
        string _appVersion;
        DeviceCrashReporterListener _listener;

        public DeviceCrashReporter(ICoroutineRunner runner, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbManager = null, IAlertView alertView = null)
            : base(runner, client, deviceInfo, breadcrumbManager, alertView)
        {
            _appVersion = deviceInfo.AppInfo.Version;
            PathsManager.CallOnLoaded(OnPathsLoaded);
        }

        void OnPathsLoaded()
        {
            _crashesBasePath = PathsManager.TemporaryDataPath + CrashesFolder;

            FileUtils.CreateDirectory(_crashesBasePath);

            ReadPendingCrashes();

            // Create listener
            var listenerGo = new GameObject("SocialPoint.DeviceCrashReporterListener");
            GameObject.DontDestroyOnLoad(listenerGo);
            _listener = listenerGo.AddComponent<DeviceCrashReporterListener>();
            _listener.SetBreadcrumbManager(_breadcrumbManager);

            // Create native object
            _nativeObject = SPUnityCrashReporterCreate(_crashesBasePath, _appVersion, FileSeparator, CrashExtension, LogExtension, _listener.gameObject.name);
        }

        public static string GetLogPathFromCrashPath(string fullCrashPath)
        {
            string fileName = Path.GetFileNameWithoutExtension(fullCrashPath) + DeviceCrashReporter.LogExtension;
            string logPath = Path.Combine(Path.GetDirectoryName(fullCrashPath), fileName);
            return logPath;
        }

        public static string ReadStackTraceFromCrashPath(string fullCrashPath)
        {
            var report = new DeviceReport(fullCrashPath);
            return report.StackTrace;
        }

        ~DeviceCrashReporter ()
        {
            SPUnityCrashReporterDestroy(_nativeObject);
        }

        protected override void OnEnable()
        {
            SPUnityCrashReporterEnable(_nativeObject);
        }

        protected override void OnDisable()
        {
            SPUnityCrashReporterDisable(_nativeObject);
        }

        protected override void OnDestroy()
        {
            SPUnityCrashReporterDisable(_nativeObject);
        }

        public override void ForceCrash()
        {
            SPUnityCrashReporterForceCrash();
        }

        protected override List<Report> GetPendingCrashes()
        {
            var reports = new List<Report>();
            try
            {
                // Iterates over all files in the crashes folder
                var dir = new DirectoryInfo(_crashesBasePath);
                FileInfo[] info = dir.GetFiles();

                foreach(FileInfo f in info)
                {
                    // Creates a report for each .crash/.logcat pair
                    if(f.Extension == CrashExtension)
                    {
                        var report = new DeviceReport(f.FullName);
                        report.Uuid = f.Name;
                        reports.Add(report);
                    }
                }
            }
            catch(DirectoryNotFoundException)
            {
                Debug.LogError(string.Format("Crash folder '{0}' not found.", _crashesBasePath));
            }
            catch(Exception e)
            {
                Debug.LogError(string.Format("Exception getting pending crashes: {0}", e));
            }

            return reports;
        }
    }

    class DeviceCrashReporterListener : MonoBehaviour
    {
        BreadcrumbManager _breadcrumbManager;

        void LeaveBreadcrumb(string message)
        {
            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.Log(message);
            }
        }

        public void SetBreadcrumbManager(BreadcrumbManager breadcrumbManager)
        {
            _breadcrumbManager = breadcrumbManager;
        }

        //If this function is called from native code, it means that a crash was detected but the Unity app is still running (non-killing crash)
        public void OnCrashDumped(string path)
        {
            DebugUtils.LogWarning("OnCrashDumped '" + path + "'");

            //A non-killing crash may not "crash" the app, but the native crash detection may stop working after it, and future crashes may not be tracked.
            //We leave a breadcrumb to know that one of them was detected.
            LeaveBreadcrumb("Non-Killing Crash Detected");

            if(FileUtils.ExistsFile(path))
            {
                LeaveBreadcrumb("Non-Killing Crash StackTrace - Start");
                LeaveBreadcrumb(DeviceCrashReporter.ReadStackTraceFromCrashPath(path));
                LeaveBreadcrumb("Non-Killing Crash StackTrace - End");

                DebugUtils.LogWarning("Removing non-killing crash file '" + path + "'...");
                FileUtils.DeleteFile(path);
            }

            string logPath = DeviceCrashReporter.GetLogPathFromCrashPath(path);
            FileUtils.DeleteFile(logPath);

            if(_breadcrumbManager != null)
            {
                _breadcrumbManager.DumpToFile();
            }
        }

        public void DebugLog(string message)
        {
            Debug.Log(message);
        }
    }
}
