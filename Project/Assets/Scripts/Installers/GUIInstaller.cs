using System;
using Zenject;
using UnityEngine;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.ScriptEvents;



public class GUIInstaller : MonoInstaller, IInitializable, IDisposable
{
    const string UIViewControllerSuffix = "Controller";
    const string GUIRootPrefab = "GUI_Root";

    [Serializable]
    public class SettingsData
    {
        public float PopupFadeSpeed = PopupsController.DefaultFadeSpeed;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;

    public override void InstallBindings()
    {
        Container.Bind<IDisposable>().ToSingleInstance(this);
        Container.Bind<IInitializable>().ToSingleInstance(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);
        UIViewController.AwakeEvent += OnViewControllerAwake;

        Container.BindInstance("popup_fade_speed", Settings.PopupFadeSpeed);

        _root = CreateRoot();
        var popups = _root.GetComponentInChildren<PopupsController>();
        if(popups != null)
        {
            Container.Rebind<PopupsController>().ToSingleInstance(popups);
        }
        var screens = _root.GetComponentInChildren<ScreensController>();
        if(screens != null)
        {
            Container.Rebind<ScreensController>().ToSingleInstance(screens);
        }
        var layers = _root.GetComponentInChildren<UILayersController>();
        if(layers != null)
        {
            Container.Rebind<UILayersController>().ToSingleInstance(layers);
            UIViewController.DefaultLayersController = layers;
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

    public void Initialize()
    {
        Container.InjectGameObject(_root);
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