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

public class ServerSyncInstaller : SubInstaller
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
        Container.Rebind<ICommandQueue>().ToMethod<CommandQueue>(CreateCommandQueue);
        Container.Bind<IDisposable>().ToLookup<ICommandQueue>();

        Container.Rebind<ServerSyncBridge>().ToMethod<ServerSyncBridge>(CreateBridge);
        Container.Bind<IEventsBridge>().ToLookup<ServerSyncBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<ServerSyncBridge>();

        Container.Rebind<CommandReceiver>().ToSingle<CommandReceiver>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCommandReceiver>(CreateAdminPanelCommandReceiver);
    }

    CommandQueue CreateCommandQueue()
    {
        return new CommandQueue(
            Container.Resolve<ICoroutineRunner>(),
            Container.Resolve<IHttpClient>());
    }

    void SetupCommandQueue(CommandQueue queue)
    {
        queue.IgnoreResponses = Settings.IgnoreResponses;
        queue.SendInterval =  Settings.SendInterval;
        queue.MaxOutOfSyncInterval = Settings.MaxOutOfSyncInterval;
        queue.Timeout = Settings.Timeout;
        queue.BackoffMultiplier = Settings.BackoffMultiplier;
        queue.PingEnabled = Settings.PingEnabled;
        queue.AppEvents = Container.Resolve<IAppEvents>();
        queue.TrackEvent = Container.Resolve<IEventTracker>().TrackEvent;
        queue.RequestSetup = Container.Resolve<ILogin>().SetupHttpRequest;
        queue.CommandReceiver = Container.Resolve<CommandReceiver>();
        queue.AutoSync = Container.Resolve<IGameLoader>().OnAutoSync;
        Container.Resolve<IGameErrorHandler>().Setup(queue);
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