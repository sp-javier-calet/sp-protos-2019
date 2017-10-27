﻿using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using UnityEngine;
using System.Text;
using SocialPoint.Base;

public class GUIInstaller : Installer, IDisposable
{
    const string kUIViewUnitySuffix = "Unity";
    const string kUIViewControllerSuffix = "Controller";
    const string kGUIRootPrefab = "GUI_Root";
    const string kUIViewControllerExamplePrefix = "GUI_";

    const float DefaultAnimationTime = 1.0f;

    [Serializable]
    public class SettingsData
    {
        public float PopupAnimationTime = DefaultAnimationTime;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;
    UIStackController _stackController;
    IAppEvents _appEvents;

    public override void InstallBindings()
    {
        Container.Add<IDisposable, GUIInstaller>(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        Container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);

        _root = CreateRoot();
        _appEvents = Container.Resolve<IAppEvents>();

        _stackController = _root.GetComponentInChildren<ScreensController>();
        if(_stackController != null)
        {
            _stackController.AppEvents = _appEvents;
            _stackController.CloseAppShow = ShowCloseAppAlertView;
            Container.Rebind<UIStackController>().ToInstance(_stackController);
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

    void ShowCloseAppAlertView()
    {
        try
        {  
            var alert = Container.Resolve<IAlertView>();
            if(alert == null)
            {
                throw new InvalidOperationException("Could not resolve Alert View");
            }

            var _closeAppPopup = (IAlertView)alert.Clone();
            _closeAppPopup.Title = "CLOSE APP";
            _closeAppPopup.Message = "Do you want to close this app?";
            _closeAppPopup.Input = false;
            _closeAppPopup.Buttons = new []{ "YES", "NO" };
            _closeAppPopup.Show(result => 
            {
                if(result == 0)
                {
                    _appEvents.KillGame();
                }

                _closeAppPopup = null;
            });
        }
        catch(Exception e)
        {
            Log.e("Exception while creating Alert View - " + e.Message);
        }
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
        name = name.Replace(kUIViewUnitySuffix, string.Empty);
        name = name.Replace(kUIViewControllerSuffix, string.Empty);

        StringBuilder stringBuilder = StringUtils.StartBuilder();
        stringBuilder.Append(kUIViewControllerExamplePrefix);
        stringBuilder.Append(name);
        return StringUtils.FinishBuilder(stringBuilder);
    }

    public void Dispose()
    {
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);
        Destroy(_root);
    }
}