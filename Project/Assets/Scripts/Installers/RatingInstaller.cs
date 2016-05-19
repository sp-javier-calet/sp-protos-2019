using System;
using SocialPoint.Dependency;
using SocialPoint.Rating;
using SocialPoint.Alert;
using SocialPoint.AdminPanel;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;

public class RatingInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public int UsesUntilPrompt = AppRater.DefaultUsesUntilPrompt;
        public int EventsUntilPrompt = AppRater.DefaultEventsUntilPrompt;
        public long DaysUntilPrompt = AppRater.DefaultDaysUntilPrompt;
        public long DaysBeforeReminding = AppRater.DefaultDaysBeforeReminding;
        public int UserLevelUntilPrompt = AppRater.DefaultUserLevelUntilPrompt;
        public int MaxPromptsPerDay = AppRater.DefaultMaxPromptsPerDay;
    }

    public SettingsData Settings = new SettingsData();


    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
        Container.Rebind<AppRater>().ToMethod<AppRater>(CreateAppRater, SetupAppRater);
        Container.Rebind<IAppRater>().ToLookup<AppRater>();
        Container.Bind<IDisposable>().ToLookup<AppRater>();
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAppRater>(CreateAdminPanel);
    }

    AdminPanelAppRater CreateAdminPanel()
    {
        return new AdminPanelAppRater(
            Container.Resolve<IAppRater>());
    }

    AppRater CreateAppRater()
    {
        return new AppRater(
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAttrStorage>("volatile"),
            Container.Resolve<IAppEvents>());
    }

    void SetupAppRater(AppRater rater)
    {
        rater.GUI = new DefaultAppRaterGUI(Container.Resolve<IAlertView>());
        rater.UsesUntilPrompt = Settings.UsesUntilPrompt;
        rater.EventsUntilPrompt = Settings.EventsUntilPrompt;
        rater.DaysUntilPrompt = Settings.DaysUntilPrompt;
        rater.DaysBeforeReminding = Settings.DaysBeforeReminding;
        rater.UserLevelUntilPrompt = Settings.UserLevelUntilPrompt;
        rater.MaxPromptsPerDay = Settings.MaxPromptsPerDay;
        rater.Init();
    }

    public void Initialize()
    {
        Container.Resolve<IAppRater>();
    }
}

