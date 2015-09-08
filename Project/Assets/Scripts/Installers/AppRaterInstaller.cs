using System;
using Zenject;
using SocialPoint.AppRater;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Hardware;

using SocialPoint.Alert;

public class AppRaterInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public int UsesUntilPrompt = 20;
        public int EventsUntilPrompt = -1;
        public int DaysUntilPrompt = 30;
        public int DaysBeforeReminding = 1;
        public int UserLevelUntilPrompt = 20;
        public int CurrentUserLevel = 0;
        public int MaxPromptsPerDay = -1;
    }

    public SettingsData Settings;

    [Inject]
    IDeviceInfo deviceInfo;

    [Inject("persistent")]
    IAttrStorage storage;

    [Inject]
    IAppEvents appEvents;

    [Inject]
    IAlertView alertView;

    public override void InstallBindings()
    {
        var appRater = new AppRater(deviceInfo, storage, appEvents);
        Container.Bind<AppRater>().ToInstance(appRater);
        var AppRaterGUI = new DefaultAppRaterGUI(alertView);
        AppRaterGUI.setAppRater(appRater);
        Container.Bind<IAppRaterGUI>().ToInstance(AppRaterGUI);
        appRater.AppRaterGUI = AppRaterGUI;

        appRater.UsesUntilPrompt = Settings.UsesUntilPrompt;
        appRater.EventsUntilPrompt = Settings.EventsUntilPrompt;
        appRater.DaysUntilPrompt = Settings.DaysUntilPrompt;
        appRater.DaysBeforeReminding = Settings.DaysBeforeReminding;
        appRater.UserLevelUntilPrompt = Settings.UserLevelUntilPrompt;
        appRater.CurrentUserLevel = Settings.CurrentUserLevel;
        appRater.MaxPromptsPerDay = Settings.MaxPromptsPerDay;

        appRater.Register();
        appRater.Init();
    }
}

