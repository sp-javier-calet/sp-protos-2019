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
        if(Container.HasBinding<NotificationManager>())
        {
            return;
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        Container.Bind<INotificationServices>().ToSingleMethod<AndroidNotificationServices>(CreateAndroidNotificationServices);
#elif UNITY_IOS && !UNITY_EDITOR
        Container.Bind<INotificationServices>().ToSingle<IosNotificationServices>();
#else
        Container.Bind<INotificationServices>().ToSingle<EmptyNotificationServices>();
#endif

        Container.Bind<NotificationManager>().ToSingle<NotificationManager>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
        Container.Resolve<NotificationManager>();

        if(Settings.AutoRegisterForRemote)
        {
            var services = Container.Resolve<INotificationServices>();
            services.RegisterForRemote();
        }
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
