using UnityEngine;
using System;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.AdminPanel;
using SocialPoint.Notifications;

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
        Container.Rebind<INotificationServices>().ToSingle<AndroidNotificationServices>();
#elif UNITY_IOS
        Container.Rebind<INotificationServices>().ToSingle<IosNotificationServices>();
#else
        Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#endif

        Container.Rebind<NotificationManager>().ToMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IDisposable>().ToMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNotifications>(CreateAdminPanel);
    }

    AdminPanelNotifications CreateAdminPanel()
    {
        return new AdminPanelNotifications(
            Container.Resolve<INotificationServices>());
    }

    NotificationManager CreateNotificationManager()
    {
        return new NotificationManager(
            Container.Resolve<INotificationServices>(),
            Container.Resolve<IAppEvents>()
        );
    }

    public void Initialize()
    {
        Container.Resolve<SocialPoint.Notifications.NotificationManager>();
        var services = Container.Resolve<INotificationServices>();
        if(Settings.AutoRegisterForRemote)
        {
            services.RequestPermissions();
        }
    }

}
