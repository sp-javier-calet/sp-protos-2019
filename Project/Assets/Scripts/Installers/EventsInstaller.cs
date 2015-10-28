using Zenject;
using System;
using SocialPoint.Events;

public class EventsInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
        public int MaxOutOfSyncInterval = EventTracker.DefaultMaxOutOfSyncInterval;
        public int SendInterval = EventTracker.DefaultSendInterval;
        public float Timeout = EventTracker.DefaultTimeout;
        public float BackoffMultiplier = EventTracker.DefaultBackoffMultiplier;
	};
	
	public SettingsData Settings;

	public override void InstallBindings()
	{
        Container.BindInstance("event_tracker_timeout", Settings.Timeout);
        Container.BindInstance("event_tracker_outofsync_interval", Settings.MaxOutOfSyncInterval);
        Container.BindInstance("event_tracker_send_interval", Settings.SendInterval);
        Container.BindInstance("event_tracker_backoff_multiplier", Settings.BackoffMultiplier);
        Container.Rebind<IEventTracker>().ToSingle<EventTracker>();
        Container.Rebind<IDisposable>().ToSingle<EventTracker>();

        Container.Resolve<IEventTracker>().Start();
	}
}