using System;

namespace SocialPoint.Crash
{
    public interface ICrashReporter : IDisposable
    {
        void ForceCrash();

        bool ErrorLogActive{ get; set; }

        bool ExceptionLogActive{ get; set; }

        bool WasEnabled { get; }

        void Enable();

        void Disable();

        void ClearUniqueExceptions();

        void SendCrashesBeforeLogin(Action callback);
    }
}
