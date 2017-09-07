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
    }
}