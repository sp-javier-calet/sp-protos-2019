using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using UnityEngine;

public class GUIExampleInstaller : Installer, IDisposable
{
    const string UIViewControllerSuffix = "Controller";
    const string GUIRootPrefab = "GUI_ExampleRoot";

    [Serializable]
    public class SettingsData
    {
        public float PopupAnimationTime = UIPopupViewController.DefaultAnimationTime;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;
    UIViewsStackController _uiViewsStackController;

    public override void InstallBindings()
    {
        Container.Add<IDisposable, GUIExampleInstaller>(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        Container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);

        _root = CreateRoot();
        var AppEvents = Container.Resolve<IAppEvents>();

        _uiViewsStackController = _root.GetComponentInChildren<UIViewsStackController>();
        if(_uiViewsStackController != null)
        {
            _uiViewsStackController.AppEvents = AppEvents;
            Container.Rebind<UIViewsStackController>().ToInstance(_uiViewsStackController);
            Container.Rebind<UIViewsStackController>().ToLookup<UIViewsStackController>();

            UIViewController.ForceCloseEvent += _uiViewsStackController.OnForceCloseUIView;
        }

        var layers = _root.GetComponentInChildren<UILayersController>();
        if(layers != null)
        {
            Container.Rebind<UILayersController>().ToInstance(layers);
            UIViewController.DefaultLayersController = layers;
        }

        var notifications = _root.GetComponentInChildren<HUDNotificationsController>();
        if(notifications != null)
        {
            Container.Rebind<HUDNotificationsController>().ToInstance(notifications);
        }

        Container.Bind<IEventsBridge>().ToSingle<GUIControlBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<GUIControlBridge>();
    }

    GameObject CreateRoot()
    {
        var root = Resources.Load<GameObject>(GUIRootPrefab);
        if(root == null)
        {
            throw new InvalidOperationException("Could not load GUI root prefab.");
        }
        var rname = root.name;
        root = Instantiate<GameObject>(root);
        root.name = rname;
        DontDestroyOnLoad(root);
        return root;
    }

    string GetControllerFactoryPrefabName(Type type)
    {
        var name = type.Name;
        name = name.Replace(UIViewControllerSuffix, string.Empty);

        return string.Format("GUI_Example{0}", name);
    }

    public void Dispose()
    {
        UIViewController.ForceCloseEvent -= _uiViewsStackController.OnForceCloseUIView;
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);

        Destroy(_root);
    }
}