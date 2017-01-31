using System.Collections;
using System.Collections.Generic;
using System.Text;
using SocialPoint.AdminPanel;
using UnityEngine;

namespace SocialPoint.AssetBundlesClient
{
    public sealed class AdminPanelAssetBundleManager : IAdminPanelConfigurer, IAdminPanelGUI
    {
        readonly AssetBundleManager _assetBundleManager;
        AdminPanelConsole _console;
        AdminPanelLayout _layout;

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
            _layout = layout;

            layout.CreateLabel("Info");
            var content = new StringBuilder();
            content.AppendLine("Server: " + _assetBundleManager.Server);
            content.AppendLine("Game: " + _assetBundleManager.Game);
            content.AppendLine("Platorm: " + Utility.GetPlatformName());

            layout.CreateVerticalLayout().CreateTextArea(content.ToString());

            layout.CreateMargin();

            var assetBundlesParsedData = Reflection.GetPrivateField<AssetBundleManager, AssetBundlesParsedData>(_assetBundleManager, "_assetBundlesParsedData");
            var loadedAssetBundles = Reflection.GetPrivateField<AssetBundleManager, Dictionary<string, LoadedAssetBundle>>(_assetBundleManager, "_loadedAssetBundles");
            var downloadingErrors = Reflection.GetPrivateField<AssetBundleManager, Dictionary<string, string>>(_assetBundleManager, "_downloadingErrors");

            if(assetBundlesParsedData.Count > 0)
            {
                layout.CreateLabel("AssetBundlesParsedData");
                content = new StringBuilder();
                foreach(var item in assetBundlesParsedData)
                {
                    content.AppendLine(item.Value.ToString());
                }
                layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }

            if(loadedAssetBundles.Count > 0)
            {
                layout.CreateLabel("LoadedAssetBundles");
                content = new StringBuilder();
                foreach(var item in loadedAssetBundles)
                {
                    content.AppendLine(item.Key + " References: " + item.Value._referencedCount);
                }
                layout.CreateVerticalScrollLayout().CreateTextArea(content.ToString());
            }

            if(downloadingErrors.Count > 0)
            {
                layout.CreateLabel("DownloadingErrors");
                content = new StringBuilder();
                foreach(var item in downloadingErrors)
                {
                    content.AppendLine(item.Key);
                    content.AppendLine(item.Value);
                }
                layout.CreateTextArea(content.ToString());
            }

            layout.CreateMargin();

            layout.CreateButton("DownloadScene", DownloadScene);
            layout.CreateButton("DownloadAsset", DownloadAsset);
        }

        void DownloadScene()
        {
            _assetBundleManager.CoroutineRunner.StartCoroutine(InitializeLevelAsync("test_scene_unity", "test_scene", true));
        }

        void DownloadAsset()
        {
            _assetBundleManager.CoroutineRunner.StartCoroutine(InstantiateGameObjectAsync("prefab_1_prefab", "prefab_1"));
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
