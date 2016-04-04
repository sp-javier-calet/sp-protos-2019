
using System;
using SocialPoint.Dependency;
using SocialPoint.Social;
using SocialPoint.Login;
using SocialPoint.AdminPanel;

public class GoogleInstaller : MonoInstaller
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
            Container.Rebind<IGoogle>().ToSingleGameObject<UnityGoogle>();
        }
        if(Settings.LoginLink)
        {
            Container.Bind<ILink>().ToSingleMethod<GooglePlayLink>(CreateLoginLink);
        }
        #else
        Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
        #endif
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGoogle>();
    }

    GooglePlayLink CreateLoginLink()
    {
        var google = Container.Resolve<IGoogle>();
        return new GooglePlayLink(google, !Settings.LoginWithUi);
    }
}
