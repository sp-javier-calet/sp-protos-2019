using UnityEngine;
using System.IO;
using UnityEditor;

namespace SpartaTools.Editor.Build
{
    public class BuildSet : ScriptableObject
    {
        public const string ContainerPath = "Assets/Sparta/Config/BuildSet/";
        public const string FileSuffix = "-BuildSet";
        public const string FileExtension = ".asset";

        /* Common configuration */
        public string CommonFlags;
        public string BundleIdentifier;
        public bool RebuildNativePlugins;

        public bool OverrideIcon;
        public Texture2D Icon;

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


            string assetPath = AssetDatabase.GenerateUniqueAssetPath(ContainerPath + configName + FileSuffix + FileExtension);
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;

        }
    }
}