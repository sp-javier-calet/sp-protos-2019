using Zenject;
using System;
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
        public bool LoginWithUi = false;
    };

    public SettingsData Settings = new SettingsData();


    public override void InstallBindings()
    {
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
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGoogle>();
    }

    GooglePlayLink CreateLoginLink(InjectContext ctx)
    {
        var google = ctx.Container.Resolve<IGoogle>();
        return new GooglePlayLink(google, Settings.LoginWithUi);
    }
}
