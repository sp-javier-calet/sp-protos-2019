using Zenject;
using UnityEngine;
using SocialPoint.Rating;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;
using SocialPoint.Alert;

public class AppRater : SocialPoint.Rating.AppRater
{
    
    [Inject]
    IAlertView alertView
    {
        set
        {
            GUI = new DefaultAppRaterGUI(value);
        }
    }

    [InjectOptional("apprater_uses_until_prompt")]
    int usesUntilPrompt
    {
        set
        {
            UsesUntilPrompt = value;
        }
    }

    [InjectOptional("apprater_events_until_prompt")]
    int eventsUntilPrompt
    {
        set
        {
            EventsUntilPrompt = value;
        }
    }

    [InjectOptional("apprater_days_until_prompt")]
    long daysUntilPrompt
    {
        set
        {
            DaysUntilPrompt = value;
        }
    }

    [InjectOptional("apprater_days_before_reminding")]
    long daysBeforeReminding
    {
        set
        {
            DaysBeforeReminding = value;
        }
    }

    [InjectOptional("apprater_user_level_until_prompt")]
    int userLevelUntilPrompt
    {
        set
        {
            UserLevelUntilPrompt = value;
        }
    }

    [InjectOptional("apprater_max_prompts_per_day")]
    int maxPromptsPerDay
    {
        set
        {
            MaxPromptsPerDay = value;
        }
    }   

    public AppRater([Inject] IDeviceInfo deviceInfo, [Inject("volatile")] IAttrStorage storage, [Inject] IAppEvents appEvents):
    base(deviceInfo, storage, appEvents)
    {
    }

    [PostInject]
    void PostInject()
    {
        Init();
    }
}
