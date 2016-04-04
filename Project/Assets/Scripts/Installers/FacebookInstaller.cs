using UnityEngine;
using System;
using SocialPoint.Dependency;
using SocialPoint.Social;
using SocialPoint.Login;
using SocialPoint.AdminPanel;

public class FacebookInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
        public bool LoginLink = true;
        public bool LoginWithUi = false;
    }
    
    public SettingsData Settings = new SettingsData();

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
        if(Settings.LoginLink)
        {
            Container.Bind<ILink>().ToSingleMethod<FacebookLink>(CreateLoginLink);
        }
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelFacebook>();
    }

    FacebookLink CreateLoginLink()
    {
        var fb = Container.Resolve<IFacebook>();
        return new FacebookLink(fb, Settings.LoginWithUi);
    }
}
