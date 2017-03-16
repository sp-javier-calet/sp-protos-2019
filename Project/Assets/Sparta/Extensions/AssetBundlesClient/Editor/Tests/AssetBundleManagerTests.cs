using System;
using System.IO;
using NSubstitute;
using NUnit.Framework;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.IO;
using SocialPoint.Utils;
using UnityEngine;

namespace SocialPoint.AssetBundlesClient
{
    [TestFixture]
    [Category("SocialPoint.Locale")]
    public sealed class AssetBundleManagerTests
    {
        AssetBundleManager _assetBundleManager;
        Action<AssetBundleLoadLevelOperation> _loadLevelOperation;
        Action<AssetBundleLoadAssetOperation> _loadAssetOperation;

        [SetUp]
        public void SetUp()
        {
            _assetBundleManager = new AssetBundleManager();

            _assetBundleManager.Scheduler = Substitute.For<IUpdateScheduler>();
            _assetBundleManager.CoroutineRunner = Substitute.For<ICoroutineRunner>();

            _assetBundleManager.Init(GetBundleDataAttrList());

            _loadLevelOperation = Substitute.For<Action<AssetBundleLoadLevelOperation>>();
            _loadAssetOperation = Substitute.For<Action<AssetBundleLoadAssetOperation>>();
        }

        [TearDown]
        public void TearDown()
        {
            _assetBundleManager.Dispose();
        }

        [Test]
        public void InitDone()
        {
            var parsed = GetAssetBundlesParsedDataReflection();
            Assert.Greater(parsed.Count, 0);
        }

        [Test]
        public void DownloadPrefab()
        {
            const string assetBundleName = "prefab_1_prefab";
            const string assetName = "prefab_1";

            var asyncRequest = _assetBundleManager.LoadAssetAsyncRequest(assetBundleName, assetName, typeof(GameObject), _loadAssetOperation);
            _assetBundleManager.CoroutineRunner.StartCoroutine(asyncRequest);
            _loadAssetOperation.Received();

            //@TODO: find a way to test coroutines. The test is a fake now.
        }

        [Test]
        public void DownloadScene()
        {
            const string sceneAssetBundleName = "test_scene_unity";
            const string sceneName = "test_scene";
            var loadSceneMode = AssetBundleLoadLevelOperation.LoadSceneBundleMode.OnlyDownload;

            var asyncRequest = _assetBundleManager.LoadLevelAsyncRequest(sceneAssetBundleName, sceneName, loadSceneMode, _loadLevelOperation);
            _assetBundleManager.CoroutineRunner.StartCoroutine(asyncRequest);
            _loadLevelOperation.Received();

            //@TODO: find a way to test coroutines. The test is a fake now.
        }

        static AttrList GetBundleDataAttrList()
        {
            const string bundleDataFile = "bundle_data.json";
            const string bundleDataKey = "bundle_data";

            string jsonPath = Path.Combine(Application.streamingAssetsPath, bundleDataFile);
            string json = FileUtils.ReadAllText(jsonPath);

            var bundlesAttrDic = new JsonAttrParser().ParseString(json).AssertDic;
            var bundleDataAttrList = bundlesAttrDic.Get(bundleDataKey).AssertList;
            return bundleDataAttrList;
        }

        AssetBundlesParsedData GetAssetBundlesParsedDataReflection()
        {
            return Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_assetBundlesParsedData");
        }
    }
}