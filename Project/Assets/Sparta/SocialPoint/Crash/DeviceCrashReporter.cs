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
                string logPath = Path.GetDirectoryName(fullCrashPath) + Path.DirectorySeparatorChar + fileName + DeviceCrashReporter.LogExtension;

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
        static extern void native_crashReporter_enable(UIntPtr ctx);

        [DllImport(PluginModuleName)]
        static extern void native_crashReporter_disable(UIntPtr ctx);

        [DllImport(PluginModuleName)]
        static extern void native_crashReporter_forceCrash();

        [DllImport(PluginModuleName)]
        static extern UIntPtr native_crashReporter_create(string path, string version, string separator, string crashExtension, string logExtension);

        [DllImport(PluginModuleName)]
        static extern void native_crashReporter_destroy(UIntPtr ctx);

        public const string CrashesFolder = "/crashes/";
        public const string CrashExtension = ".crash";
        public const string LogExtension = ".logcat";
        public const string FileSeparator = "-";

        string _crashesBasePath;
        UIntPtr _nativeObject;
        string _appVersion;

        public DeviceCrashReporter(ICoroutineRunner runner, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbManager = null, IAlertView alertView = null)
            : base(runner, client, deviceInfo, breadcrumbManager, alertView)
        {
            _appVersion = deviceInfo.AppInfo.Version;
            PathsManager.CallOnLoaded(OnPathsLoaded);
        }

        void OnPathsLoaded()
        {
            _crashesBasePath = PathsManager.PersistentDataPath + CrashesFolder;

            FileUtils.CreateDirectory(_crashesBasePath);

            ReadPendingCrashes();

            // Create native object
            _nativeObject = native_crashReporter_create(_crashesBasePath, _appVersion, FileSeparator, CrashExtension, LogExtension);
        }

        ~DeviceCrashReporter ()
        {
            native_crashReporter_destroy(_nativeObject);
        }

        protected override void OnEnable()
        {
            native_crashReporter_enable(_nativeObject);
        }

        protected override void OnDisable()
        {
            native_crashReporter_disable(_nativeObject);
        }

        protected override void OnDestroy()
        {
            native_crashReporter_disable(_nativeObject);
        }

        public override void ForceCrash()
        {
            native_crashReporter_forceCrash();
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
}
