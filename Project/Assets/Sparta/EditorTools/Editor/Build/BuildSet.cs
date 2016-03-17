using UnityEngine;
using System.IO;
using UnityEditor;

namespace SpartaTools.Editor.Build
{
    public class BuildSet : ScriptableObject
    {
        public const string BuildSetPath = "Assets/Sparta/Config/BuildSet/";

        /* Common configuration */
        public string CommonFlags;
        public string BundleIdentifier;
        public bool RebuildNativePlugins;

        /* iOS configuration */
        public string IosFlags;
        public string XcodeModsPrefixes;

        /* Android configuration */
        public string AndroidFlags;
        public bool ForceBundleVersionCode;
        public int BundleVersionCode;

        public bool UseKeytore;
        public string KeystorePath;
        public string KeystoreFilePassword;
        public string KeystoreAlias;
        public string KeystorePassword;


        public bool Validate()
        {
            return !string.IsNullOrEmpty(BundleIdentifier);
        }

        public static void CreateBuildSet(string configName)
        {
            System.Type type = typeof(BuildSet);
            var asset = ScriptableObject.CreateInstance(type);
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if(path == "")
            {
                path = "Assets";
            }
            else if(Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(BuildSetPath + configName + ".asset");
            AssetDatabase.CreateAsset(asset, assetPathAndName);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }
    }
}