using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using SocialPoint.Network;

public class ServerSyncInstaller : Installer
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
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.BindInstance("command_queue_ignore_responses", Settings.IgnoreResponses);
        Container.BindInstance("command_queue_send_interval", Settings.SendInterval);
        Container.BindInstance("command_queue_outofsync_interval", Settings.MaxOutOfSyncInterval);
        Container.BindInstance("command_queue_timeout", Settings.Timeout);
        Container.BindInstance("command_queue_backoff_multiplier", Settings.BackoffMultiplier);
        Container.BindInstance("command_queue_ping_enabled", Settings.PingEnabled);

        Container.Rebind<ICommandQueue>().ToSingleMethod<CommandQueue>(CreateCommandQueue);
        Container.Bind<IDisposable>().ToLookup<ICommandQueue>();

        Container.Rebind<ServerSyncBridge>().ToSingleMethod<ServerSyncBridge>(CreateBridge);
        Container.Bind<IEventsBridge>().ToLookup<ServerSyncBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<ServerSyncBridge>();

        Container.Rebind<CommandReceiver>().ToSingle<CommandReceiver>();
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelCommandReceiver>(CreateAdminPanelCommandReceiver);
    }

    CommandQueue CreateCommandQueue()
    {
        return new CommandQueue(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>());
    }

    ServerSyncBridge CreateBridge()
    {
        return new ServerSyncBridge(
            Container.Resolve<ICommandQueue>());
    }

    AdminPanelCommandReceiver CreateAdminPanelCommandReceiver()
    {
        return new AdminPanelCommandReceiver(
            Container.Resolve<CommandReceiver>());
    }
}