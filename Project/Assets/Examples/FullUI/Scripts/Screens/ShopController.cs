using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;

public class ShopController : UIScreenViewController 
{
    public void OnNotEnoughtResourcesClick()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.Push(typeof(NotEnoughtResourcesController));
        uiViewsStackController.Push(typeof(NotEnoughtResourcesController));
        uiViewsStackController.Push(typeof(NotEnoughtResourcesController));
    }

    public void OnNotEnoughtResourcesImmediateClick()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.PushImmediate(typeof(NotEnoughtResourcesController));
    }

    public void OnNotEnoughtResourcesReplaceClick()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.Replace(typeof(NotEnoughtResourcesControllerRed));
    }

    public void OnNotEnoughtResourcesReplaceImmediateClick()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.ReplaceImmediate(typeof(NotEnoughtResourcesControllerRed));
    }
}