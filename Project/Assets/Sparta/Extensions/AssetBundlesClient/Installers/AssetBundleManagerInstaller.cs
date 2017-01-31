using System;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
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

            mng.Init(GetBundleDataAttrList());
        }

        static AttrList GetBundleDataAttrList()
        {
            const string bundleDataJsonId = "bundle_data";
            var bundleDataAttrList = new AttrList();
            var resource = UnityEngine.Resources.Load(bundleDataJsonId);
            if(resource != null)
            {
                var textAsset = resource as UnityEngine.TextAsset;
                if(textAsset != null)
                {
                    var json = textAsset.text;
                    var bundlesAttrDic = new JsonAttrParser().ParseString(json).AssertDic;
                    bundleDataAttrList = bundlesAttrDic.Get(bundleDataJsonId).AssertList;
                }
            }
            return bundleDataAttrList;
        }
    }
}