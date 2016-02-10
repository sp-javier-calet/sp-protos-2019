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
    };
    
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
    }
}
