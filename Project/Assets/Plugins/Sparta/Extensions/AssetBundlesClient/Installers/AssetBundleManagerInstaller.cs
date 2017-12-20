using System;
using SocialPoint.Dependency;
using SocialPoint.Utils;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

namespace SocialPoint.AssetBundlesClient
{
    public class AssetBundleManagerInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public string Server = AssetBundleManager.DefaultServer;
            public string Game = AssetBundleManager.DefaultGame;
            public bool TransformNamesToLowercase = false;
        }

        public SettingsData Settings = new SettingsData();


        public override void InstallBindings()
        {
            Container.Rebind<AssetBundleManager>().ToMethod<AssetBundleManager>(CreateAssetBundleManager, SetupAssetBundleManager);
            Container.Bind<IDisposable>().ToLookup<AssetBundleManager>();

            #if ADMIN_PANEL
            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAssetBundleManager>(CreateAdminPanel);
            #endif
        }

        #if ADMIN_PANEL
        AdminPanelAssetBundleManager CreateAdminPanel()
        {
            return new AdminPanelAssetBundleManager(Container.Resolve<AssetBundleManager>());
        }
        #endif

        static AssetBundleManager CreateAssetBundleManager()
        {
            return new AssetBundleManager();
        }

        void SetupAssetBundleManager(AssetBundleManager mng)
        {
            mng.Data = Settings;

            mng.Scheduler = Container.Resolve<IUpdateScheduler>();
            mng.CoroutineRunner = Container.Resolve<ICoroutineRunner>();
            mng.Setup();
        }
    }
}