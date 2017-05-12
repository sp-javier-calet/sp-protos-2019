﻿#if(UNITY_ANDROID || (UNITY_IOS && !NO_GPGS))
#define GOOGLE_SUPPORTED
#endif

using System;
using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.ServerEvents;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Social
{
    public class GoogleInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseEmpty;
            public bool LoginLink = true;
            public bool LoginWithUi = true;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            #if GOOGLE_SUPPORTED
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
                Container.Bind<GooglePlayLink>().ToMethod<GooglePlayLink>(CreateLoginLink);
                Container.Bind<ILink>().ToLookup<GooglePlayLink>();
                Container.Bind<IDisposable>().ToLookup<GooglePlayLink>();
            }
            #else
            Container.Rebind<IGoogle>().ToSingle<EmptyGoogle>();
            #endif

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGoogle>(CreateAdminPanel);
            #endif
        }

        #if GOOGLE_SUPPORTED

        UnityGoogle CreateUnityGoogle()
        {
            return new UnityGoogle();
        }

        void SetupUnityGoogle(UnityGoogle google)
        {
            google.Scheduler = Container.Resolve<SocialPoint.Utils.IUpdateScheduler>();
        }

        #endif

        #if ADMIN_PANEL

        AdminPanelGoogle CreateAdminPanel()
        {
            return new AdminPanelGoogle(
                Container.Resolve<IGoogle>());
        }

        #endif

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
