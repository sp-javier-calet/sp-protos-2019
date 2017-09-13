using SocialPoint.GUIControl;
using System;
using SocialPoint.Utils;
using System.Text;
using SocialPoint.AdminPanel;

public class UIViewsStackController : UINewStackController
{
    const string kUIViewControllerSuffix = "Controller";
    const string UIViewControllerExamplePrefix = "GUI_Example";

    const string kAdminPanelPrefab = "GUI_AdminPanel";

    public string GetControllerFactoryPrefabName(Type type)
    {
        if(type == typeof(AdminPanelController))
        {
            return kAdminPanelPrefab;
        }
        else
        {
            var name = type.Name;
            name = name.Replace(kUIViewControllerSuffix, string.Empty);

            StringBuilder stringBuilder = StringUtils.StartBuilder();
            stringBuilder.Append(UIViewControllerExamplePrefix);
            stringBuilder.Append(name);
            return StringUtils.FinishBuilder(stringBuilder);
        }
    }
}