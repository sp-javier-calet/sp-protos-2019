using System;
using Zenject;
using SocialPoint.Alert;
using UnityEngine;
using SocialPoint.GUI;
using SocialPoint.Base;
using UnityEngine.Assertions;

public class AlertInstaller : MonoInstaller
{
    [Serializable]
    public class SettingsData
    {
        public bool UseNativeAlert = false;
        public string Prefab;
    }

    public SettingsData Settings;

    [Inject]
    PopupsController Popups;

    public override void InstallBindings()
    {
        if(Container.HasBinding<IAlertView>())
        {
            return;
        }

        UnityAlertView.ShowDelegate = (GameObject go) => {
            var viewController = go.GetComponent<UIViewController>();
            DebugUtils.Assert(viewController != null, "GameObject doesn't have a viewController");
            Popups.Push(viewController);
        };
        UnityAlertView.HideDelegate = (GameObject go) => {
            var viewController = go.GetComponent<UIViewController>();
            DebugUtils.Assert(viewController != null,  "GameObject doesn't have a viewController");
            viewController.Hide(true);
        };
        if(Settings.UseNativeAlert)
        {
            Container.Bind<IAlertView>().ToSingle<AlertView>();
        }
        else
        {
            var unityAlertView = new UnityAlertView(Settings.Prefab != string.Empty ? Settings.Prefab : UnityAlertView.DefaultPrefab);
            Container.Bind<IAlertView>().ToInstance(unityAlertView);
        }
    }
}


