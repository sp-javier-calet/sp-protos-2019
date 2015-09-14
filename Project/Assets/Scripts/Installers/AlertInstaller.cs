using System;
using Zenject;
using SocialPoint.Alert;
using UnityEngine;
using SocialPoint.GUI;
using SocialPoint.Base;

public class AlertInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseNativeAlert = false;
        public GameObject Prefab;
    }

    public SettingsData Settings;

    PopupsController _popups;

    public override void InstallBindings()
    {
        if(Container.HasBinding<IAlertView>())
        {
            return;
        }

        UnityAlertView.ShowDelegate = ShowUnityAlert;
        UnityAlertView.HideDelegate = HideUnityAlert;
        if(Settings.UseNativeAlert)
        {
            Container.Bind<IAlertView>().ToSingle<AlertView>();
        }
        else
        {
            var unityAlertView = new UnityAlertView(Settings.Prefab);
            Container.Bind<IAlertView>().ToSingleInstance(unityAlertView);
        }
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


