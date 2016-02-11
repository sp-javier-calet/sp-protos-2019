using Zenject;
using UnityEngine;
using System;
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

        Container.Rebind<NotificationManager>().ToSingle<NotificationManager>();
        Container.Bind<IDisposable>().ToSingle<NotificationManager>();
        Container.Resolve<NotificationManager>();

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
    }

    public void Initialize()
    {
        var services = Container.Resolve<INotificationServices>();
        if(Settings.AutoRegisterForRemote)
        {
            services.RegisterForRemote();
        }
    }
    
}