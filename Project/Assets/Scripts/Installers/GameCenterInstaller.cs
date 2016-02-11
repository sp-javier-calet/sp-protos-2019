using UnityEngine;
using Zenject;
using System;
using SocialPoint.Social;
using SocialPoint.Login;

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
    }

    GameCenterLink CreateLoginLink(InjectContext ctx)
    {
        var gc = ctx.Container.Resolve<IGameCenter>();
        return new GameCenterLink(gc);
    }
}
