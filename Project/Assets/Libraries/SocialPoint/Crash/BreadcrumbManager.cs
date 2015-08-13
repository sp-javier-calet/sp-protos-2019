using System;
using System.IO;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.IO;
using UnityEngine;
using SPDebug = SocialPoint.Base.Debug;

namespace SocialPoint.Crash
{
    public enum BreadcrumbType
    {
        AppEnterBackground,
        AppEnterForeground,
        AppBecomeActive,
        AppResignActive,
        AppWillTerminate,
        AppMemoryWarning,
        AppWillDestroy,
        ConnectionReachable,
        ConnectionNotReachable,
        CommandError,
        Custom,
        GameOpened,
        GameLoaded,
        HttpResponseError,
        IAP,
        OpenedPopup,
        RestartGame,
        StartTutorial
    }

    public struct Breadcrumb
    {
        long timestamp;
        BreadcrumbType type;
        string info;

        public Breadcrumb(BreadcrumbType type, string info = "")
        {
            timestamp = TimeUtils.Timestamp;
            this.type = type;
            this.info = info;
        }

        public override string ToString()
        {
            return string.Format("{0} {1} \t{2}", TimeUtils.GetTime(timestamp).ToString("yyyy/MM/dd HH:mm:ss"), type, info);
        }
    }

    public class BreadcrumbManager
    {
        private const string LastSessionBreadcrumbsName = "old";
        private static bool _initialized = false;

        /*
         * InitializeBreadcrumbFile function ensures that every 
         * BreadcrumbManager instanced in the same game session  
         * uses the same breadcrumb file.
         */
        private static void InitializeBreadcrumbFile()
        {
            if(!_initialized)
            {
                _initialized = true;
                string breadCrumbDirectoryPath = PathsManager.PersistentDataPath + "/breadcrumb/";

                if(Directory.Exists(breadCrumbDirectoryPath) == false)
                {
                    Directory.CreateDirectory(breadCrumbDirectoryPath);
                }
                
                if(FileUtils.Exists(BreadcrumbLogPath()))
                {
                    FileUtils.CopyFile(BreadcrumbLogPath(), BreadcrumbLogPath(LastSessionBreadcrumbsName), true);
                }

                using(StreamWriter file = new StreamWriter(BreadcrumbLogPath(), false))
                {
                    file.WriteLine(string.Format("Breadcrumb log {0}", TimeUtils.GetTime(TimeUtils.Timestamp).ToString("yyyy/MM/dd HH:mm:ss")));
                }
            }
        }

        public static string BreadcrumbLogPath(string uuid = "")
        {
            return string.Format("{0}/breadcrumb/Breadcrumb{1}.log", PathsManager.PersistentDataPath,
                                                                        uuid != "" ? "-" + uuid : "");
        }

        #region BreadcrumbManager implementation

        public BreadcrumbManager()
        {
            InitializeBreadcrumbFile();
        }

        public void Log(BreadcrumbType type, string info = "")
        {
            Breadcrumb breadcrumb = new Breadcrumb(type, info);
            using(StreamWriter file = new StreamWriter(BreadcrumbLogPath(), true))
            {
                file.WriteLine(breadcrumb);
            }
        }

        public void RemoveData()
        {
            FileUtils.Delete(BreadcrumbLogPath());
            FileUtils.Delete(BreadcrumbLogPath(LastSessionBreadcrumbsName));
        }

        public string OldBreadCrumb
        {
            get{
                string oldPath = BreadcrumbLogPath(LastSessionBreadcrumbsName);
                if(!FileUtils.Exists(oldPath))
                {
                    return null;
                }

                using(StreamReader stream = new StreamReader(oldPath))
                {
                    return stream.ReadToEnd();
                }
            }
        }

        #endregion
    }
}

