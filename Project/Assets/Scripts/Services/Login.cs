using Zenject;
using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.Events;

class Login : SocialPointLogin
{
    [Inject]
    public IDeviceInfo InjectDeviceInfo
    {
        set
        {
            DeviceInfo = value;
        }
    }

    [Inject]
    public IEventTracker InjectEventTracker
    {
        set
        {
            TrackEvent = value.TrackEvent;
        }
    }
    
    [InjectOptional("persistent")]
    public IAttrStorage InjectStorage
    {
        set
        {
            Storage = value;
        }
    }
    
    [InjectOptional("login_timeout")]
    public float InjectTimeout
    {
        set
        {
            Timeout = value;
        }
    }
        
    [InjectOptional("login_activity_timeout")]
    public float InjectActivityTimeout
    {
        set
        {
            ActivityTimeout = value;
        }
    }
    
    [InjectOptional("login_autoupdate_friends")]
    public bool InjectAutoUpdateFriends
    {
        set
        {
            AutoUpdateFriends = value;
        }
    }
    
    [InjectOptional("login_autoupdate_friends_photo_size")]
    public uint InjectAutoUpdateFriendsPhotosSize
    {
        set
        {
            AutoUpdateFriendsPhotosSize = value;
        }
    }
    
    [InjectOptional("login_max_retries")]
    public uint InjectMaxLoginRetries
    {
        set
        {
            MaxLoginRetries = value;
        }
    }
    
    [InjectOptional("login_user_mappings_block")]
    public uint InjectUserMappingsBlock
    {
        set
        {
            UserMappingsBlock = value;
        }
    }
    
    [InjectOptional("language")]
    public string InjectLanguage
    {
        set
        {
            Language = value;
        }
    }
    
    public Login(IHttpClient client, [Inject("base_url")] string baseUrl=null) : base(client, baseUrl)
    {
    }
}