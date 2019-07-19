//-----------------------------------------------------------------------
// HUDResource.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using UnityEngine;
using SocialPoint.Dependency;

public class HUDResource : MonoBehaviour
{
    public enum ResourceType
    {
        Gold,
        Gems
    }

    [SerializeField]
    ResourceType _resourceType;

    public void OnClick()
    {
        var uiHUDNotificationsController = Services.Instance.Resolve<HUDNotificationsController>();
        if(uiHUDNotificationsController == null)
        {
            throw new InvalidOperationException("Could not find UI HUD NotificationsController");
        }

        uiHUDNotificationsController.ShowNotification("Button for resource '" + _resourceType + "' pressed");
    }
}
