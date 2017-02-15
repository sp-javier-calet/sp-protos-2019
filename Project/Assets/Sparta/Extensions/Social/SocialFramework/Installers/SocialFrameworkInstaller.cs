using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Locale;
using SocialPoint.Connection;

namespace SocialPoint.Social
{
    public class SocialFrameworkInstaller : ServiceInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind<SocialManager>().ToMethod<SocialManager>(CreateSocialManager);

            Container.Bind<PlayersManager>().ToMethod<PlayersManager>(CreatePlayersManager, SetupPlayersManager);

            Container.Bind<ChatManager>().ToMethod<ChatManager>(CreateChatManager, SetupChatManager);
            Container.Bind<IDisposable>().ToLookup<ChatManager>();

            Container.Bind<AlliancesManager>().ToMethod<AlliancesManager>(CreateAlliancesManager, SetupAlliancesManager);
            Container.Bind<IDisposable>().ToLookup<AlliancesManager>();

            Container.Bind<IRankManager>().ToMethod<IRankManager>(CreateRankManager);
            Container.Bind<IAccessTypeManager>().ToMethod<IAccessTypeManager>(CreateAccessTypeManager);

            Container.Bind<PlayersDataFactory>().ToMethod<PlayersDataFactory>(CreatePlayersDataFactory);

            Container.Bind<AllianceDataFactory>().ToMethod<AllianceDataFactory>(CreateAlliancesDataFactory, SetupAlliancesDataFactory);

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelSocialFramework>(CreateAdminPanelSocialFramework);

            Container.Listen<IChatRoom>().WhenResolved(SetupChatRoom);
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

        AdminPanelSocialFramework CreateAdminPanelSocialFramework()
        {
            return new AdminPanelSocialFramework(
                Container.Resolve<ConnectionManager>(),
                Container.Resolve<ChatManager>(),
                Container.Resolve<AlliancesManager>(),
                Container.Resolve<PlayersManager>(),
                Container.Resolve<SocialManager>());
        }

        void SetupChatRoom(IChatRoom room)
        {
            room.ChatManager = Container.Resolve<ChatManager>();
            room.Localization = Container.Resolve<Localization>();
        }
    }
}
