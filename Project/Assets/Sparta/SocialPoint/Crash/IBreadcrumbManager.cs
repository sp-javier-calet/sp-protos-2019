using System;

namespace SocialPoint.Crash
{
    public interface IBreadcrumbManager
    {
        void Log(string info);

        void DumpToFile();

        void RemoveData();

        string CurrentBreadcrumb { get; }

        string OldBreadcrumb { get; }

        bool HasOldBreadcrumb { get; }

        Exception LogException { get; set; }
    }
}