using SocialPoint.Dependency;
using SocialPoint.ServerEvents;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.Attributes;
using SocialPoint.Login;
using SocialPoint.Crash;
using SocialPoint.AppEvents;
using SocialPoint.Base;
using SocialPoint.Utils;
using SocialPoint.GameLoading;
using System;

class EventTracker : SocialPointEventTracker
{   
    public EventTracker(ICoroutineRunner runner):base(runner)
    {
        Timeout = ServiceLocator.Instance.TryResolve<float>("event_tracker_timeout", Timeout);
        MaxOutOfSyncInterval = ServiceLocator.Instance.TryResolve<int>("event_tracker_outofsync_interval");
        SendInterval = ServiceLocator.Instance.TryResolve<int>("event_tracker_send_interval");
        HttpClient = ServiceLocator.Instance.Resolve<IHttpClient>();
        DeviceInfo = ServiceLocator.Instance.Resolve<IDeviceInfo>();
        CommandQueue = ServiceLocator.Instance.Resolve<ICommandQueue>();
        BreadcrumbManager = ServiceLocator.Instance.Resolve<BreadcrumbManager>();
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        RequestSetup = ServiceLocator.Instance.Resolve<ILogin>().SetupHttpRequest;
        ServiceLocator.Instance.Resolve<IGameErrorHandler>().Setup(this);
    }

}
