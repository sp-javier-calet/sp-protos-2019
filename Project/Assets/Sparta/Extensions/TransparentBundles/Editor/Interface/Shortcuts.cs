using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public static class Shortcuts
    {
        static EditorClientController _controller;

        static void Init()
        {
            if(_controller == null)
            {
                _controller = EditorClientController.GetInstance();
            }
        }

        [MenuItem("Assets/Bundles/Create or Update Bundle")]
        public static void CreateOrUpdate()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());
            if(assets.Count > 0 && _controller.CreateOrUpdateBundles(assets))
            {
                BundlesWindow.OpenWindow();
                if(BundlesWindow.Window.position.xMin == 0 && BundlesWindow.Window.position.yMin == 0)
                {
                    EditorUtility.DisplayDialog("Transparent Bundles", "The selected assets will be created or updated shortly.", "Close");
                }
            }
        }

        [MenuItem("Assets/Bundles/Remove Bundle")]
        public static void Remove()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());

            if(assets.Count > 1)
            {
                EditorUtility.DisplayDialog("Removing Bundle",
                    "The bundle removal operation can only be done with one asset at a time. If you want to delete multiple bundles open the Bundles Window (Social Point > Bundles)",
                    "Close");
            }
            else if(assets.Count > 0 && EditorUtility.DisplayDialog("Removing Bundle",
                        "You are about to remove the bundle of the asset '" + assets[0].Name +
                        "' from the server. Keep in mind that this operation cannot be undone. Are you sure?",
                        "Remove it", "Cancel"))
            {
                if(_controller.PerfomBundleOperation(new List<Asset>() { assets[0] }, BundleOperation.remove_asset_bundles))
                {
                    BundlesWindow.OpenWindow();
                    if(BundlesWindow.Window.position.xMin == 0 && BundlesWindow.Window.position.yMin == 0)
                    {
                        EditorUtility.DisplayDialog("Transparent Bundles", "The selected assets will be removed shortly.", "Close");
                    }
                }
            }
        }

        [MenuItem("Assets/Bundles/Add Bundle into the Build")]
        public static void IntoBuild()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());
            if(assets.Count > 0 && _controller.PerfomBundleOperation(assets, BundleOperation.create_local_asset_bundles))
            {
                BundlesWindow.OpenWindow();
                if(BundlesWindow.Window.position.xMin == 0 && BundlesWindow.Window.position.yMin == 0)
                {
                    EditorUtility.DisplayDialog("Transparent Bundles", "The selected assets will be placed into the build shortly.", "Close");
                }
            }
        }

        [MenuItem("Assets/Bundles/Remove Bundle from the Build")]
        public static void OutsideBuild()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());
            if(assets.Count > 0 && _controller.PerfomBundleOperation(assets, BundleOperation.remove_local_asset_bundles))
            {
                BundlesWindow.OpenWindow();
                if(BundlesWindow.Window.position.xMin == 0 && BundlesWindow.Window.position.yMin == 0)
                {
                    EditorUtility.DisplayDialog("Transparent Bundles", "The selected assets will be removed from the build shortly.", "Close");
                }
            }
        }
    }
}
