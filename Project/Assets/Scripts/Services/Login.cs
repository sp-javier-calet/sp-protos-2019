﻿using SocialPoint.Dependency;
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
        Timeout = ServiceLocator.Instance.OptResolve<float>("login_timeout", Timeout);
        ActivityTimeout = ServiceLocator.Instance.OptResolve<float>("login_activity_timeout", ActivityTimeout);
        AutoUpdateFriends = ServiceLocator.Instance.OptResolve<bool>("login_autoupdate_friends", AutoUpdateFriends);
        AutoUpdateFriendsPhotosSize = ServiceLocator.Instance.OptResolve<uint>("login_autoupdate_friends_photo_size", AutoUpdateFriendsPhotosSize);
        UserMappingsBlock = ServiceLocator.Instance.OptResolve<uint>("login_user_mappings_block", UserMappingsBlock);
        Language = ServiceLocator.Instance.OptResolve<string>("language", Language);

        var links = ServiceLocator.Instance.Resolve<List<ILink>>();
        foreach(var link in links)
        {
            AddLink(link);
        }
    }
}