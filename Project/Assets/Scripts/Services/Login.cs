using Zenject;
using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.AppEvents;
using System.Collections.Generic;

public class Login : SocialPointLogin
{
    [Inject]
    IDeviceInfo injectDeviceInfo
    {
        set
        {
            DeviceInfo = value;
        }
    }

    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackEvent = value.TrackSystemEvent;
        }
    }

    [InjectOptional]
    List<ILink> injectLinks
    {
        set
        {
            foreach(var link in value)
            {
                AddLink(link);
            }
        }
    }
    
    [Inject("persistent")]
    IAttrStorage injectStorage
    {
        set
        {
            Storage = value;
        }
    }
    
    [InjectOptional("login_timeout")]
    float injectTimeout
    {
        set
        {
            Timeout = value;
        }
    }
        
    [InjectOptional("login_activity_timeout")]
    float injectActivityTimeout
    {
        set
        {
            ActivityTimeout = value;
        }
    }
    
    [InjectOptional("login_autoupdate_friends")]
    bool injectAutoUpdateFriends
    {
        set
        {
            AutoUpdateFriends = value;
        }
    }
    
    [InjectOptional("login_autoupdate_friends_photo_size")]
    uint injectAutoUpdateFriendsPhotosSize
    {
        set
        {
            AutoUpdateFriendsPhotosSize = value;
        }
    }
    
    [InjectOptional("login_user_mappings_block")]
    uint injectUserMappingsBlock
    {
        set
        {
            UserMappingsBlock = value;
        }
    }
    
    [InjectOptional("language")]
    string injectLanguage
    {
        set
        {
            Language = value;
        }
    }
    
    public Login(IHttpClient client, LoginConfig config) : base(client, config)
    {
    }
}