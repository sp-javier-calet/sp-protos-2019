using UnityEngine;
using Zenject;
using System;
using SocialPoint.Social;

public class GameCenterInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
        public bool EnableVerification = true;
    };
    
    public SettingsData Settings;
    
    
    public override void InstallBindings()
    {
        if(Container.HasBinding<IGameCenter>())
        {
            return;
        }
        if(Settings.UseEmpty)
        {
            Container.Bind<IGameCenter>().ToSingle<EmptyGameCenter>();
        }
        else
        {
            Container.Bind<IGameCenter>().ToSingleMethod<UnityGameCenter>(CreateUnityGameCenter);
        }
    }

    UnityGameCenter CreateUnityGameCenter(InjectContext ctx)
    {
        return new UnityGameCenter(Settings.EnableVerification);
    }
}
