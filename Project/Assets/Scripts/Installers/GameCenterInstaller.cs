using UnityEngine;
using System;
using SocialPoint.Dependency;
using SocialPoint.Social;
using SocialPoint.Login;
using SocialPoint.AdminPanel;

public class GameCenterInstaller : MonoInstaller
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
            Container.Rebind<IGameCenter>().ToSingleMethod<EmptyGameCenter>(CreateEmpty);
        }
        else
        {
            Container.Rebind<IGameCenter>().ToSingleMethod<UnityGameCenter>(CreateUnity);
        }
        if(Settings.LoginLink)
        {
            Container.Bind<ILink>().ToSingleMethod<GameCenterLink>(CreateLoginLink);
        }
        #else
        Container.Rebind<IGameCenter>().ToSingleMethod<EmptyGameCenter>(CreateEmptyGameCenter);
        #endif
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelGameCenter>(CreateAdminPanel);
    }

    EmptyGameCenter CreateEmpty()
    {
        return new EmptyGameCenter(null);
    }

    UnityGameCenter CreateUnity()
    {
        return new UnityGameCenter(
            Container.gameObject.transform);
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

    EmptyGameCenter CreateEmptyGameCenter()
    {
        return new EmptyGameCenter("Test User");
    }

}
