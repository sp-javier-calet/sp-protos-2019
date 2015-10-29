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
        if(Settings.UseEmpty)
        {
            Container.Rebind<IGameCenter>().ToSingle<EmptyGameCenter>();
        }
        else
        {
            Container.Rebind<IGameCenter>().ToSingleMethod<UnityGameCenter>(CreateUnityGameCenter);
        }
    }

    UnityGameCenter CreateUnityGameCenter(InjectContext ctx)
    {
        return new UnityGameCenter(Settings.EnableVerification);
    }
}
