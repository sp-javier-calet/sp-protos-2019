using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.ScriptEvents;


public class GUISceneInstaller : MonoInstaller, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab = null;
        public bool InitialScreenAnimation = false;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);
    }

    public void Initialize()
    {
        if(Settings.InitialScreenPrefab == null)
        {
            return;
        }
        var screens = Container.Resolve<ScreensController>();
        if(screens == null)
        {
            throw new InvalidOperationException("Could not find screens controller for initial screen");
        }
        var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
        var ctrl = go.GetComponent<UIViewController>();
        if(ctrl == null)
        {
            throw new InvalidOperationException("Initial Screen Prefab does not contain a UIViewController");
        }
        else
        {
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
}