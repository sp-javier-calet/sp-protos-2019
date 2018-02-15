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
            Container.Listen<IUpdateable>().Then(OnUpdateableResolved);
            Container.Listen<IDeltaUpdateable>().Then(OnUpdateableResolved);
            Container.Listen<IDeltaUpdateable<int>>().Then(OnUpdateableResolved);

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelNativeCallsHandler>(CreateAdminPanel);
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelDependency>(CreateAdminPanelDependency);
            #endif
        }

        void OnUpdateableResolved(IUpdateable updateable)
        {
            var scheduler = Container.Resolve<IUpdateScheduler>();
            scheduler.Add(updateable);
        }

        void OnUpdateableResolved(IDeltaUpdateable deltaUpdateable)
        {
            var scheduler = Container.Resolve<IUpdateScheduler>();
            scheduler.Add(deltaUpdateable);
        }

        void OnUpdateableResolved(IDeltaUpdateable<int> deltaUpdateable)
        {
            var scheduler = Container.Resolve<IUpdateScheduler>();
            scheduler.Add(deltaUpdateable);
        }

        public void Initialize()
        {
            Container.Resolve<IUpdateScheduler>();
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
