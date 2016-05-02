using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Rating;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;
using SocialPoint.Alert;

public class AppRater : SocialPoint.Rating.AppRater
{

    public AppRater(IDeviceInfo deviceInfo, IAttrStorage storage, IAppEvents appEvents) :
        base(deviceInfo, storage, appEvents)
    {

        GUI = new DefaultAppRaterGUI(ServiceLocator.Instance.Resolve<IAlertView>());
        UsesUntilPrompt = ServiceLocator.Instance.OptResolve<int>("apprater_uses_until_prompt", UsesUntilPrompt);
        EventsUntilPrompt = ServiceLocator.Instance.OptResolve<int>("apprater_events_until_prompt", EventsUntilPrompt);
        DaysUntilPrompt = ServiceLocator.Instance.OptResolve<long>("apprater_days_until_prompt", DaysUntilPrompt);
        DaysBeforeReminding = ServiceLocator.Instance.OptResolve<long>("apprater_days_before_reminding", DaysBeforeReminding);
        UserLevelUntilPrompt = ServiceLocator.Instance.OptResolve<int>("apprater_user_level_until_prompt", UserLevelUntilPrompt);
        MaxPromptsPerDay = ServiceLocator.Instance.OptResolve<int>("apprater_max_prompts_per_day", MaxPromptsPerDay);
        Init();
    }
}
