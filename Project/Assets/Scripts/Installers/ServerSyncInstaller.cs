using Zenject;
using System;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;

public class ServerSyncInstaller : MonoInstaller
{
	[Serializable]
	public class SettingsData
	{
        public bool IgnoreResponses = CommandQueue.DefaultIgnoreResponses;
        public int SendInterval = CommandQueue.DefaultSendInterval;
        public int MaxOutOfSyncInterval = CommandQueue.DefaultMaxOutOfSyncInterval;
        public float Timeout = CommandQueue.DefaultTimeout;
        public float BackoffMultiplier = CommandQueue.DefaultBackoffMultiplier;
        public bool PingEnabled = CommandQueue.DefaultPingEnabled;
	};
	
    public SettingsData Settings = new SettingsData();

	public override void InstallBindings()
	{
        Container.BindInstance("command_queue_ignore_responses", Settings.IgnoreResponses);
        Container.BindInstance("command_queue_send_interval", Settings.SendInterval);
        Container.BindInstance("command_queue_outofsync_interval", Settings.MaxOutOfSyncInterval);
        Container.BindInstance("command_queue_timeout", Settings.Timeout);
        Container.BindInstance("command_queue_backoff_multiplier", Settings.BackoffMultiplier);
        Container.BindInstance("command_queue_ping_enabled", Settings.PingEnabled);

        Container.Rebind<ICommandQueue>().ToSingle<CommandQueue>();
        Container.Bind<IDisposable>().ToLookup<ICommandQueue>();

        Container.Bind<IEventsBridge>().ToSingle<ServerSyncBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<ServerSyncBridge>();
	}

}