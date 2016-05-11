using System;
using SocialPoint.Dependency;
using SocialPoint.Rating;
using SocialPoint.Alert;
using SocialPoint.AdminPanel;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;

public class RatingInstaller : MonoInstaller, IInitializable
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
        Container.Bind<IInitializable>().ToSingleInstance(this);
        Container.BindInstance("apprater_uses_until_prompt", Settings.UsesUntilPrompt);
        Container.BindInstance("apprater_events_until_prompt", Settings.EventsUntilPrompt);
        Container.BindInstance("apprater_days_until_prompt", Settings.DaysUntilPrompt);
        Container.BindInstance("apprater_days_before_reminding", Settings.DaysBeforeReminding);
        Container.BindInstance("apprater_user_level_until_prompt", Settings.UserLevelUntilPrompt);
        Container.BindInstance("apprater_max_prompts_per_day", Settings.MaxPromptsPerDay);
        Container.Rebind<AppRater>().ToSingleMethod<AppRater>(CreateAppRater);
        Container.Rebind<IAppRater>().ToLookup<AppRater>();
        Container.Bind<IDisposable>().ToLookup<AppRater>();
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelAppRater>(CreateAdminPanel);
    }

    AdminPanelAppRater CreateAdminPanel()
    {
        return new AdminPanelAppRater(
            Container.Resolve<IAppRater>());
    }

    AppRater CreateAppRater()
    {
        var rater = new AppRater(
            Container.Resolve<IDeviceInfo>(),
            Container.Resolve<IAttrStorage>("volatile"),
            Container.Resolve<IAppEvents>());

        rater.GUI = new DefaultAppRaterGUI(ServiceLocator.Instance.Resolve<IAlertView>());
        rater.UsesUntilPrompt = Container.Resolve<int>("apprater_uses_until_prompt", rater.UsesUntilPrompt);
        rater.EventsUntilPrompt = Container.Resolve<int>("apprater_events_until_prompt", rater.EventsUntilPrompt);
        rater.DaysUntilPrompt = Container.Resolve<long>("apprater_days_until_prompt", rater.DaysUntilPrompt);
        rater.DaysBeforeReminding = Container.Resolve<long>("apprater_days_before_reminding", rater.DaysBeforeReminding);
        rater.UserLevelUntilPrompt = Container.Resolve<int>("apprater_user_level_until_prompt", rater.UserLevelUntilPrompt);
        rater.MaxPromptsPerDay = Container.Resolve<int>("apprater_max_prompts_per_day", rater.MaxPromptsPerDay);
        rater.Init();

        return rater;
    }

    public void Initialize()
    {
        Container.Resolve<IAppRater>();
    }
}

