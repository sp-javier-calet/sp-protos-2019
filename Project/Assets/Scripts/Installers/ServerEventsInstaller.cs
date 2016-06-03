using System;
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.Hardware;
using SocialPoint.ServerSync;
using SocialPoint.Login;
using SocialPoint.Crash;
using SocialPoint.AppEvents;
using SocialPoint.GameLoading;

public class ServerEventsInstaller : SubInstaller
{
	[Serializable]
    public class SettingsData
	{
        public int MaxOutOfSyncInterval = SocialPointEventTracker.DefaultMaxOutOfSyncInterval;
        public int SendInterval = SocialPointEventTracker.DefaultSendInterval;
        public float Timeout = SocialPointEventTracker.DefaultTimeout;
        public float BackoffMultiplier = SocialPointEventTracker.DefaultBackoffMultiplier;
	}

	public SettingsData Settings = new SettingsData();

	public override void InstallBindings()
	{
        Container.Rebind<SocialPointEventTracker>()
            .ToMethod<SocialPointEventTracker>(CreateEventTracker, SetupEventTracker);
        Container.Rebind<IEventTracker>().ToLookup<SocialPointEventTracker>();
        Container.Bind<IDisposable>().ToLookup<IEventTracker>();

        Container.Rebind<ServerEventsBridge>().ToMethod<ServerEventsBridge>(CreateBridge);
        Container.Bind<IEventsBridge>().ToLookup<ServerEventsBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<ServerEventsBridge>();
	}

    ServerEventsBridge CreateBridge()
    {
        return new ServerEventsBridge(
            Container.Resolve<IEventTracker>());
    }

    SocialPointEventTracker CreateEventTracker()
    {
        return new SocialPointEventTracker(
            Container.Resolve<IFixedUpdateScheduler>());
    }

    void SetupEventTracker(SocialPointEventTracker tracker)
    {
        tracker.Timeout = Settings.Timeout;
        tracker.MaxOutOfSyncInterval = Settings.MaxOutOfSyncInterval;
        tracker.SendInterval = Settings.SendInterval;
        tracker.BackoffMultiplier = Settings.BackoffMultiplier;
        tracker.HttpClient = Container.Resolve<IHttpClient>();
        tracker.DeviceInfo = Container.Resolve<IDeviceInfo>();
        tracker.CommandQueue = Container.Resolve<ICommandQueue>();
        tracker.BreadcrumbManager = Container.Resolve<BreadcrumbManager>();
        tracker.AppEvents = Container.Resolve<IAppEvents>();
        tracker.RequestSetup = Container.Resolve<ILogin>().SetupHttpRequest;
        Container.Resolve<IGameErrorHandler>().Setup(tracker);
    }
}
