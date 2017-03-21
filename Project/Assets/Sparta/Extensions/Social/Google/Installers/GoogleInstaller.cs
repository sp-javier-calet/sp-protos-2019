using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.ServerEvents;

namespace SocialPoint.Social
{
    public class GoogleInstaller : ServiceInstaller, IInitializable
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
                Container.Bind<IInitializable>().ToInstance(this);

                Container.Rebind<UnityGoogle>().ToMethod<UnityGoogle>(CreateUnityGoogle, SetupUnityGoogle);
                Container.Rebind<IGoogle>().ToLookup<UnityGoogle>();
                Container.Bind<IDisposable>().ToLookup<UnityGoogle>();
            }
            if(Settings.LoginLink)
            {
                Container.Bind<ILink>().ToMethod<GooglePlayLink>(CreateLoginLink);
                Container.Bind<IDisposable>().ToLookup<GooglePlayLink>();
            }
            #else
            Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
            #endif
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGoogle>(CreateAdminPanel);
        }

        UnityGoogle CreateUnityGoogle()
        {
            return new UnityGoogle();
        }

        void SetupUnityGoogle(UnityGoogle google)
        {
            google.Scheduler = Container.Resolve<IUpdateScheduler>();
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

        public void Initialize()
        { 
            var google = Container.Resolve<IGoogle>();
            if(google != null)
            {
                google.TrackEvent = Container.Resolve<IEventTracker>().TrackSystemEvent;            
            }
        }
    }
}
