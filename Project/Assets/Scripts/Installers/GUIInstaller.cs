using System;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using UnityEngine;
using System.Text;

public class GUIInstaller : Installer, IDisposable
{
    const string kUIViewControllerSuffix = "Controller";
    const string kGUIRootPrefab = "GUI_Root";
    const string kUIViewControllerExamplePrefix = "GUI_";

    [Serializable]
    public class SettingsData
    {
        public float PopupAnimationTime = PopupsController.DefaultAnimationTime;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;

    public override void InstallBindings()
    {
        Container.Add<IDisposable, GUIInstaller>(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        Container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);

        _root = CreateRoot();
        var AppEvents = Container.Resolve<IAppEvents>();
        var popups = _root.GetComponentInChildren<PopupsController>();
        if(popups != null)
        {
            popups.AppEvents = AppEvents;
            Container.Rebind<PopupsController>().ToInstance(popups);
            Container.Rebind<UIStackController>().ToLookup<PopupsController>();
        }

        var screens = _root.GetComponentInChildren<ScreensController>();
        if(screens != null)
        {
            screens.AppEvents = AppEvents;
            Container.Rebind<ScreensController>().ToInstance(screens);
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
        var root = Resources.Load<GameObject>(kGUIRootPrefab);
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
        name = name.Replace(kUIViewControllerSuffix, string.Empty);

        StringBuilder stringBuilder = StringUtils.StartBuilder();
        stringBuilder.Append(kUIViewControllerExamplePrefix);
        stringBuilder.Append(name);
        return StringUtils.FinishBuilder(stringBuilder);
    }

    public void Dispose()
    {
//        UIViewController.ForceCloseEvent -= _uiViewsStackController.OnForceCloseUIView;

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);
        Destroy(_root);
    }
}