using System;
using NSubstitute;
using NUnit.Framework;
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
            PathsManager.Init();

            _assetBundleManager = new AssetBundleManager();

            _assetBundleManager.Scheduler = Substitute.For<IUpdateScheduler>();
            _assetBundleManager.CoroutineRunner = Substitute.For<ICoroutineRunner>();
            _assetBundleManager.Setup();

            _loadLevelOperation = Substitute.For<Action<AssetBundleLoadLevelOperation>>();
            _loadAssetOperation = Substitute.For<Action<AssetBundleLoadAssetOperation>>();
        }

        [TearDown]
        public void TearDown()
        {
            _assetBundleManager.Dispose();
        }

        [Test]
        public void DownloadPrefab()
        {
            const string assetBundleName = "test_prefab_prefab";
            const string assetName = "test_prefab";

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
    }
}