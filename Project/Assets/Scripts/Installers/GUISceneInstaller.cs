using System;
using UnityEngine;
using SocialPoint.Dependency;

public class GUISceneInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public GameObject InitialScreenPrefab;
        public bool InitialScreenAnimation;
    }

    public SettingsData Settings = new SettingsData();

    public GUISceneInstaller() : base(ModuleType.Game)
    {
    }

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

        var uiViewsStackController = Container.Resolve<UIViewsStackController>();
        if(uiViewsStackController == null)
        {
            throw new InvalidOperationException("Could not find UI Controller");
        }
            
        if(Settings.InitialScreenPrefab != null)
        {
            var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
            if(go != null)
            {
                if(Settings.InitialScreenAnimation)
                {
                    uiViewsStackController.Push(go);
                }
                else
                {
                    uiViewsStackController.PushImmediate(go);
                }
            }
        }
    }
}