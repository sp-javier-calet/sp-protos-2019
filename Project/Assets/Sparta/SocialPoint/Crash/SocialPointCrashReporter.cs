using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.Alert;
using SocialPoint.Utils;

namespace SocialPoint.Crash
{
    #if !UNITY_EDITOR
    using BasePlatformCrashReporter = DeviceCrashReporter;
    #else
    using BasePlatformCrashReporter = BaseCrashReporter;
    #endif

    public class SocialPointCrashReporter : BasePlatformCrashReporter
    {
        public SocialPointCrashReporter(ICoroutineRunner runner, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbs = null, IAlertView alertView = null)
            : base(runner, client, deviceInfo, breadcrumbs, alertView)
        {
        }
    }
}
