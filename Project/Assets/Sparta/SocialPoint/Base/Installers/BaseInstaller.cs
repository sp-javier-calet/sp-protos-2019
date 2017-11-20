using System;
using SocialPoint.Dependency;
using SocialPoint.Utils;
using UnityEngine;
using SocialPoint.Crash;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Base
{
    public class BaseInstaller : ServiceInstaller, IInitializable
    {
        [Serializable]
        public class SettingsData
        {
            [Tooltip("If true exceptions during Update() for IUpdateScheduler will be tracked as handled exceptions one by one")]
            public bool TrackUpdateExceptionsAsHandled;
        }

        public SettingsData Settings = new SettingsData();

        public override void InstallBindings()
        {
            Container.Bind<IInitializable>().ToInstance(this);
            Container.RebindUnityComponent<UnityUpdateRunner>();
            Container.Rebind<ICoroutineRunner>().ToLookup<UnityUpdateRunner>();
            Container.Rebind<IUpdateScheduler>().ToLookup<UnityUpdateRunner>();
            Container.Bind<NativeCallsHandler>().ToMethod<NativeCallsHandler>(CreateNativeCallsHandler);

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNativeCallsHandler>(CreateAdminPanel);
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelDependency>(CreateAdminPanelDependency);
            #endif
        }

        public void Initialize()
        {
            var scheduler = Container.Resolve<IUpdateScheduler>();
            var updateables = Container.ResolveList<IUpdateable>().ToArray();
            if(updateables != null)
            {
                scheduler.Add(updateables);
            }

            if(Settings.TrackUpdateExceptionsAsHandled)
            {
                scheduler.OnExceptionInUpdate += Container.Resolve<ICrashReporter>().ReportHandledException;
            }
        }

        NativeCallsHandler CreateNativeCallsHandler()
        {
            var trans = Container.Resolve<Transform>();
            var go = new GameObject();
            UnityEngine.Object.DontDestroyOnLoad(go);
            go.transform.SetParent(trans);
            return go.AddComponent<NativeCallsHandler>();
        }

        #if ADMIN_PANEL
        AdminPanelNativeCallsHandler CreateAdminPanel()
        {
            return new AdminPanelNativeCallsHandler(
                Container.Resolve<NativeCallsHandler>());
        }

        AdminPanelDependency CreateAdminPanelDependency()
        {
            return new AdminPanelDependency();
        }
        #endif
    }
}
