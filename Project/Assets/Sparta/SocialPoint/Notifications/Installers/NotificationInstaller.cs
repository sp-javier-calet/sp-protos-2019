using UnityEngine;
using System;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.AdminPanel;
using SocialPoint.Notifications;
using SocialPoint.Utils;

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

        Container.Rebind<NotificationManager>().ToMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IDisposable>().ToMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNotifications>(CreateAdminPanel);
    }

    #if !UNITY_EDITOR
    

#if UNITY_IOS
    IosNotificationServices CreateIosNotificationServices()
    {
        return new IosNotificationServices(Container.Resolve<ICoroutineRunner>());
    }

#elif UNITY_ANDROID
    AndroidNotificationServices CreateAndroidNotificationServices()
    {
        return new AndroidNotificationServices(Container.Resolve<ICoroutineRunner>());
    }
#endif
    
    #endif

    AdminPanelNotifications CreateAdminPanel()
    {
        return new AdminPanelNotifications(
            Container.Resolve<INotificationServices>());
    }

    NotificationManager CreateNotificationManager()
    {
        return new NotificationManager(
            Container.Resolve<INotificationServices>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<ICommandQueue>()
        );
    }

    public void Initialize()
    {
        Container.Resolve<NotificationManager>();
        var services = Container.Resolve<INotificationServices>();
        if(Settings.AutoRegisterForRemote)
        {
            services.RequestPermissions();
        }
    }

}
