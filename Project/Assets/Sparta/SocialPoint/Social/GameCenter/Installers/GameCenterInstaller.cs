using System;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Utils;
using UnityEngine;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Social
{
    public class GameCenterInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
            public bool LoginLink = true;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            #if UNITY_IOS
        if(Settings.UseEmpty)
        {
            Container.Rebind<IGameCenter>().ToMethod<EmptyGameCenter>(CreateEmpty);
        }
        else
        {
            Container.Rebind<IGameCenter>().ToMethod<UnityGameCenter>(CreateUnity);
        }
        if(Settings.LoginLink)
        {
            Container.Bind<ILink>().ToMethod<GameCenterLink>(CreateLoginLink);
        }
            #else
            Container.Rebind<IGameCenter>().ToMethod<EmptyGameCenter>(CreateEmpty);
            #endif

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGameCenter>(CreateAdminPanel);
            #endif
        }

        EmptyGameCenter CreateEmpty()
        {
            return new EmptyGameCenter("test");
        }

        UnityGameCenter CreateUnity()
        {
            return new UnityGameCenter(
                Container.Resolve<NativeCallsHandler>());
        }

        #if ADMIN_PANEL
        AdminPanelGameCenter CreateAdminPanel()
        {
            return new AdminPanelGameCenter(
                Container.Resolve<IGameCenter>());
        }
        #endif

        GameCenterLink CreateLoginLink()
        {
            var gc = Container.Resolve<IGameCenter>();
            return new GameCenterLink(gc);
        }
    }
}
