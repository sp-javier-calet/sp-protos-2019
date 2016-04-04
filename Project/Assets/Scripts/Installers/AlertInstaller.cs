using System;
using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.Alert;
using SocialPoint.GUIControl;
using SocialPoint.Base;
using SocialPoint.ScriptEvents;

public class AlertInstaller : MonoInstaller
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
            Container.Rebind<IAlertView>().ToSingleInstance(unityAlertView);
            Container.Bind<IDisposable>().ToLookup<IAlertView>();
        }

        Container.Bind<IEventsBridge>().ToSingle<AlertBridge>();
        Container.Bind<IScriptEventsBridge>().ToSingle<AlertBridge>();
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


