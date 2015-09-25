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
    };
    
    public SettingsData Settings;

    public override void InstallBindings()
    {
        if(Container.HasBinding<NotificationManager>())
        {
            return;
        }

        Container.Bind<NotificationManager>().ToSingle<NotificationManager>();
        Container.Bind<INotificationServices>().ToGetter<NotificationManager>((mng) => mng.Services);
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelNotifications>();
        Container.Resolve<NotificationManager>();

        if(Settings.AutoRegisterForRemote)
        {
            var services = Container.Resolve<INotificationServices>();
            services.RegisterForRemote();
        }
    }
}
