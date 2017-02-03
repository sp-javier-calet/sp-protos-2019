using System;
using SocialPoint.AdminPanel;
using SocialPoint.Dependency;
using SocialPoint.Utils;

namespace SocialPoint.AssetBundlesClient
{
    public class AssetBundleManagerInstaller : ServiceInstaller
    {
        [Serializable]
        public class SettingsData
        {
            public string Server = AssetBundleManager.DefaultServer;
            public string Game = AssetBundleManager.DefaultGame;
        }

        public SettingsData Settings = new SettingsData();


        public override void InstallBindings()
        {
            Container.Rebind<AssetBundleManager>().ToMethod<AssetBundleManager>(CreateAssetBundleManager, SetupAssetBundleManager);
            Container.Bind<IDisposable>().ToLookup<AssetBundleManager>();

            Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelAssetBundleManager>(CreateAdminPanel);
        }

        AdminPanelAssetBundleManager CreateAdminPanel()
        {
            return new AdminPanelAssetBundleManager(Container.Resolve<AssetBundleManager>());
        }

        AssetBundleManager CreateAssetBundleManager()
        {
            return new AssetBundleManager();
        }

        void SetupAssetBundleManager(AssetBundleManager mng)
        {
            mng.Server = Settings.Server;
            mng.Game = Settings.Game;

            mng.Scheduler = Container.Resolve<IUpdateScheduler>();
            mng.CoroutineRunner = Container.Resolve<ICoroutineRunner>();
        }
    }
}