using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.IO;

namespace SocialPoint.Crash
{
    public interface ICrashReporter
    {
        void ForceCrash();

        bool ErrorLogActive{ get; set; }

        bool ExceptionLogActive{ get; set; }

        bool IsEnabled { get; }

        void Enable();

        void Disable();

        void ClearUniqueExceptions();
    }
}