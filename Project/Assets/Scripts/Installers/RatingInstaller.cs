using System;
using Zenject;
using SocialPoint.Rating;
using SocialPoint.Alert;
using SocialPoint.AdminPanel;

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
        Container.Rebind<IAppRater>().ToSingle<AppRater>();
        Container.Bind<IDisposable>().ToSingle<AppRater>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelAppRater>();
    }

    public void Initialize()
    {
        Container.Resolve<IAppRater>();
    }
}

