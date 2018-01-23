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
            public LinkMode LoginLinkMode = LinkMode.Auto;
            public bool LoadAchievements;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            #if UNITY_IOS && !UNITY_EDITOR
            if(Settings.UseEmpty)
            {
                Container.Rebind<IGameCenter>().ToMethod<EmptyGameCenter>(CreateEmpty);
            }
            else
            {
                Container.Rebind<IosGameCenter>().ToMethod<IosGameCenter>(CreateIos);
                Container.Rebind<IGameCenter>().ToLookup<IosGameCenter>();
            }

            if(Settings.LoginLink)
            {
                Container.Bind<GameCenterLink>().ToMethod<GameCenterLink>(CreateLoginLink);
                Container.Bind<ILink>().ToLookup<GameCenterLink>();
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
            return new EmptyGameCenter();
        }

        #if UNITY_IOS
        IosGameCenter CreateIos()
        {
            return new IosGameCenter(true, Settings.LoadAchievements);
        }
        #endif

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
            return new GameCenterLink(gc, Settings.LoginLinkMode);
        }
    }
}
