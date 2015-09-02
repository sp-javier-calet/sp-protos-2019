using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.IO;

namespace SocialPoint.Crash
{
    public interface ICrashReporter
    {
        void ForceCrash();

        bool ErrorLogActive{ set; }

        bool ExceptionLogActive{ set; }

        bool IsEnabled { get; }

        void Enable();

        void Disable();

        void ClearUniqueExceptions();
    }
}