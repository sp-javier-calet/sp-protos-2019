using SocialPoint.GUIControl;
using System;
using SocialPoint.Utils;
using System.Text;

public class UIViewsStackController : UINewStackController
{
    const string kUIViewControllerSuffix = "Controller";
    const string kUIViewControllerExamplePrefix = "GUI_";

    public string GetControllerFactoryPrefabName(Type type)
    {
        var name = type.Name;
        name = name.Replace(kUIViewControllerSuffix, string.Empty);

        StringBuilder stringBuilder = StringUtils.StartBuilder();
        stringBuilder.Append(kUIViewControllerExamplePrefix);
        stringBuilder.Append(name);
        return StringUtils.FinishBuilder(stringBuilder);
    }
}