#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#define NATIVE_ALERTVIEW
#endif

using System;
using SocialPoint.AdminPanel;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using UnityEngine;

namespace SocialPoint.Alert
{

    public class AlertInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public bool UseNativeAlert;
            public GameObject UnityAlertViewPrefab;
        }

        public SettingsData Settings = new SettingsData();

        UIStackController _popups;

        static bool IsNativeViewAvailable
        {
            get
            {
                #if NATIVE_ALERTVIEW
                return true;
                #else
                return false;
                #endif 
            }
        }

        public override void InstallBindings()
        {
            UnityAlertView.ShowDelegate = ShowUnityAlert;
            UnityAlertView.HideDelegate = HideUnityAlert;

            if(Settings.UseNativeAlert && IsNativeViewAvailable)
            {
                Container.Rebind<IAlertView>().ToMethod<AlertView>(CreateAlertView);
            }
            else
            {
                var unityAlertView = new UnityAlertView(Settings.UnityAlertViewPrefab);
                Container.Rebind<IAlertView>().ToInstance(unityAlertView);
            }
            Container.Bind<IDisposable>().ToLookup<IAlertView>();

            Container.Bind<AlertBridge>().ToMethod<AlertBridge>(CreateAlertBridge);
            Container.Bind<IEventsBridge>().ToLookup<AlertBridge>();
            Container.Bind<IScriptEventsBridge>().ToLookup<AlertBridge>();

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAlert>(CreateAdminPanel);
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
                _popups = Container.Resolve<UIStackController>();
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

        AlertView CreateAlertView()
        {
            var alert = new AlertView();

            #if UNITY_IOS && !UNITY_EDITOR
        if(alert is IosAlertView)
        {
            (alert as IosAlertView).NativeHandler = Container.Resolve<SocialPoint.Utils.NativeCallsHandler>();
        }
            #endif
            return alert;
        }

        static void HideUnityAlert(GameObject go)
        {
            var ctrl = go.GetComponent<UIViewController>();
            DebugUtils.Assert(ctrl != null, "GameObject doesn't have a viewController");
            ctrl.Hide(true);
        }

        AdminPanelAlert CreateAdminPanel()
        {
            return new AdminPanelAlert(Container.Resolve<IAlertView>());
        }
    }
}
