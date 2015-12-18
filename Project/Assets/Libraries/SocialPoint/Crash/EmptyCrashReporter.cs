using System;

namespace SocialPoint.Crash
{
    public class EmptyCrashReporter : ICrashReporter
    {
        public void ForceCrash()
        {
            // Null object exception
            string a = null;
            a.Clone();
        }

        public bool ErrorLogActive{ get; set; }

        public bool ExceptionLogActive{ get; set; }

        public bool WasEnabled { get; private set; }

        public void Enable()
        {
            WasEnabled = true;
        }

        public void Disable()
        {
            WasEnabled = false;
        }

        public void Dispose()
        {
            Disable();
        }

        public void ClearUniqueExceptions()
        {
        }

        public void SendCrashesBeforeLogin(Action callback)
        {
            if(callback != null)
            {
                callback();
            }
        }
    }
}
