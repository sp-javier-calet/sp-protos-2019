using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using UnityEngine;

public class GUIMainHUDInstaller : Installer, IInitializable
{
    const string kGUIMainHUDPrefab = "GUI_HUD";

    [Serializable]
    public class SettingsData
    {
        public bool InitialScreenAnimation;
    }

    public SettingsData Settings = new SettingsData();

    public GUIMainHUDInstaller() : base(ModuleType.Game)
    {
    }

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
    }

    public void Initialize()
    {
        var screens = Container.Resolve<ScreensController>();
        if(screens == null)
        {
            throw new InvalidOperationException("Could not find screens controller for GUI main HUD");
        }

        var hud = Resources.Load<GameObject>(kGUIMainHUDPrefab);
        if(hud == null)
        {
            throw new InvalidOperationException("Could not load GUI main HUD prefab");
        }
            
        var go = Instantiate<GameObject>(hud);
        var ctrl = go.GetComponent<UIViewController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("HUI Main HUD Prefab does not contain a UIViewController");
        }

        if(Settings.InitialScreenAnimation)
        {
            screens.Push(ctrl);
        }
        else
        {
            screens.PushImmediate(ctrl);
        }
    }
}