using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.Alert;
using UnityEngine;

namespace SocialPoint.Crash
{
    #if !UNITY_EDITOR
    using BasePlatformCrashReporter = DeviceCrashReporter;
    

#else
    using BasePlatformCrashReporter = BaseCrashReporter;
    #endif

    public class SocialPointCrashReporter : BasePlatformCrashReporter
    {
        public SocialPointCrashReporter(MonoBehaviour behaviour, IHttpClient client, IDeviceInfo deviceInfo, IAlertView alertView, BreadcrumbManager breadcrumbs = null)
            : base(behaviour, client, deviceInfo, alertView, breadcrumbs)
        {
        }
    }
}
