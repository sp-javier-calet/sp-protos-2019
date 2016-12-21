using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Utils;

namespace SocialPoint.Social
{
    public class FacebookInstaller : ServiceInstaller
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
                Container.Rebind<IFacebook>().ToMethod<UnityFacebook>(CreateUnityFacebook);
            }
            if(Settings.LoginLink)
            {
                Container.Bind<ILink>().ToMethod<FacebookLink>(CreateLoginLink);
            }
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelFacebook>(CreateAdminPanel);
        }

        AdminPanelFacebook CreateAdminPanel()
        {
            return new AdminPanelFacebook(
                Container.Resolve<IFacebook>());
        }

        UnityFacebook CreateUnityFacebook()
        {
            return new UnityFacebook(
                Container.Resolve<ICoroutineRunner>());
        }

        FacebookLink CreateLoginLink()
        {
            var fb = Container.Resolve<IFacebook>();
            return new FacebookLink(fb, Settings.LoginWithUi);
        }
    }
}