using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using UnityEngine;

public class GUIInstaller : Installer, IDisposable
{
    const string UIViewControllerSuffix = "Controller";
    const string GUIRootPrefab = "GUI_Root";

    [Serializable]
    public class SettingsData
    {
        public float PopupAnimationTime = UIViewsStackController.DefaultAnimationTime;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;
    UIViewsStackController _uiViewsStackController;

    public override void InstallBindings()
    {
        Container.Add<IDisposable, GUIInstaller>(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        Container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);

        _root = CreateRoot();
        var AppEvents = Container.Resolve<IAppEvents>();
        var EventDispatcher = Container.Resolve<IEventDispatcher>();

        _uiViewsStackController = _root.GetComponentInChildren<UIViewsStackController>();
        if(_uiViewsStackController != null)
        {
            _uiViewsStackController.AppEvents = AppEvents;
            _uiViewsStackController.EventDispatcher = EventDispatcher;
            Container.Rebind<UIViewsStackController>().ToInstance(_uiViewsStackController);
            Container.Rebind<UIStackController>().ToLookup<UIStackController>();

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
        if(StringUtils.EndsWith(name, UIViewControllerSuffix))
        {
            name = name.Substring(0, name.Length - UIViewControllerSuffix.Length);
        }

        // Change to stringbuilder????
        return string.Format("GUI_{0}", name);
    }

    public void Dispose()
    {
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);
        Destroy(_root);
    }
}