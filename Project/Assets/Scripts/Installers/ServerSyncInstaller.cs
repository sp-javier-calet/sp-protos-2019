using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.GameLoading;

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
        var queue = new CommandQueue(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>());

        queue.IgnoreResponses = Container.Resolve<bool>("command_queue_ignore_responses", queue.IgnoreResponses);
        queue.SendInterval =  Container.Resolve<int>("command_queue_send_interval", queue.SendInterval);
        queue.MaxOutOfSyncInterval = Container.Resolve<int>("command_queue_outofsync_interval", queue.MaxOutOfSyncInterval);
        queue.Timeout = Container.Resolve<float>("command_queue_timeout", queue.Timeout);
        queue.BackoffMultiplier = Container.Resolve<float>("command_queue_backoff_multiplier", queue.BackoffMultiplier);
        queue.PingEnabled = Container.Resolve<bool>("command_queue_ping_enabled", queue.PingEnabled);
        queue.AppEvents = Container.Resolve<IAppEvents>();
        queue.TrackEvent = Container.Resolve<IEventTracker>().TrackEvent;
        queue.RequestSetup = Container.Resolve<ILogin>().SetupHttpRequest;
        queue.CommandReceiver = Container.Resolve<CommandReceiver>();
        queue.AutoSync = Container.Resolve<IGameLoader>().OnAutoSync;
        Container.Resolve<IGameErrorHandler>().Setup(queue);

        return queue;
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