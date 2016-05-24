using SocialPoint.VideoAds;
using System;
using Zenject;

public class VideoAds : SocialPointVideoAdsManager
{
    [InjectOptional("videoads_appid")]
    string VideoAdsAppId
    {
        set
        {
            AppId = value;
        }
    }

    [InjectOptional("videoads_userid")]
    string VideoAdsUserId
    {
        set
        {
            UserId = value;
        }
    }

    [InjectOptional("videoads_securitytoken")]
    string VideoAdsSecurityToken
    {
        set
        {
            SecurityToken = value;
        }
    }

}


