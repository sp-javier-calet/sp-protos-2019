
using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Social;
using SocialPoint.Login;
using SocialPoint.AdminPanel;

public class GoogleInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool UseEmpty = false;
        public bool LoginLink = true;
        public bool LoginWithUi = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        #if UNITY_ANDROID
        if(Settings.UseEmpty)
        {
            Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
        }
        else
        {
            Container.Rebind<IGoogle>().ToSingle<UnityGoogle>();
            Container.Rebind<MonoBehaviour>().ToSingle<UnityGoogle>();
        }
        if(Settings.LoginLink)
        {
            Container.Bind<ILink>().ToMethod<GooglePlayLink>(CreateLoginLink);
        }
        #else
        Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
        #endif
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGoogle>(CreateAdminPanel);
    }

    AdminPanelGoogle CreateAdminPanel()
    {
        return new AdminPanelGoogle(
            Container.Resolve<IGoogle>());
    }

    GooglePlayLink CreateLoginLink()
    {
        var google = Container.Resolve<IGoogle>();
        return new GooglePlayLink(google, !Settings.LoginWithUi);
    }
}
