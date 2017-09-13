using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;

public class NotEnoughtResourcesControllerRed : UIPopupViewController 
{
    public void OnPopUntilShop()
    {
        var uiViewsStackController = Services.Instance.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }

        uiViewsStackController.PopUntil(typeof(ShopController));
    }
}