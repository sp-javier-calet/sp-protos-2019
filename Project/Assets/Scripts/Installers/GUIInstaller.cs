using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using SocialPoint.Utils;
using UnityEngine;
using System.Text;
using SocialPoint.Base;
#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif
using SocialPoint.Attributes;
using SocialPoint.Hardware;


public class GUIInstaller : Installer, IDisposable, IInitializable
{
    const string kUIViewUnitySuffix = "Unity";
    const string kUIViewControllerSuffix = "Controller";
    const string kGUIRootPrefab = "GUI_Root";
    const string kUIViewControllerExamplePrefix = "GUI_";
    const string kPersistentTag = "persistent";

    const float DefaultAnimationTime = 1.0f;

    [Serializable]
    public class SettingsData
    {
        public float PopupAnimationTime = DefaultAnimationTime;
        public float TooltipAnimationTime = DefaultAnimationTime;
        public Vector2 TooltipScreenBoundsDelta = Vector2.zero;
    }

    public SettingsData Settings = new SettingsData();

    GameObject _root;
    UIStackController _stackController;
    UITooltipController _uiTooltipController;
    IDeviceInfo _deviceInfo;
    IAppEvents _appEvents;

    #region IInitializable implementation

    public void Initialize(IResolutionContainer container)
    {
        _appEvents = container.Resolve<IAppEvents>();
        if(_stackController != null)
        {
            _stackController.AppEvents = _appEvents;
        }
        _deviceInfo = container.Resolve<IDeviceInfo>();
        if(_uiTooltipController != null)
        {
            _uiTooltipController.DeviceInfo = _deviceInfo;
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

        _stackController = _root.GetComponentInChildren<ScreensController>();
        if(_stackController != null)
        {
            _stackController.CloseAppShow = ShowCloseAppAlertView;
            container.Bind<UIStackController>().ToInstance(_stackController);
        }

        _uiTooltipController = _root.GetComponentInChildren<UITooltipController>();
        if(_uiTooltipController != null)
        {
            _uiTooltipController.ScreenBoundsDelta = Settings.TooltipScreenBoundsDelta;
            container.Bind<UITooltipController>().ToInstance(_uiTooltipController);
        }

        var layers = _root.GetComponentInChildren<UILayersController>();
        if(layers != null)
        {
            container.Bind<UILayersController>().ToInstance(layers);
            UIViewController.DefaultLayersController = layers;
        }

        var notifications = _root.GetComponentInChildren<HUDNotificationsController>();
        if(notifications != null)
        {
            container.Bind<HUDNotificationsController>().ToInstance(notifications);
        }

        container.Bind<IScriptEventsBridge>().ToSingle<GUIControlBridge>();

#if ADMIN_PANEL
        container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelUI>(CreateAdminPanel);
#endif
    }

#if ADMIN_PANEL
    AdminPanelUI CreateAdminPanel(IResolutionContainer container)
    {
        var storage = container.Resolve<IAttrStorage>(kPersistentTag);

        return new AdminPanelUI(_deviceInfo, storage);
    }
#endif

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

    static GameObject CreateRoot()
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

    static string GetControllerFactoryPrefabName(Type type)
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
