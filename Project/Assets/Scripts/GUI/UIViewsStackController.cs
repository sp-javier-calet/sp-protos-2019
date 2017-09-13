using SocialPoint.GUIControl;
using System;
using SocialPoint.Utils;
using System.Text;
using SocialPoint.AdminPanel;

public class UIViewsStackController : UINewStackController
{
    const string kUIViewControllerSuffix = "Controller";
    const string kUIViewControllerExamplePrefix = "GUI_Example";

    const string kAdminPanelPrefab = "GUI_AdminPanel";
    const string kMainHUDPrefab = "GUI_ExampleHUD";

    public string GetControllerFactoryPrefabName(Type type)
    {
        if(type == typeof(AdminPanelController))
        {
            return kAdminPanelPrefab;
        }
        else if(type == typeof(HUDController))
        {
            return kMainHUDPrefab;
        }
        else
        {
            var name = type.Name;
            name = name.Replace(kUIViewControllerSuffix, string.Empty);

            StringBuilder stringBuilder = StringUtils.StartBuilder();
            stringBuilder.Append(kUIViewControllerExamplePrefix);
            stringBuilder.Append(name);
            return StringUtils.FinishBuilder(stringBuilder);
        }
    }
}