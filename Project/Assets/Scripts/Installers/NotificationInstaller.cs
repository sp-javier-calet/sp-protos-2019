using Zenject;
using UnityEngine;
using System;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.AdminPanel;
using SocialPoint.Notifications;

public class NotificationInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool AutoRegisterForRemote = true;
        public string AndroidLargeIcon = AndroidNotificationServices.DefaultLargeIcon;
        public string AndroidSmallIcon = AndroidNotificationServices.DefaultSmallIcon;
        public Color AndroidIconBackgroundColor = AndroidNotificationServices.DefaultIconBackgroundColor;
    };
    
    public SettingsData Settings;

    public override void InstallBindings()
    {

#if UNITY_ANDROID 
        Container.Rebind<INotificationServices>().ToSingleMethod<AndroidNotificationServices>(CreateAndroidNotificationServices);
#elif UNITY_IOS
        Container.Rebind<INotificationServices>().ToSingle<IosNotificationServices>();
#else
        Container.Rebind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#endif

        Container.Rebind<NotificationManager>().ToSingle<NotificationManager>();
        Container.Bind<IDisposable>().ToSingle<NotificationManager>();
        Container.Resolve<NotificationManager>();

        if(Settings.AutoRegisterForRemote)
        {
            var services = Container.Resolve<INotificationServices>();
            services.RegisterForRemote();
        }

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
    }

    AndroidNotificationServices CreateAndroidNotificationServices(InjectContext ctx)
    {
        var services = new AndroidNotificationServices();
        services.LargeIcon = Settings.AndroidLargeIcon;
        services.SmallIcon = Settings.AndroidSmallIcon;
        services.IconBrackgroundColor = Settings.AndroidIconBackgroundColor;
        return services;
    }
}
