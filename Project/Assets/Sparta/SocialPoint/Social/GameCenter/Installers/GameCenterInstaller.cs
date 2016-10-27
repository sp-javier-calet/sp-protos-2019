using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Social;
using SocialPoint.Utils;
using UnityEngine;

public class GameCenterInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
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
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGameCenter>(CreateAdminPanel);
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


    AdminPanelGameCenter CreateAdminPanel()
    {
        return new AdminPanelGameCenter(
            Container.Resolve<IGameCenter>());
    }

    GameCenterLink CreateLoginLink()
    {
        var gc = Container.Resolve<IGameCenter>();
        return new GameCenterLink(gc);
    }
}
