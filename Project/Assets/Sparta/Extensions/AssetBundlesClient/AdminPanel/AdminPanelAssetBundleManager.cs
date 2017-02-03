using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using UnityEngine;

namespace SocialPoint.AssetBundlesClient
{
    public sealed class AdminPanelAssetBundleManager : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly AssetBundleManager _assetBundleManager;
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

            _layout.CreateMargin();

            DownloadSceneFoldoutGUI(_layout.CreateFoldoutLayout("DownloadScene"));
            DownloadAssetFoldoutGUI(_layout.CreateFoldoutLayout("DownloadAsset"));

            _layout.CreateMargin();

            AddAssetBundlesParsedDataPanel();
            AddLoadedAssetBundlesPanel();
            AddDownloadingErrorsPanel();
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

        AssetBundlesParsedData GetAssetBundlesParsedDataReflection()
        {
            return Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_assetBundlesParsedData");
        }

        void FillFoldoutLists()
        {
            if(_scenes.Count > 0 || _prefabs.Count > 0)
            {
                return;
            }

            var assetBundlesParsedData = GetAssetBundlesParsedDataReflection();
            if(assetBundlesParsedData.Count == 0)
            {
                // init assetBundleManager with fake data from resources folder
                _assetBundleManager.Init(GetBundleDataAttrList());
            }

            // request for assetBundlesParsedData again
            assetBundlesParsedData = GetAssetBundlesParsedDataReflection();
            var iter = assetBundlesParsedData.GetEnumerator();
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
            content.AppendLine("Server: " + _assetBundleManager.Server);
            content.AppendLine("Game: " + _assetBundleManager.Game);
            content.AppendLine("Platorm: " + Utility.GetPlatformName());
            _layout.CreateVerticalLayout().CreateTextArea(content.ToString());
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
            var assetBundlesParsedData = GetAssetBundlesParsedDataReflection();
            if(assetBundlesParsedData.Count > 0)
            {
                _layout.CreateLabel("AssetBundlesParsedData");
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
                    content.AppendLine(item.Key + " References: " + item.Value._referencedCount);
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
            _assetBundleManager.CoroutineRunner.StartCoroutine(InitializeLevelAsync(sceneName + _sceneSuffix, sceneName, true));
        }

        void DownloadAsset(string prefabName)
        {
            _assetBundleManager.CoroutineRunner.StartCoroutine(InstantiateGameObjectAsync(prefabName + _prefabSuffix, prefabName));
        }

        IEnumerator InitializeLevelAsync(string sceneAssetBundleName, string sceneName, bool isAdditive)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            // Load level from assetBundle.
            AssetBundleLoadOperation request = AssetBundleManager.LoadLevelAsync(sceneAssetBundleName, sceneName, isAdditive);
            if(request == null)
            {
                yield break;
            }
            yield return _assetBundleManager.CoroutineRunner.StartCoroutine(request);

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            ConsolePrint("Finished loading scene " + sceneName + " in " + elapsedTime + " seconds");

            _layout.Refresh();
        }

        IEnumerator InstantiateGameObjectAsync(string assetBundleName, string assetName)
        {
            // This is simply to get the elapsed time for this phase of AssetLoading.
            float startTime = Time.realtimeSinceStartup;

            // Load asset from assetBundle.
            AssetBundleLoadAssetOperation request = AssetBundleManager.LoadAssetAsync(assetBundleName, assetName, typeof(GameObject));
            if(request == null)
            {
                yield break;
            }
            yield return _assetBundleManager.CoroutineRunner.StartCoroutine(request);

            // Get the asset.
            GameObject prefab = request.GetAsset<GameObject>();

            if(prefab != null)
            {
                Object.Instantiate(prefab);
            }

            // Calculate and display the elapsed time.
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            ConsolePrint(assetName + (prefab == null ? " was not" : " was") + " loaded successfully in " + elapsedTime + " seconds");

            _layout.Refresh();

        }
    }
}
