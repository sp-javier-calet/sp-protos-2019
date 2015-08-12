using SocialPoint.Hardware;
using SocialPoint.Network;
using SocialPoint.IO;
using UnityEngine;

namespace SocialPoint.Crash
{
    #if !UNITY_EDITOR
    using BasePlatformCrashReporter = DeviceCrashReporter;
    #else
    using BasePlatformCrashReporter = CrashReporterBase;
    #endif

    public class SocialPointCrashReporter : BasePlatformCrashReporter
    {
        public SocialPointCrashReporter(MonoBehaviour behaviour, IHttpClient client, IDeviceInfo deviceInfo, BreadcrumbManager breadcrumbManager = null)
            : base(behaviour, client, deviceInfo, breadcrumbManager)
        {
        }
    }
}
