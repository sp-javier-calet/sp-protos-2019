using System.IO;
using SocialPoint.IO;
using SocialPoint.Utils;

namespace SocialPoint.Crash
{
    public struct Breadcrumb
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

    public class BreadcrumbManager
    {
        const string LastSessionBreadcrumbsName = "old";
        static bool _initialized;

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
                string breadCrumbDirectoryPath = PathsManager.PersistentDataPath + "/breadcrumb/";

                if(!Directory.Exists(breadCrumbDirectoryPath))
                {
                    Directory.CreateDirectory(breadCrumbDirectoryPath);
                }
                
                if(FileUtils.Exists(BreadcrumbLogPath()))
                {
                    FileUtils.CopyFile(BreadcrumbLogPath(), BreadcrumbLogPath(LastSessionBreadcrumbsName), true);
                }

                using(var file = new StreamWriter(BreadcrumbLogPath(), false))
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
            var breadcrumb = new Breadcrumb(info);
            using(var file = new StreamWriter(BreadcrumbLogPath(), true))
            {
                file.WriteLine(breadcrumb);
            }
        }

        public void RemoveData()
        {
            FileUtils.Delete(BreadcrumbLogPath());
            FileUtils.Delete(BreadcrumbLogPath(LastSessionBreadcrumbsName));
        }

        public string CurrentBreadcrumb
        {
            get
            {
                string path = BreadcrumbLogPath();
                return !FileUtils.Exists(path) ? null : FileUtils.ReadAllText(path);
            }
        }

        public string OldBreadcrumb
        {
            get
            {
                string oldPath = BreadcrumbLogPath(LastSessionBreadcrumbsName);
                return !FileUtils.Exists(oldPath) ? null : FileUtils.ReadAllText(oldPath);
            }
        }

        #endregion
    }
}

