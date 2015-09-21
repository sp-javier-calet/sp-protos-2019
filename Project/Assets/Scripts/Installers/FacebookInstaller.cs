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
        if(Container.HasBinding<IFacebook>())
        {
            return;
        }
        if(Settings.UseEmpty)
        {
            Container.Bind<IFacebook>().ToSingle<EmptyFacebook>();
        }
        else
        {
            Container.Bind<IFacebook>().ToSingle<UnityFacebook>();
        }
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelFacebook>();
    }
}
