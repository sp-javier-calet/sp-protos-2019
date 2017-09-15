using System;
using SocialPoint.Dependency;
using SocialPoint.ServerSync;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using SocialPoint.Network;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.GameLoading;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.ServerSync
{
    public class ServerSyncInstaller : SubInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
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
            if(!Settings.UseEmpty)
            {
                Container.Rebind<ICommandQueue>().ToMethod<CommandQueue>(CreateCommandQueue, SetupCommandQueue);

                Container.Rebind<ServerSyncBridge>().ToMethod<ServerSyncBridge>(CreateBridge);
                Container.Bind<IEventsBridge>().ToLookup<ServerSyncBridge>();
                Container.Bind<IScriptEventsBridge>().ToLookup<ServerSyncBridge>();
            }
            else
            {
                Container.Bind<ICommandQueue>().ToSingle<EmptyCommandQueue>();
            }

            Container.Bind<IDisposable>().ToLookup<ICommandQueue>();

            Container.Rebind<CommandReceiver>().ToSingle<CommandReceiver>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCommandReceiver>(CreateAdminPanelCommandReceiver);
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelCommandQueue>(CreateAdminPanelCommandQueue);
            #endif
        }

        CommandQueue CreateCommandQueue()
        {
            return new CommandQueue(
                Container.Resolve<IUpdateScheduler>(),
                Container.Resolve<IHttpClient>());
        }

        void SetupCommandQueue(CommandQueue queue)
        {
            queue.IgnoreResponses = Settings.IgnoreResponses;
            queue.SendInterval = Settings.SendInterval;
            queue.MaxOutOfSyncInterval = Settings.MaxOutOfSyncInterval;
            queue.Timeout = Settings.Timeout;
            queue.BackoffMultiplier = Settings.BackoffMultiplier;
            queue.PingEnabled = Settings.PingEnabled;
            queue.AppEvents = Container.Resolve<IAppEvents>();
            queue.TrackSystemEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;
            queue.LoginData = Container.Resolve<ILoginData>();
            queue.CommandReceiver = Container.Resolve<CommandReceiver>();
            Container.Resolve<IGameErrorHandler>().Setup(queue);
        }

        ServerSyncBridge CreateBridge()
        {
            return new ServerSyncBridge(
                Container.Resolve<ICommandQueue>());
        }

        #if ADMIN_PANEL
        AdminPanelCommandReceiver CreateAdminPanelCommandReceiver()
        {
            return new AdminPanelCommandReceiver(
                Container.Resolve<CommandReceiver>());
        }

        AdminPanelCommandQueue CreateAdminPanelCommandQueue()
        {
            return new AdminPanelCommandQueue(Container.Resolve<ICommandQueue>());
        }
        #endif
    }
}
