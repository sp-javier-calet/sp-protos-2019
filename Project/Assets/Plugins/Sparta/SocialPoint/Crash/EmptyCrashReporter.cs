using System;

namespace SocialPoint.Crash
{
    public sealed class EmptyCrashReporter : ICrashReporter
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

        public bool IsEnabled { get; private set; }

        public void Enable()
        {
            IsEnabled = true;
            WasEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
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

        public void ReportHandledException(Exception e)
        {
        }
    }
}
