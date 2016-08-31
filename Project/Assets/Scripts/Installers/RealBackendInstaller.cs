using System;
using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;

public class RealBackendInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public ServerSyncInstaller.SettingsData Sync = new ServerSyncInstaller.SettingsData();
        public LoginInstaller.SettingsData Login = new LoginInstaller.SettingsData();
        public ServerEventsInstaller.SettingsData Events = new ServerEventsInstaller.SettingsData();
        public NotificationInstaller.SettingsData Notifications = new NotificationInstaller.SettingsData();
        public CrashInstaller.SettingsData Crashes = new CrashInstaller.SettingsData();
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        {
            var installer = new ServerSyncInstaller();
            installer.Settings = Settings.Sync;
            Container.Install(installer);
        }
        {
            var installer = new LoginInstaller();
            installer.Settings = Settings.Login;
            Container.Install(installer);
        }
        {
            var installer = new ServerEventsInstaller();
            installer.Settings = Settings.Events;
            Container.Install(installer);
        }
        {
            UnityEngine.Debug.Log("*** TEST RealBackendInstaller");
            var installer = new NotificationInstaller();
            installer.Settings = Settings.Notifications;
            Container.Install(installer);
        }
        {
            var installer = new CrashInstaller();
            installer.Settings = Settings.Crashes;
            Container.Install(installer);
        }
        {
            var installer = new QualityStatsInstaller();
            Container.Install(installer);
        }
        {
            var installer = new MessageCenterInstaller();
            Container.Install(installer);
        }
        {
            UnityEngine.Debug.Log("*** TEST RealBackendInstaller CrossPromotionInstaller");
            var installer = new CrossPromotionInstaller();
            Container.Install(installer);
        }
    }

}