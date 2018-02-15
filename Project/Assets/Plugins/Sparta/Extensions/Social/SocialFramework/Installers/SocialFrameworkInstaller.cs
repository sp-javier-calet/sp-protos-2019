using System;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Locale;
using SocialPoint.Connection;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Social
{
    public class SocialFrameworkInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public bool EnableAlliances;
            public bool EnableChat;
            public bool EnableMessageSystem;
            public bool EnableDonations;

            public SettingsData()
            {
                EnableAlliances = true;
                EnableChat = true;
                EnableMessageSystem = true;
                EnableDonations = true;
            }
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<SocialManager>().ToMethod<SocialManager>(CreateSocialManager);

            Container.Bind<PlayersManager>().ToMethod<PlayersManager>(CreatePlayersManager, SetupPlayersManager);
            Container.Bind<PlayersDataFactory>().ToMethod<PlayersDataFactory>(CreatePlayersDataFactory);

            if(Settings.EnableChat)
            {
                Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager, SetupChatManager);
                Container.Bind<IDisposable>().ToLookup<ChatManager>();
                Container.Listen<IChatRoom>().Then(SetupChatRoom);
            }
            if(Settings.EnableAlliances)
            {
                Container.Bind<AlliancesManager>().ToMethod<AlliancesManager>(CreateAlliancesManager, SetupAlliancesManager);
                Container.Bind<IDisposable>().ToLookup<AlliancesManager>();

                Container.Bind<IRankManager>().ToMethod<IRankManager>(CreateRankManager);
                Container.Bind<IAccessTypeManager>().ToMethod<IAccessTypeManager>(CreateAccessTypeManager);

                Container.Bind<AllianceDataFactory>().ToMethod<AllianceDataFactory>(CreateAlliancesDataFactory, SetupAlliancesDataFactory);
            }
            if(Settings.EnableMessageSystem)
            {
                Container.Bind<MessagingSystemManager>().ToMethod<MessagingSystemManager>(CreateMessagingSystemManager, SetupMessagingSystemManager);
                Container.Bind<IDisposable>().ToLookup<MessagingSystemManager>();
            }
            if(Settings.EnableDonations)
            {
                Container.Bind<DonationsManager>().ToMethod<DonationsManager>(CreateDonationsManager, SetupDonationsManager);
                Container.Bind<IDisposable>().ToLookup<DonationsManager>();
            }
            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanelSocialFramework, SetupAdminPanelSocialFramework);
            #endif

            Container.Bind<IInitializable>().ToInstance(this);
        }

        public void Initialize()
        {
            Container.Resolve<PlayersManager>();
            if(Settings.EnableChat)
            {
                Container.Resolve<ChatManager>();
            }
            if(Settings.EnableAlliances)
            {
                Container.Resolve<AlliancesManager>();
            }
            if(Settings.EnableMessageSystem)
            {
                Container.Resolve<MessagingSystemManager>();
            }
            if(Settings.EnableDonations)
            {
                Container.Resolve<DonationsManager>();
            }
        }

        SocialManager CreateSocialManager()
        {
            return new SocialManager();
        }

        PlayersManager CreatePlayersManager()
        {
            return new PlayersManager(Container.Resolve<ConnectionManager>(), Container.Resolve<SocialManager>());
        }

        void SetupPlayersManager(PlayersManager manager)
        {
            manager.Factory = Container.Resolve<PlayersDataFactory>();
        }

        ChatManager CreateChatManager()
        {
            return new ChatManager(
                Container.Resolve<ConnectionManager>(), Container.Resolve<SocialManager>());
        }

        void SetupChatManager(ChatManager manager)
        {
            manager.Register(Container.ResolveList<IChatRoom>());
        }

        AlliancesManager CreateAlliancesManager()
        {
            return new AlliancesManager(
                Container.Resolve<ConnectionManager>(), Container.Resolve<SocialManager>(), Container.Resolve<ChatManager>());
        }

        void SetupAlliancesManager(AlliancesManager manager)
        {
            manager.Factory = Container.Resolve<AllianceDataFactory>();
            manager.LoginData = Container.Resolve<ILoginData>();
            manager.Ranks = Container.Resolve<IRankManager>();
            manager.AccessTypes = Container.Resolve<IAccessTypeManager>();
        }

        PlayersDataFactory CreatePlayersDataFactory()
        {
            return new PlayersDataFactory();
        }

        AllianceDataFactory CreateAlliancesDataFactory()
        {
            return new AllianceDataFactory();
        }

        void SetupAlliancesDataFactory(AllianceDataFactory factory)
        {
            factory.Ranks = Container.Resolve<IRankManager>();
        }

        IRankManager CreateRankManager()
        {
            return new DefaultRankManager();
        }

        IAccessTypeManager CreateAccessTypeManager()
        {
            return new DefaultAccessTypeManager();
        }

        MessagingSystemManager CreateMessagingSystemManager()
        {
            return new MessagingSystemManager(Container.Resolve<ConnectionManager>());
        }

        void SetupMessagingSystemManager(MessagingSystemManager manager)
        {
            manager.SocialManager = Container.Resolve<SocialManager>();
            if(Settings.EnableAlliances)
            {
                manager.AlliancesManager = Container.Resolve<AlliancesManager>();
            }
        }

        DonationsManager CreateDonationsManager()
        {
            return new DonationsManager();
        }

        void SetupDonationsManager(DonationsManager manager)
        {
            manager.Setup(Container.Resolve<ConnectionManager>());
        }

        #if ADMIN_PANEL
        AdminPanelSocialFramework CreateAdminPanelSocialFramework()
        {
            return new AdminPanelSocialFramework(
                Container.Resolve<ConnectionManager>(),
                Container.Resolve<SocialManager>(),
                Container.Resolve<PlayersManager>()
            );
        }

        void SetupAdminPanelSocialFramework(AdminPanelSocialFramework adminPanel)
        {
            if(Settings.EnableChat)
            {
                adminPanel.ChatManager = Container.Resolve<ChatManager>();
            }
            if(Settings.EnableAlliances)
            {
                adminPanel.AlliancesManager = Container.Resolve<AlliancesManager>();
            }
            if(Settings.EnableMessageSystem)
            {
                adminPanel.MessagesManager = Container.Resolve<MessagingSystemManager>();
            }
            if(Settings.EnableDonations)
            {
                adminPanel.DonationsManager = Container.Resolve<DonationsManager>();
            }
        }
        #endif

        void SetupChatRoom(IChatRoom room)
        {
            room.ChatManager = Container.Resolve<ChatManager>();
            room.Localization = Container.Resolve<Localization>();
        }
    }
}
