using System;
using System.IO;
using SocialPoint.Utils;
using SocialPoint.IO;
using UnityEngine;

namespace SocialPoint.Crash
{

    public struct Breadcrumb
    {
        long timestamp;
        string info;

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

        public void Log(string info)
        {
            Breadcrumb breadcrumb = new Breadcrumb(info);
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

