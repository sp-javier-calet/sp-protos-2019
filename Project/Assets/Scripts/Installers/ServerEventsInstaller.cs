using System;
using SocialPoint.Dependency;
using SocialPoint.ServerEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;

public class ServerEventsInstaller : Installer
{
	[Serializable]
    public class SettingsData
	{
        public int MaxOutOfSyncInterval = EventTracker.DefaultMaxOutOfSyncInterval;
        public int SendInterval = EventTracker.DefaultSendInterval;
        public float Timeout = EventTracker.DefaultTimeout;
        public float BackoffMultiplier = EventTracker.DefaultBackoffMultiplier;
	}

	public SettingsData Settings = new SettingsData();

	public override void InstallBindings()
	{
        Container.BindInstance("event_tracker_timeout", Settings.Timeout);
        Container.BindInstance("event_tracker_outofsync_interval", Settings.MaxOutOfSyncInterval);
        Container.BindInstance("event_tracker_send_interval", Settings.SendInterval);
        Container.BindInstance("event_tracker_backoff_multiplier", Settings.BackoffMultiplier);
        Container.Rebind<EventTracker>().ToSingleMethod<EventTracker>(CreateEventTracker);
        Container.Rebind<IEventTracker>().ToLookup<EventTracker>();
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

    EventTracker CreateEventTracker()
    {
        return new EventTracker(
            Container.Resolve<ICoroutineRunner>());
    }
}
