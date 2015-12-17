using System;
using Zenject;
using SocialPoint.Attributes;
using SocialPoint.ServerEvents;
using SocialPoint.Login;
using SocialPoint.ServerSync;
using SocialPoint.Crash;
using System.Text;

public class RealBackendInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public ServerSyncInstaller.SettingsData Sync = new ServerSyncInstaller.SettingsData();
        public LoginInstaller.SettingsData Login = new LoginInstaller.SettingsData();
        public ServerEventsInstaller.SettingsData Events = new ServerEventsInstaller.SettingsData();
        public NotificationInstaller.SettingsData Notifications = new NotificationInstaller.SettingsData();
        public CrashInstaller.SettingsData Crashes = new CrashInstaller.SettingsData();
    };
    
    public SettingsData Settings = new SettingsData();
    
    public override void InstallBindings()
    {
        var baseUrl = Settings.Login.Environment.GetUrl();
        if(string.IsNullOrEmpty(baseUrl))
        {
            var installer = gameObject.AddComponent<EmptyBackendInstaller>();
            Container.Install(installer);
            return;
        }
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
            var installer = new QualityStatsInstaller();
            Container.Install(installer);
        }
        {
            var installer = new NotificationInstaller();
            installer.Settings = Settings.Notifications;
            Container.Install(installer);
        }
        {
            var installer = new CrashInstaller();
            installer.Settings = Settings.Crashes;
            Container.Install(installer);
        }
    }



}