using UnityEngine;
using Zenject;
using System;
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
            Container.Rebind<IGameCenter>().ToSingle<EmptyGameCenter>();
        }
        else
        {
            Container.Rebind<IGameCenter>().ToSingle<UnityGameCenter>();
        }
        if(Settings.LoginLink)
        {
        Container.Bind<ILink>().ToSingleMethod<GameCenterLink>(CreateLoginLink);
        }
        #else
        Container.Rebind<IGameCenter>().ToSingle<EmptyGameCenter>();
        #endif
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGameCenter>();
    }

    GameCenterLink CreateLoginLink(InjectContext ctx)
    {
        var gc = ctx.Container.Resolve<IGameCenter>();
        return new GameCenterLink(gc);
    }
}
