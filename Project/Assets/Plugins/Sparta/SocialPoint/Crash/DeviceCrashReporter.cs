using AOT;
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

namespace SocialPoint.Crash
{
    public class DeviceCrashReporter : BaseCrashReporter
    {
        class DeviceReport : Report
        {
            string CrashPath { get; set; }

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
        const string PluginModuleName = "sp_unity_crash";
#else
        const string PluginModuleName = "__Internal";
#endif

        /* Native plugin interface */
        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporter_Enable();

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporter_Disable();

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporter_ForceCrash();

        [DllImport(PluginModuleName)]
        static extern void SPUnityCrashReporter_Create(string crashPath, string version, string separator, string crashExtension, string logExtension, Action<string> callback);

        public const string CrashesFolder = "/crashes/";
        public const string CrashExtension = ".crash";
        public const string LogExtension = ".logcat";
        public const string FileSeparator = "-";

        string _crashesBasePath;
        string _appVersion;
        static DeviceCrashReporter _instance;

        public DeviceCrashReporter(IUpdateScheduler updateScheduler, IHttpClient client, IDeviceInfo deviceInfo, IBreadcrumbManager breadcrumbManager = null, IAlertView alertView = null)
            : base(updateScheduler, client, deviceInfo, breadcrumbManager, alertView)
        {
#if !ADMIN_PANEL && !UNITY_EDITOR && !ENABLE_IL2CPP
            throw new InvalidOperationException("Crash reporter will only work reliably with the IL2CPP scripting backend.");
#endif
            if(_instance != null)
            {
                throw new InvalidOperationException("There can only be one DeviceCrashReporter instance.");
            }
            _instance = this;
            _appVersion = deviceInfo.AppInfo.Version;
            PathsManager.CallOnLoaded(OnPathsLoaded);
        }

        void OnPathsLoaded()
        {
            _crashesBasePath = PathsManager.TemporaryDataPath + CrashesFolder;

            FileUtils.CreateDirectory(_crashesBasePath);

            ReadPendingCrashes();

            // Create native object
            SPUnityCrashReporter_Create(_crashesBasePath, _appVersion, FileSeparator, CrashExtension, LogExtension, CrashDumpledCallback);
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

        protected override void OnEnable()
        {
            SPUnityCrashReporter_Enable();
        }

        protected override void OnDisable()
        {
            SPUnityCrashReporter_Disable();
        }

        protected override void OnDestroy()
        {
            SPUnityCrashReporter_Disable();
        }

        public override void ForceCrash()
        {
            SPUnityCrashReporter_ForceCrash();
        }

        [MonoPInvokeCallback(typeof(Action<string, string>))]
        static void CrashDumpledCallback(string path)
        {
            if(_instance != null)
            {
                _instance.OnCrashDumped(path);
            }
        }

        void OnCrashDumped(string path)
        {
            Log.w("OnCrashDumped '" + path + "'");
            _breadcrumbManager.Log("OnCrashDumped");
            _breadcrumbManager.DumpToFile();
        }

        protected override List<Report> GetPendingCrashes()
        {
            var reports = new List<Report>();
            try
            {
                // Iterates over all files in the crashes folder
                var dir = new DirectoryInfo(_crashesBasePath);
                FileInfo[] info = dir.GetFiles();

                for(int i = 0, infoLength = info.Length; i < infoLength; i++)
                {
                    // Creates a report for each .crash/.logcat pair
                    FileInfo f = info[i];
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
                Log.e(string.Format("Crash folder '{0}' not found.", _crashesBasePath));
            }
            catch(Exception e)
            {
                Log.e(string.Format("Exception getting pending crashes: {0}", e));
            }

            return reports;
        }
    }
}
