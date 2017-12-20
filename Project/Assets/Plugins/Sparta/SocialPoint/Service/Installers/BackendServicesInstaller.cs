using System;
using SocialPoint.Crash;
using SocialPoint.CrossPromotion;
using SocialPoint.Dependency;
using SocialPoint.Notifications;
using SocialPoint.Login;
using SocialPoint.QualityStats;
using SocialPoint.ServerEvents;
using SocialPoint.ServerMessaging;
using SocialPoint.ServerSync;

namespace SocialPoint.Service
{
    public class BackendServicesInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public ServerSyncInstaller.SettingsData Sync = new ServerSyncInstaller.SettingsData();
            public LoginInstaller.SettingsData Login = new LoginInstaller.SettingsData();
            public ServerEventsInstaller.SettingsData Events = new ServerEventsInstaller.SettingsData();
            public NotificationInstaller.SettingsData Notifications = new NotificationInstaller.SettingsData();
            public CrashInstaller.SettingsData Crashes = new CrashInstaller.SettingsData();
            public QualityStatsInstaller.SettingsData QualityStats = new QualityStatsInstaller.SettingsData();
            public MessageCenterInstaller.SettingsData MessageCenter = new MessageCenterInstaller.SettingsData();
            public CrossPromotionInstaller.SettingsData CrossPromotion = new CrossPromotionInstaller.SettingsData();
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            if(!Container.HasInstalled<ServerSyncInstaller>())
            {
                var installer = new ServerSyncInstaller();
                installer.Settings = Settings.Sync;
                Container.Install(installer);
            }

            if(!Container.HasInstalled<LoginInstaller>())
            {
                var installer = new LoginInstaller();
                installer.Settings = Settings.Login;
                Container.Install(installer);
            }

            if(!Container.HasInstalled<ServerEventsInstaller>())
            {
                var installer = new ServerEventsInstaller();
                installer.Settings = Settings.Events;
                Container.Install(installer);
            }

            if(!Container.HasInstalled<NotificationInstaller>())
            {
                var installer = new NotificationInstaller();
                installer.Settings = Settings.Notifications;
                Container.Install(installer);
            }

            if(!Container.HasInstalled<CrashInstaller>())
            {
                var installer = new CrashInstaller();
                installer.Settings = Settings.Crashes;
                Container.Install(installer);
            }

            if(!Container.HasInstalled<QualityStatsInstaller>())
            {
                var installer = new QualityStatsInstaller();
                Container.Install(installer);
            }

            if(!Container.HasInstalled<MessageCenterInstaller>())
            {
                var installer = new MessageCenterInstaller();
                Container.Install(installer);
            }

            if(!Container.HasInstalled<CrossPromotionInstaller>())
            {
                var installer = new CrossPromotionInstaller();
                Container.Install(installer);
            }
        }
    }
}
