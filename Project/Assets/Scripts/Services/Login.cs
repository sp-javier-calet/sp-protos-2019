using SocialPoint.Dependency;
using SocialPoint.Login;
using SocialPoint.Hardware;
using SocialPoint.Attributes;
using SocialPoint.Network;
using SocialPoint.ServerEvents;
using SocialPoint.AppEvents;
using System.Collections.Generic;

public class Login : SocialPointLogin
{
    public Login(IHttpClient client, LoginConfig config) : base(client, config)
    {
        DeviceInfo = ServiceLocator.Instance.Resolve<IDeviceInfo>();
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        TrackEvent = ServiceLocator.Instance.Resolve<IEventTracker>().TrackSystemEvent;
        Storage = ServiceLocator.Instance.Resolve<IAttrStorage>("persistent");
        Timeout = ServiceLocator.Instance.Resolve<float>("login_timeout", Timeout);
        ActivityTimeout = ServiceLocator.Instance.Resolve<float>("login_activity_timeout", ActivityTimeout);
        AutoUpdateFriends = ServiceLocator.Instance.Resolve<bool>("login_autoupdate_friends", AutoUpdateFriends);
        AutoUpdateFriendsPhotosSize = ServiceLocator.Instance.Resolve<uint>("login_autoupdate_friends_photo_size", AutoUpdateFriendsPhotosSize);
        UserMappingsBlock = ServiceLocator.Instance.Resolve<uint>("login_user_mappings_block", UserMappingsBlock);
        Language = ServiceLocator.Instance.Resolve<string>("language", Language);

        var links = ServiceLocator.Instance.ResolveArray<ILink>();
        for(var i = 0; i < links.Length; i++)
        {
            AddLink(links[i]);
        }
    }
}