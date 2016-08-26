using System.IO;
using SocialPoint.Base;
using SocialPoint.IO;
using SocialPoint.Utils;
using System;

namespace SocialPoint.Crash
{
    public sealed class BreadcrumbManager : IBreadcrumbManager, Log.IBreadcrumbLogger
    {
        struct Breadcrumb
        {
            readonly long timestamp;
            readonly string info;

            public Breadcrumb(string info)
            {
                timestamp = TimeUtils.Timestamp;
                this.info = info;
            }

            public override string ToString()
            {
                return string.Format("{0} \t{1}", TimeUtils.GetTime(timestamp).ToString("yyyy/MM/dd HH:mm:ss"), info);
            }
        }

        const string LastSessionBreadcrumbsName = "old";
        static bool _initialized;

        public Exception LogException {get; set;}

        /*
         * InitializeBreadcrumbFile function ensures that every 
         * BreadcrumbManager instanced in the same game session  
         * uses the same breadcrumb file.
         */
        static void InitializeBreadcrumbFile()
        {
            if(!_initialized)
            {
                _initialized = true;
                string breadCrumbDirectoryPath = BreadcrumbDirectoryPath();
                string breadCrumbFilename = BreadcrumbFilename();
                string breadCrumbLogPath = BreadcrumbLogPath();

                BreadcrumbManagerBinding.SetDumpFilePath(breadCrumbDirectoryPath, breadCrumbFilename);

                if(FileUtils.ExistsFile(breadCrumbLogPath))
                {
                    FileUtils.CopyFile(breadCrumbLogPath, BreadcrumbLogPath(LastSessionBreadcrumbsName), true);
                }
            }
        }

        public static string BreadcrumbDirectoryPath()
        {
            return PathsManager.AppPersistentDataPath + "/breadcrumb/";
        }

        public static string BreadcrumbFilename(string uuid = "")
        {
            return string.Format("Breadcrumb{0}.log", uuid != "" ? "-" + uuid : "");
        }

        public static string BreadcrumbLogPath(string uuid = "")
        {
            return BreadcrumbDirectoryPath() + BreadcrumbFilename(uuid);
        }

        #region BreadcrumbManager implementation

        public BreadcrumbManager()
        {
            Log("Breadcrumb Log");
            PathsManager.CallOnLoaded(InitializeBreadcrumbFile);
        }

        public void Log(string info)
        {
            Breadcrumb breadcrumb = new Breadcrumb(info);
            BreadcrumbManagerBinding.Log(breadcrumb.ToString());
        }

        public void DumpToFile()
        {
            BreadcrumbManagerBinding.DumpToFile();
        }

        public void RemoveData()
        {
            FileUtils.DeleteFile(BreadcrumbLogPath());
            FileUtils.DeleteFile(BreadcrumbLogPath(LastSessionBreadcrumbsName));
        }

        public string CurrentBreadcrumb
        {
            get
            {
                string path = BreadcrumbLogPath();
                return !FileUtils.ExistsFile(path) ? null : FileUtils.ReadAllText(path);
            }
        }

        public string OldBreadcrumb
        {
            get
            {
                string oldPath = BreadcrumbLogPath(LastSessionBreadcrumbsName);
                return !FileUtils.ExistsFile(oldPath) ? null : FileUtils.ReadAllText(oldPath);
            }
        }

        public bool HasOldBreadcrumb
        {
            get
            {
                return FileUtils.ExistsFile(BreadcrumbLogPath(LastSessionBreadcrumbsName));
            }
        }

        #endregion


        #region Log.IBreadcrumbLogger implementation

        public void Leave(string message)
        {
            Log(message);
        }
        #endregion
    }
}

