using UnityEngine;
using Zenject;
using System;
using SocialPoint.Social;
using SocialPoint.AdminPanel;

public class FacebookInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
    };
    
    public SettingsData Settings;


    public override void InstallBindings()
    {
        if(Settings.UseEmpty)
        {
            Container.Rebind<IFacebook>().ToSingle<EmptyFacebook>();
        }
        else
        {
            Container.Rebind<IFacebook>().ToSingle<UnityFacebook>();
        }
    }
}
