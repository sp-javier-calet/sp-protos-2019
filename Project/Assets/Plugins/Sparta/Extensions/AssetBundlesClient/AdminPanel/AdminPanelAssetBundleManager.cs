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
        AssetBundlesParsedData _mergedAssetBundlesParsedData = new AssetBundlesParsedData();

        string _baseDownloadingURL;
        string _localAssetBundlesPath;

        AdminPanelConsole _console;
        AdminPanelLayout _layout;

        List<string> _scenes = new List<string>();
        List<string> _prefabs = new List<string>();

        const string _sceneSuffix = "_unity";
        const string _prefabSuffix = "_prefab";
        const string _remoteColor = "lime";
        const string _localColor = "aqua";
        const string _errorColor = "red";

        const float _secondsToDays = 1.0f / (60.0f * 60.0f * 24.0f);

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

            AddMergeIssuesPanel();
            AddAssetBundlesParsedDataPanel();
        }

        void AddCleanCacheButton()
        {
            _layout.CreateButton("Clean Cache", () =>
            {
#if UNITY_2017_1_OR_NEWER
                bool succeeded = Caching.ClearCache();
#else
                bool succeeded = Caching.CleanCache();
#endif
                _console.Print("Clean Cache " + (succeeded ? "Succeeded" : "Failed, cache in use"));
            }
            );

            _layout.CreateMargin();
        }

        static AttrList GetBundlesDataAttrList()
        {
            const string bundleDataFile = "local_bundle_data.json";
            const string bundleDataKey = "local_bundle_data";

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

            _baseDownloadingURL = Reflection.GetPrivateField<AssetBundleManager, string>(_assetBundleManager, "_baseDownloadingURL");
            _localAssetBundlesPath = Reflection.GetPrivateField<AssetBundleManager, string>(_assetBundleManager, "_localAssetBundlesPath");

            _remoteAssetBundlesParsedData = Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_remoteAssetBundlesParsedData");

            if(_remoteAssetBundlesParsedData.Count == 0)
            {
                _console.Print("There is no Asset Bundles data parsed from the config manager.");
                _console.Print("Trying to load bundles from bundle_data.json from Streaming Assets");
                Reflection.CallPrivateVoidMethod<AssetBundleManager>(_assetBundleManager, "LoadBundleData", GetBundlesDataAttrList(), false);
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
            var content = new StringBuilder();
            content.AppendLine("Server: " + _assetBundleManager.Data.Server);
            content.AppendLine("Game: " + _assetBundleManager.Data.Game);
            content.AppendLine("Names To Lower Case: " + _assetBundleManager.Data.TransformNamesToLowercase);
            content.AppendLine("Platorm: " + Utility.GetPlatformName());
            content.AppendLine("BaseDownloadingURL: " + _baseDownloadingURL);
            content.AppendLine();
            content.AppendLine("Local Asset Bundles Path: " + _localAssetBundlesPath);
            content.AppendLine();
            content.AppendLine("Caching.expirationDelay: " + Caching.expirationDelay * _secondsToDays);
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

        void AddAssetBundlesParsedDataPanel()
        {
            var assetBundlesParsedData = _mergedAssetBundlesParsedData;
            if(assetBundlesParsedData.Count > 0)
            {
                _layout.CreateLabel("AssetBundlesParsedData");
                var content = new StringBuilder();

                content.AppendFormat("<color={0}>remote bundles</color> - <color={1}>local bundles</color>\n\n", _remoteColor, _localColor);

                var iter = assetBundlesParsedData.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendFormat("<color={0}>", item.Value.RemoteIsNewest ? _remoteColor : _localColor);
                    content.AppendLine(item.Value.ToString());
                    content.Append("</color>");
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
                content.AppendFormat("<color={0}>", _errorColor);
                var iter = downloadingErrors.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendLine(item.Key);
                    content.AppendLine(item.Value);
                }
                iter.Dispose();
                content.Append("</color>");
                _layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }
        }

        void AddMergeIssuesPanel()
        {
            var mergeIssues = Reflection.GetPrivateField<AssetBundleManager, List<string>>(_assetBundleManager, "_mergeIssues");
            if(mergeIssues.Count > 0)
            {
                _layout.CreateLabel("Merge Issues");
                var content = new StringBuilder();
                content.AppendFormat("<color={0}>", _errorColor);
                var iter = mergeIssues.GetEnumerator();
                while(iter.MoveNext())
                {
                    var item = iter.Current;
                    content.AppendLine(item);
                }
                iter.Dispose();
                content.Append("</color>");
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
            string requestOK = string.Format("<color={0}> OK </color>", _remoteColor);
            string requestError = string.Format("<color={0}> {1}</color>", _errorColor, request.Error);
            ConsolePrint(string.Format("Scene - {0} - {1} - Time: {2} seconds", sceneName, string.IsNullOrEmpty(request.Error) ? requestOK : requestError, elapsedTime));

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
                    Object.Instantiate(prefab, new Vector3(Random.Range(-5, 6), Random.Range(-5, 6), Random.Range(-5, 6)), Quaternion.identity);
                }
            }

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            string requestOK = string.Format("<color={0}> OK </color>", _remoteColor);
            string requestError = string.Format("<color={0}>{1}</color>", _errorColor, request.Error);
            ConsolePrint(string.Format("Prefab - {0} - {1} - Time: {2} seconds", assetName, prefab != null ? requestOK : requestError, elapsedTime));

            _layout.Refresh();
        }
    }
}

#endif
