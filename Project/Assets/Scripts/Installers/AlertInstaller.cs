using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Alert;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.ScriptEvents;

public class AlertInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public bool UseNativeAlert = false;
        public GameObject UnityAlertViewPrefab;
    }

    public SettingsData Settings = new SettingsData();

    PopupsController _popups;

    public override void InstallBindings()
    {
        UnityAlertView.ShowDelegate = ShowUnityAlert;
        UnityAlertView.HideDelegate = HideUnityAlert;
        if(Settings.UseNativeAlert)
        {
            Container.Rebind<IAlertView>().ToSingle<AlertView>();
        }
        else
        {
            var unityAlertView = new UnityAlertView(Settings.UnityAlertViewPrefab);
            Container.Rebind<IAlertView>().ToInstance(unityAlertView);
            Container.Bind<IDisposable>().ToLookup<IAlertView>();
        }

        Container.Bind<AlertBridge>().ToMethod<AlertBridge>(CreateAlertBridge);
        Container.Bind<IEventsBridge>().ToLookup<AlertBridge>();
        Container.Bind<IScriptEventsBridge>().ToLookup<AlertBridge>();
    }

    public AlertBridge CreateAlertBridge()
    {
        return new AlertBridge(Container.Resolve<IAlertView>());
    }        

    void ShowUnityAlert(GameObject go)
    {
        var ctrl = go.GetComponent<UIViewController>();
        DebugUtils.Assert(ctrl != null, "GameObject doesn't have a viewController");
        if(_popups == null)
        {
            _popups = GameObject.FindObjectOfType<PopupsController>();
        }
        if(_popups != null)
        {
            _popups.Push(ctrl);
        }
        else
        {
            ctrl.Show();
        }
    }

    void HideUnityAlert(GameObject go)
    {
        var ctrl = go.GetComponent<UIViewController>();
        DebugUtils.Assert(ctrl != null,  "GameObject doesn't have a viewController");
        ctrl.Hide(true);
    }
}


