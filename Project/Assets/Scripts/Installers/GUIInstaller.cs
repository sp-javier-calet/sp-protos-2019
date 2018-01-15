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
    IAppEvents _appEvents;

    #region IInitializable implementation

    public void Initialize()
    {
        _appEvents = Container.Resolve<IAppEvents>();

        if(_stackController != null)
        {
            _stackController.AppEvents = _appEvents;
        }
            
#if ADMIN_PANEL
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelUI>(CreateAdminPanel);
#endif
    }

#endregion

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);

        Container.Add<IDisposable, GUIInstaller>(this);

        UIViewController.Factory.Define((UIViewControllerFactory.DefaultPrefabDelegate)GetControllerFactoryPrefabName);

        Container.Bind<float>("popup_animation_time").ToInstance(Settings.PopupAnimationTime);
        Container.Bind<float>("tooltip_animation_time").ToInstance(Settings.TooltipAnimationTime);

        _root = CreateRoot();

        _stackController = _root.GetComponentInChildren<ScreensController>();
        if(_stackController != null)
        {
            _stackController.CloseAppShow = ShowCloseAppAlertView;
            Container.Rebind<UIStackController>().ToInstance(_stackController);
        }
            
        var uiTooltipController = _root.GetComponentInChildren<UITooltipController>();
        if(uiTooltipController != null)
        {
            uiTooltipController.ScreenBoundsDelta = Settings.TooltipScreenBoundsDelta;
            Container.Rebind<UITooltipController>().ToInstance(uiTooltipController);
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

        Container.Bind<IScriptEventsBridge>().ToSingle<GUIControlBridge>();
    }
        
#if ADMIN_PANEL
    AdminPanelUI CreateAdminPanel()
    {
        var storage = Container.Resolve<IAttrStorage>(kPersistentTag);
        var iDeviceInfo = Container.Resolve<IDeviceInfo>();

        return new AdminPanelUI(iDeviceInfo, storage);
    }
#endif

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