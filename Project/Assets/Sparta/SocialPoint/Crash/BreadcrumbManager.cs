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
                string breadCrumbDirectoryPath = BreadcrumbDirectoryPath();
                string breadCrumbLogPath = BreadcrumbLogPath();

                //*** TEST Commented to leave directory and file creation to native
                //FileUtils.CreateDirectory(breadCrumbDirectoryPath);
                UnityEngine.Debug.Log("*** TEST BreadCrumbDirectoryPath: " + breadCrumbDirectoryPath);
                UnityEngine.Debug.Log("*** TEST BreadcrumbLogPath: " + breadCrumbLogPath);

                if(FileUtils.ExistsFile(breadCrumbLogPath))
                {
                    UnityEngine.Debug.Log("*** TEST Old breadcrumbs file found. Content: " + FileUtils.ReadAllText(BreadcrumbManager.BreadcrumbLogPath()));
                    FileUtils.CopyFile(breadCrumbLogPath, BreadcrumbLogPath(LastSessionBreadcrumbsName), true);
                }
                else
                {
                    UnityEngine.Debug.Log("*** TEST Old breadcrumbs file NOT found");
                }

                using(var file = new StreamWriter(breadCrumbLogPath, false))
                {
                    file.WriteLine(string.Format("Breadcrumb log {0}", TimeUtils.GetTime(TimeUtils.Timestamp).ToString("yyyy/MM/dd HH:mm:ss")));
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
            PathsManager.CallOnLoaded(InitializeBreadcrumbFile);

            //*** TEST
            UnityEngine.Debug.Log("*** TEST Leaving some breadcrumbs");
            Log("Test Breadcrumb 1");
            Log("Test Breadcrumb 2");
            Log("Test Breadcrumb 3");
        }

        public void Log(string info)
        {
            Breadcrumb breadcrumb = new Breadcrumb(info);
            /*
            using(var file = new StreamWriter(BreadcrumbLogPath(), true))
            {
                file.WriteLine(breadcrumb);
            }*/
            BreadcrumbManagerBinding.Log(breadcrumb.ToString());
        }

        public void RemoveData()
        {
            //*** TEST booleans to know if files existed
            bool breadcrumbsExisted = false;
            bool oldBreadcrumbsExisted = false;

            breadcrumbsExisted = FileUtils.DeleteFile(BreadcrumbLogPath());
            oldBreadcrumbsExisted = FileUtils.DeleteFile(BreadcrumbLogPath(LastSessionBreadcrumbsName));

            //*** TEST
            if(breadcrumbsExisted)
                UnityEngine.Debug.Log("*** TEST Breadcrumb file deleted");
            else
                UnityEngine.Debug.Log("*** TEST Breadcrumb file didn't exist");
            if(oldBreadcrumbsExisted)
                UnityEngine.Debug.Log("*** TEST OLD Breadcrumb file deleted");
            else
                UnityEngine.Debug.Log("*** TEST OLD Breadcrumb file didn't exist");
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
                return !HasOldBreadcrumb ? null : FileUtils.ReadAllText(oldPath);
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
    }
}

