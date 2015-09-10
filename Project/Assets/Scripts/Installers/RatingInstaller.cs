using System;
using Zenject;
using SocialPoint.Rating;
using SocialPoint.Alert;

public class RatingInstaller : MonoInstaller
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

    public SettingsData Settings;


    public override void InstallBindings()
    {
        Container.BindInstance("apprater_uses_until_prompt", Settings.UsesUntilPrompt);
        Container.BindInstance("apprater_events_until_prompt", Settings.EventsUntilPrompt);
        Container.BindInstance("apprater_days_until_prompt", Settings.DaysUntilPrompt);
        Container.BindInstance("apprater_days_before_reminding", Settings.DaysBeforeReminding);
        Container.BindInstance("apprater_user_level_until_prompt", Settings.UserLevelUntilPrompt);
        Container.BindInstance("apprater_max_prompts_per_day", Settings.MaxPromptsPerDay);
        Container.Bind<IAppRater>().ToSingle<AppRater>();
        Container.Resolve<IAppRater>();
    }
}

