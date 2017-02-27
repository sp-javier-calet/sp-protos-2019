using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace SocialPoint.TransparentBundles
{
    public class Shortcuts
    {

        private static EditorClientController _controller;

        private static void Init()
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
            _controller.CreateOrUpdateBundles(assets);
            BundlesWindow.OpenWindow();
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
            else if(EditorUtility.DisplayDialog("Removing Bundle",
                         "You are about to remove the bundle of the asset '" + assets[0].Name +
                         "' from the server. Keep in mind that this operation cannot be undone. Are you sure?",
                         "Remove it", "Cancel"))
            {
                _controller.RemoveBundle(assets[0]);
            }
        }

        [MenuItem("Assets/Bundles/Add Bundle into the Build")]
        public static void IntoBuild()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());
            _controller.BundlesIntoBuild(assets);
        }

        [MenuItem("Assets/Bundles/Remove Bundle from the Build")]
        public static void OutsideBuild()
        {
            Init();
            var assets = new List<Asset>(_controller.GetAssetsFromSelection());
            _controller.BundlesOutsideBuild(assets);
        }
    }
}
