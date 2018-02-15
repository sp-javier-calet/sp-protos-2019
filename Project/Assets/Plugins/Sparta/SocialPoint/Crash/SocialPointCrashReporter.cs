using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.Alert;
using SocialPoint.Utils;

namespace SocialPoint.Crash
{
    #if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
    using BasePlatformCrashReporter = DeviceCrashReporter;

#else
    using BasePlatformCrashReporter = BaseCrashReporter;
    #endif

    public sealed class SocialPointCrashReporter : BasePlatformCrashReporter
    {
        public SocialPointCrashReporter(IUpdateScheduler updateScheduler, IHttpClient client, IDeviceInfo deviceInfo, IBreadcrumbManager breadcrumbs = null, IAlertView alertView = null)
            : base(updateScheduler, client, deviceInfo, breadcrumbs, alertView)
        {
        }
    }
}
