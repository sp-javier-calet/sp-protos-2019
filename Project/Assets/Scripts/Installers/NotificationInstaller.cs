using UnityEngine;
using System;
using SocialPoint.Dependency;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.AdminPanel;
using SocialPoint.Notifications;

public class NotificationInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public bool AutoRegisterForRemote = true;
    }
    
    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToSingleInstance(this);

#if UNITY_EDITOR
        Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#elif UNITY_ANDROID 
        Container.Rebind<INotificationServices>().ToSingle<AndroidNotificationServices>();
#elif UNITY_IOS
        Container.Rebind<INotificationServices>().ToSingle<IosNotificationServices>();
#else
        Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#endif

        Container.Rebind<NotificationManager>().ToSingleMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IDisposable>().ToSingleMethod<NotificationManager>(CreateNotificationManager);
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelNotifications>(CreateAdminPanel);
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
        Container.Resolve<NotificationManager>();
        var services = Container.Resolve<INotificationServices>();
        if(Settings.AutoRegisterForRemote)
        {
            services.RegisterForRemote();
        }
    }
    
}