using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;

public class NotEnoughtResourcesController : UIPopupViewController 
{
    public void OnCloseClick()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.Pop();
    }
}

