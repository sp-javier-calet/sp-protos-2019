
namespace SocialPoint.Crash
{
    public class EmptyBreadcrumbManager : IBreadcrumbManager
    {
        public void Log(string info)
        {
        }

        public void DumpToFile()
        {
        }

        public void RemoveData()
        {
        }

        public string CurrentBreadcrumb
        {
            get
            {
                return null;
            }
        }

        public string OldBreadcrumb
        {
            get
            {
                return null;
            }
        }

        public bool HasOldBreadcrumb
        {
            get
            {
                return false;
            }
        }
    }
}