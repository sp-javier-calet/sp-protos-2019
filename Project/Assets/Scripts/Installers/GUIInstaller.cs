using System;
using Zenject;
using UnityEngine;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.ScriptEvents;



public class GUIInstaller : MonoInstaller, IDisposable
{
    const string UIViewControllerSuffix = "Controller";

    [Serializable]
    public class SettingsData
    {
        public float PopupFadeSpeed = PopupsController.DefaultFadeSpeed;
        public GameObject InitialScreenPrefab = null;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);
        UIViewController.AwakeEvent += OnViewControllerAwake;

        Container.BindInstance("popup_fade_speed", Settings.PopupFadeSpeed);

        var popups = GameObject.FindObjectOfType<PopupsController>();
        if(popups != null)
        {
            Container.Rebind<PopupsController>().ToSingleInstance(popups);
        }
        var screens = GameObject.FindObjectOfType<ScreensController>();
        if(screens != null)
        {
            Container.Rebind<ScreensController>().ToSingleInstance(screens);
            if(Settings.InitialScreenPrefab != null)
            {
                var go = Instantiate<GameObject>(Settings.InitialScreenPrefab);
                var ctrl = go.GetComponent<UIViewController>();
                if(ctrl == null)
                {
                    throw new InvalidOperationException("Initial Screen Prefab does not contain a UIViewController");
                }
                else
                {
                    screens.PushImmediate(ctrl);
                }
            }
        }

        var uiLayerController = GameObject.FindObjectOfType<UILayersController>();
        if(uiLayerController != null)
        {
            UIViewController.LayersController = uiLayerController;
        }
                
        Container.Bind<IEventsBridge>().ToSingle<GUIControlBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<GUIControlBridge>();
    }

    void OnViewControllerAwake(UIViewController ctrl)
    {
        if(ctrl.gameObject.transform.parent == null)
        {
            Container.Inject(ctrl);
        }
    }

    string GetControllerFactoryPrefabName(Type type)
    {
        var name = type.Name;
        if(name.EndsWith(UIViewControllerSuffix))
        {
            name = name.Substring(0, name.Length - UIViewControllerSuffix.Length);
        }
        return string.Format("GUI_{0}", name);
    }

    public void Dispose()
    {
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);
        UIViewController.AwakeEvent -= OnViewControllerAwake;
    }


}