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
        public string AndroidLargeIcon = AndroidNotificationSettings.DefaultLargeIcon;
        public string AndroidSmallIcon = AndroidNotificationSettings.DefaultSmallIcon;
        public Color AndroidIconBackgroundColor = AndroidNotificationSettings.DefaultIconBackgroundColor;
    };
    
    public SettingsData Settings;

    public override void InstallBindings()
    {

#if UNITY_ANDROID 
        Container.Rebind<AndroidNotificationSettings>().ToSingleInstance(new AndroidNotificationSettings{
            LargeIcon = Settings.AndroidLargeIcon,
            SmallIcon = Settings.AndroidSmallIcon,
            IconBackgroundColor = Settings.AndroidIconBackgroundColor
        });
        Container.Rebind<INotificationServices>().ToSingle<AndroidNotificationServices>();
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


}
