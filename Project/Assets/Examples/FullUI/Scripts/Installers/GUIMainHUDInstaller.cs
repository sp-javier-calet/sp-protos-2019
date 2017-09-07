using System;
using UnityEngine;
using SocialPoint.Dependency;

public class GUIMainHUDInstaller : Installer, IInitializable
{
    public GUIMainHUDInstaller() : base(ModuleType.Game)
    {
    }

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
    }

    public void Initialize()
    {
        var uiViewsStackController = Container.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }
            
        uiViewsStackController.PushImmediate(typeof(HUDController));
    }
}