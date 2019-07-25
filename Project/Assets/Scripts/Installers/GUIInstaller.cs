//-----------------------------------------------------------------------
// GUIInstaller.cs
//
// Copyright 2019 Social Point SL. All rights reserved.
//
//-----------------------------------------------------------------------

using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using UnityEngine;
using System.Text;
using SocialPoint.Base;
using SocialPoint.Hardware;

public class GUIInstaller : Installer, IDisposable, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        const float DefaultAnimationTime = 1.0f;

        public float PopupAnimationTime = DefaultAnimationTime;
        public float TooltipAnimationTime = DefaultAnimationTime;
        public Vector2 TooltipScreenBoundsDelta = Vector2.zero;

        public GameObject GUIRootPrefab;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;
    UIStackController _stackController;
    UITooltipController _uiTooltipController;
    IDeviceInfo _deviceInfo;
    IAppEvents _appEvents;

    static StringBuilder _stringBuilder = new StringBuilder();

    #region IInitializable implementation

    public void Initialize(IResolutionContainer container)
    {
        _appEvents = container.Resolve<IAppEvents>();
        if(_stackController != null)
        {
            _stackController.AppEvents = _appEvents;
        }
    }

    #endregion

    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IInitializable>().ToInstance(this);

        container.Bind<IDisposable>().ToInstance(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);
        container.Bind<float>("tooltip_animation_time").ToInstance(Settings.TooltipAnimationTime);

        _root = CreateRoot();

        _stackController = _root.GetComponentInChildren<ScreensController>(true);
        if(_stackController != null)
        {
            _stackController.CloseAppShow = ShowCloseAppAlertView;
            container.Bind<UIStackController>().ToInstance(_stackController);
        }

        _uiTooltipController = _root.GetComponentInChildren<UITooltipController>(true);
        if(_uiTooltipController != null)
        {
            var safeArea = _uiTooltipController.RT.GetComponent<UISafeAreaView>();
            if(safeArea != null)
            {
                safeArea.ForceScreenBoundsDelta = true;
                safeArea.ScreenBoundsDelta = Settings.TooltipScreenBoundsDelta;
                safeArea.UpdateSafeArea();
            }

            container.Bind<UITooltipController>().ToInstance(_uiTooltipController);
        }

        container.Bind<HUDNotificationsController>().ToMethod(CreateHUDNotificationsController);
        container.Bind<IScriptEventsBridge>().ToSingle<GUIControlBridge>();
    }

    void ShowCloseAppAlertView()
    {
        try
        {
            var alert = Services.Instance.Resolve<IAlertView>();
            if(alert == null)
            {
                throw new InvalidOperationException("Could not resolve Alert View");
            }

            var _closeAppPopup = (IAlertView)alert.Clone();
            _closeAppPopup.Title = "CLOSE APP";
            _closeAppPopup.Message = "Do you want to close this app?";
            _closeAppPopup.Input = false;
            _closeAppPopup.Buttons = new[] { "YES", "NO" };
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
        var root = Settings.GUIRootPrefab;
        if(root == null)
        {
            throw new MissingReferenceException("Could not load GUI root prefab.");
        }

        root = Instantiate(root);
        DontDestroyOnLoad(root);
        return root;
    }

    static string GetControllerFactoryPrefabName(Type type)
    {
        const string kUIViewUnitySuffix = "Unity";
        const string kUIControllerSuffix = "Controller";
        const string kUIViewSuffix = "View";
        const string kUIViewControllerPrefix = "GUI_";

        var name = type.Name;
        name = name.Replace(kUIViewUnitySuffix, string.Empty);
        name = name.Replace(kUIViewSuffix, string.Empty);
        name = name.Replace(kUIControllerSuffix, string.Empty);

        _stringBuilder.Length = 0;
        _stringBuilder.Append(kUIViewControllerPrefix);
        _stringBuilder.Append(name);

        return _stringBuilder.ToString();
    }

    HUDNotificationsController CreateHUDNotificationsController(IResolutionContainer container)
    {
        return _root.GetComponentInChildren<HUDNotificationsController>(true); ;
    }

    public void Dispose()
    {
        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)null);
        Destroy(_root);
    }
}
