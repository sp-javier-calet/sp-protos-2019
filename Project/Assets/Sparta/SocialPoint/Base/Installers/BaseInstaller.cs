using SocialPoint.Dependency;
using SocialPoint.Utils;
using UnityEngine;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.Base
{
    public class BaseInstaller : ServiceInstaller, IInitializable
    {
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
        }

        NativeCallsHandler CreateNativeCallsHandler()
        {
            var trans = Container.Resolve<Transform>();
            var go = new GameObject();
            Object.DontDestroyOnLoad(go);
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
