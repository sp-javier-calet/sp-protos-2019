#if ADMIN_PANEL

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.IO;
using UnityEngine;

namespace SocialPoint.AssetBundlesClient
{
    public sealed class AdminPanelAssetBundleManager : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly AssetBundleManager _assetBundleManager;

        AssetBundlesParsedData _remoteAssetBundlesParsedData = new AssetBundlesParsedData();
        AssetBundlesParsedData _localAssetBundlesParsedData = new AssetBundlesParsedData();
        AssetBundlesParsedData _mergedAssetBundlesParsedData = new AssetBundlesParsedData();

        AdminPanelConsole _console;
        AdminPanelLayout _layout;

        List<string> _scenes = new List<string>();
        List<string> _prefabs = new List<string>();

        const string _sceneSuffix = "_unity";
        const string _prefabSuffix = "_prefab";

        char[] _sceneSuffixCharArray = _sceneSuffix.ToCharArray();
        char[] _prefabSuffixCharArray = _prefabSuffix.ToCharArray();

        public AdminPanelAssetBundleManager(AssetBundleManager assetBundleManager)
        {
            _assetBundleManager = assetBundleManager;
        }

        public void OnConfigure(AdminPanel.AdminPanel adminPanel)
        {
            _console = adminPanel.Console;
            adminPanel.RegisterGUI("System", new AdminPanelNestedGUI("AssetBundleManager", this));
        }

        void ConsolePrint(string msg)
        {
            if(_console != null)
            {
                _console.Print(msg);
            }
        }

        public void OnCreateGUI(AdminPanelLayout layout)
        {
            FillFoldoutLists();

            _layout = layout;

            _layout.CreateLabel("AssetBundleManager");

            AddBasicInfo();

            AddCleanCacheButton();

            DownloadSceneFoldoutGUI(_layout.CreateFoldoutLayout("DownloadScene", true));
            DownloadAssetFoldoutGUI(_layout.CreateFoldoutLayout("DownloadAsset", true));

            _layout.CreateMargin();

            AddDownloadingErrorsPanel();
            AddLoadedAssetBundlesPanel();

            AddAssetBundlesParsedDataPanel(ParsedDataType.Merged);
            AddAssetBundlesParsedDataPanel(ParsedDataType.Remote);
            AddAssetBundlesParsedDataPanel(ParsedDataType.Local);
        }

        void AddCleanCacheButton()
        {
            _layout.CreateButton("Clean Cache", () => {
                bool succeeded = Caching.CleanCache();
                _console.Print("Clean Cache " + (succeeded ? "Succeeded" : "Failed, cache in use"));
            }
            );

            _layout.CreateMargin();
        }

        static AttrList GetBundlesDataAttrList()
        {
            const string bundleDataFile = "bundle_data.json";
            const string bundleDataKey = "bundle_data";

            string jsonPath = Path.Combine(PathsManager.StreamingAssetsPath, bundleDataFile);
            string json = FileUtils.ReadAllText(jsonPath);

            var bundlesAttrDic = new JsonAttrParser().ParseString(json).AssertDic;
            var bundleDataAttrList = bundlesAttrDic.Get(bundleDataKey).AssertList;
            return bundleDataAttrList;
        }

        void FillFoldoutLists()
        {
            if(_scenes.Count > 0 || _prefabs.Count > 0)
            {
                return;
            }

            _localAssetBundlesParsedData = Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_localAssetBundlesParsedData");
            _remoteAssetBundlesParsedData = Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_remoteAssetBundlesParsedData");

            if(_remoteAssetBundlesParsedData.Count == 0)
            {
                _console.Print("There is no Asset Bundles data parsed from the config manager.");
                _console.Print("Trying to load bundles from bundle_data.json from Streaming Assets");
                Reflection.CallPrivateVoidMethod<AssetBundleManager>(_assetBundleManager, "LoadBundleData", GetBundlesDataAttrList(), null);
            }

            _mergedAssetBundlesParsedData = Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_mergedAssetBundlesParsedData");

            var iter = _mergedAssetBundlesParsedData.GetEnumerator();
            while(iter.MoveNext())
            {
                var item = iter.Current;
                var itemName = item.Value.Name;
                if(itemName.EndsWith(_sceneSuffix))
                {
                    _scenes.Add(itemName.TrimEnd(_sceneSuffixCharArray));
                }
                else if(itemName.EndsWith(_prefabSuffix))
                {
                    _prefabs.Add(itemName.TrimEnd(_prefabSuffixCharArray));
                }
            }
            iter.Dispose();
        }

        void AddBasicInfo()
        {
            var baseDownloadingURL = Reflection.GetPrivateField<AssetBundleManager, string>(_assetBundleManager, "_baseDownloadingURL");
            var localAssetBundlesPath = Reflection.GetPrivateField<AssetBundleManager, string>(_assetBundleManager, "_localAssetBundlesPath");

            var content = new StringBuilder();
            content.AppendLine("Server: " + _assetBundleManager.Server);
            content.AppendLine("Game: " + _assetBundleManager.Game);
            content.AppendLine("Platorm: " + Utility.GetPlatformName());
            content.AppendLine("BaseDownloadingURL: " + baseDownloadingURL);
            content.AppendLine();
            content.AppendLine("Local Asset Bundles Path: " + localAssetBundlesPath);
            content.AppendLine();

            _layout.CreateVerticalLayout().CreateTextArea(content.ToString());
            _layout.CreateMargin();
        }

        public void DownloadSceneFoldoutGUI(AdminPanelLayout layout)
        {
            // Each lambda must capture a diferent reference, so it has to be a local variable
            for(int i = 0, _scenesCount = _scenes.Count; i < _scenesCount; i++)
            {
                var item = _scenes[i];
                var localString = item;
                layout.CreateButton(item, () => DownloadScene(localString));
            }
            _layout.CreateMargin();
        }

        public void DownloadAssetFoldoutGUI(AdminPanelLayout layout)
        {
            // Each lambda must capture a diferent reference, so it has to be a local variable
            for(int i = 0, _prefabsCount = _prefabs.Count; i < _prefabsCount; i++)
            {
                var item = _prefabs[i];
                var localString = item;
                layout.CreateButton(item, () => DownloadAsset(localString));
            }
            _layout.CreateMargin();
        }

        enum ParsedDataType
        {
            Local,
            Remote,
            Merged
        }

        void AddAssetBundlesParsedDataPanel(ParsedDataType dataType)
        {
            var assetBundlesParsedData = dataType == ParsedDataType.Local ? _localAssetBundlesParsedData : dataType == ParsedDataType.Remote ? _remoteAssetBundlesParsedData : _mergedAssetBundlesParsedData;
            if(assetBundlesParsedData.Count > 0)
            {
                _layout.CreateLabel(dataType == ParsedDataType.Local ? "LocalAssetBundlesParsedData" : dataType == ParsedDataType.Remote ? "RemoteAssetBundlesParsedData" : "MergedAssetBundlesParsedData");
                var content = new StringBuilder();
                var iter = assetBundlesParsedData.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendLine(item.Value.ToString());
                }
                iter.Dispose();
                _layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }
        }

        void AddLoadedAssetBundlesPanel()
        {
            var loadedAssetBundles = Reflection.GetPrivateField<AssetBundleManager, Dictionary<string, LoadedAssetBundle>>(_assetBundleManager, "_loadedAssetBundles");
            if(loadedAssetBundles.Count > 0)
            {
                _layout.CreateLabel("LoadedAssetBundles");
                var content = new StringBuilder();
                var iter = loadedAssetBundles.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendLine(item.Key + " - References: " + item.Value._referencedCount);
                }
                iter.Dispose();
                _layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }
        }

        void AddDownloadingErrorsPanel()
        {
            var downloadingErrors = Reflection.GetPrivateField<AssetBundleManager, Dictionary<string, string>>(_assetBundleManager, "_downloadingErrors");
            if(downloadingErrors.Count > 0)
            {
                _layout.CreateLabel("DownloadingErrors");
                var content = new StringBuilder();
                var iter = downloadingErrors.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendLine(item.Key);
                    content.AppendLine(item.Value);
                }
                iter.Dispose();
                _layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }
        }

        void DownloadScene(string sceneName)
        {
            _assetBundleManager.CoroutineRunner.StartCoroutine(InitializeLevelAsync(sceneName + _sceneSuffix, sceneName, AssetBundleLoadLevelOperation.LoadSceneBundleMode.Additive));
        }

        void DownloadAsset(string prefabName)
        {
            _assetBundleManager.CoroutineRunner.StartCoroutine(InstantiateGameObjectAsync(prefabName + _prefabSuffix, prefabName));
        }

        IEnumerator InitializeLevelAsync(string sceneAssetBundleName, string sceneName, AssetBundleLoadLevelOperation.LoadSceneBundleMode loadSceneMode)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            AssetBundleLoadLevelOperation request = null;
            yield return _assetBundleManager.LoadLevelAsyncRequest(sceneAssetBundleName, sceneName, loadSceneMode, req => request = req);

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            ConsolePrint(string.Format("Scene - {0} - {1} - Time: {2} seconds", sceneName, string.IsNullOrEmpty(request.Error) ? " OK" : request.Error, elapsedTime));

            _layout.Refresh();
        }

        IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            AssetBundleLoadAssetOperation request = null;
            yield return _assetBundleManager.LoadAssetAsyncRequest(assetBundleName, assetName, typeof(GameObject), req => request = req);

            // Get the asset.
            GameObject prefab = null;
            if(request != null)
            {
                prefab = request.GetAsset<GameObject>();
                if(prefab != null)
                {
                    Object.Instantiate(prefab, new Vector3(Random.Range(-10, 11), Random.Range(-10, 11), Random.Range(-10, 11)), Quaternion.identity);
                }
            }

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            ConsolePrint(string.Format("Prefab - {0} - {1} - Time: {2} seconds", assetName, prefab != null ? " OK" : request.Error, elapsedTime));

            _layout.Refresh();
        }
    }
}

#endif
