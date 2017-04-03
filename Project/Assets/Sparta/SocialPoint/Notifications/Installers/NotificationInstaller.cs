using System;
using SocialPoint.Dependency;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Notifications
{
    public class NotificationInstaller : SubInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            public bool AutoRegisterForRemote = true;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);

#if UNITY_EDITOR
            Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#elif UNITY_ANDROID
        Container.Rebind<INotificationServices>().ToMethod<AndroidNotificationServices>(CreateAndroidNotificationServices);
#elif UNITY_IOS
        Container.Rebind<INotificationServices>().ToMethod<IosNotificationServices>(CreateIosNotificationServices);
#else
        Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#endif
            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNotifications>(CreateAdminPanel);
            #endif
        }

        #if !UNITY_EDITOR
    


#if UNITY_IOS
    IosNotificationServices CreateIosNotificationServices()
    {
        return new IosNotificationServices(Container.Resolve<SocialPoint.Utils.ICoroutineRunner>());
    }



#elif UNITY_ANDROID
    AndroidNotificationServices CreateAndroidNotificationServices()
    {
        return new AndroidNotificationServices(Container.Resolve<SocialPoint.Utils.ICoroutineRunner>());
    }
#endif
    
    #endif

        #if ADMIN_PANEL
        AdminPanelNotifications CreateAdminPanel()
        {
            return new AdminPanelNotifications(
                Container.Resolve<INotificationServices>());
        }
        #endif

        public void Initialize()
        {
            var services = Container.Resolve<INotificationServices>();
            if(Settings.AutoRegisterForRemote)
            {
                services.RequestPermissions();
            }
        }
    }
}
