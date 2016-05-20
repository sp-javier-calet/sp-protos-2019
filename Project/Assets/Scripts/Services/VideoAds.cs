using SocialPoint.VideoAds;
using System;
using Zenject;

public class VideoAds : SocialPointVideoAdsManager
{
    [InjectOptional("videoads_appid")]
    string userLevelUntilPrompt
    {
        set
        {
            AppId = value;
        }
    }

    [InjectOptional("videoads_userid")]
    string maxPromptsPerDay
    {
        set
        {
            UserId = value;
        }
    }
}


