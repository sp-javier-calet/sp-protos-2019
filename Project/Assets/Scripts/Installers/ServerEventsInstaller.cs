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

public class ServerEventsInstaller : Installer
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
        Container.BindInstance("event_tracker_timeout", Settings.Timeout);
        Container.BindInstance("event_tracker_outofsync_interval", Settings.MaxOutOfSyncInterval);
        Container.BindInstance("event_tracker_send_interval", Settings.SendInterval);
        Container.BindInstance("event_tracker_backoff_multiplier", Settings.BackoffMultiplier);
        Container.Rebind<SocialPointEventTracker>().ToSingleMethod<SocialPointEventTracker>(CreateEventTracker);
        Container.Rebind<IEventTracker>().ToLookup<SocialPointEventTracker>();
        Container.Bind<IDisposable>().ToLookup<IEventTracker>();

        Container.Rebind<ServerEventsBridge>().ToSingleMethod<ServerEventsBridge>(CreateBridge);
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
        var tracker = new SocialPointEventTracker(
            Container.Resolve<ICoroutineRunner>());

        tracker.Timeout = Container.Resolve<float>("event_tracker_timeout", tracker.Timeout);
        tracker.MaxOutOfSyncInterval = Container.Resolve<int>("event_tracker_outofsync_interval");
        tracker.SendInterval = Container.Resolve<int>("event_tracker_send_interval");
        tracker.HttpClient = Container.Resolve<IHttpClient>();
        tracker.DeviceInfo = Container.Resolve<IDeviceInfo>();
        tracker.CommandQueue = Container.Resolve<ICommandQueue>();
        tracker.BreadcrumbManager = Container.Resolve<BreadcrumbManager>();
        tracker.AppEvents = Container.Resolve<IAppEvents>();
        tracker.RequestSetup = Container.Resolve<ILogin>().SetupHttpRequest;
        Container.Resolve<IGameErrorHandler>().Setup(tracker);

        return tracker;
    }
}
