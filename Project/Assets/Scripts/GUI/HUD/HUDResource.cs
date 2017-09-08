using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Alert;

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
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }
            
        uiViewsStackController.Push(typeof(NotEnoughtResourcesController));
        uiViewsStackController.Push(typeof(ShopController));
    }
}
