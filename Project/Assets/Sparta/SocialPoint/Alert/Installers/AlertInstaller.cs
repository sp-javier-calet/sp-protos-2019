#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
#define NATIVE_ALERTVIEW
#endif

using System;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.ScriptEvents;
using UnityEngine;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

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

        UIStackController _stackController;

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

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAlert>(CreateAdminPanel);
            #endif
        }

        public AlertBridge CreateAlertBridge()
        {
            return new AlertBridge(Container.Resolve<IAlertView>());
        }

        void ShowUnityAlert(GameObject go)
        {
            var ctrl = go.GetComponent<UIViewController>();
            ctrl.IsFullScreen = false;
            DebugUtils.Assert(ctrl != null, "GameObject doesn't have a viewController");
            if(_stackController == null)
            {
                _stackController = Container.Resolve<UIStackController>();
            }

            if(_stackController != null)
            {
                _stackController.Push(ctrl);
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
            ctrl.Close();
        }

        #if ADMIN_PANEL
        AdminPanelAlert CreateAdminPanel()
        {
            return new AdminPanelAlert(Container.Resolve<IAlertView>());
        }
        #endif
    }
}
