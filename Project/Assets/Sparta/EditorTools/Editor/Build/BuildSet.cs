using UnityEngine;
using UnityEditor;
using System;

namespace SpartaTools.Editor.Build
{
    public class BuildSet : ScriptableObject
    {
        public const string ContainerPath = "Assets/Sparta/Config/BuildSet/";
        public const string FileSuffix = "-BuildSet";
        public const string FileExtension = ".asset";

        /* Common configuration */
        public string CommonFlags;
        public bool RebuildNativePlugins;
        public virtual bool OverrideIcon { get; set; }
        public Texture2D Icon;

        /* iOS configuration */
        public string IosBundleIdentifier;
        public string IosFlags;
        public string XcodeModsPrefixes;
        public string[] IosRemovedResources;

        /* Android configuration */
        public string AndroidBundleIdentifier;
        public string AndroidFlags;
        public virtual bool ForceBundleVersionCode { get; set; }
        public int BundleVersionCode;
        public string[] AndroidRemovedResources;

        public virtual bool UseKeystore { get; set; }
        public string KeystorePath;
        public string KeystoreFilePassword;
        public string KeystoreAlias;
        public string KeystorePassword;


        public static string PathForConfigName(string configName)
        {
            return ContainerPath + configName + FileSuffix + FileExtension;
        }

        public virtual bool Validate()
        {
            return true;
        }

        public virtual void Apply()
        {
            var baseSettings = BaseSettings.Load();

            // Revert to base settings
            baseSettings.Apply();

            if(!Validate())
            {
                throw new InvalidOperationException(string.Format("Invalid configuration for '{0}'", name));
            }

            if(OverrideIcon)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] {
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon
                });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] {
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon,
                    Icon
                });
            }

            // Bundle Identifier
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS && !string.IsNullOrEmpty(IosBundleIdentifier))
            {
                PlayerSettings.bundleIdentifier = IosBundleIdentifier;
            }
            else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android && !string.IsNullOrEmpty(AndroidBundleIdentifier))
            {
                PlayerSettings.bundleIdentifier = AndroidBundleIdentifier;
            }

            // Flags
            var commonFlags = string.IsNullOrEmpty(CommonFlags)? baseSettings.CommonFlags : CommonFlags;
            var androidFlags = string.IsNullOrEmpty(AndroidFlags)? baseSettings.AndroidFlags : AndroidFlags;
            var iosFlags = string.IsNullOrEmpty(IosFlags)? baseSettings.IosFlags : IosFlags;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, commonFlags + ";" + androidFlags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, commonFlags + ";" + iosFlags);

            if(ForceBundleVersionCode)
            {
                PlayerSettings.Android.bundleVersionCode = BundleVersionCode;
            }

            // Android Keystore
            if(UseKeystore && !string.IsNullOrEmpty(KeystorePath))
            {       
                PlayerSettings.Android.keystoreName = KeystorePath;
                PlayerSettings.Android.keystorePass = KeystoreFilePassword;
                PlayerSettings.Android.keyaliasName = KeystoreAlias;
                PlayerSettings.Android.keyaliasPass = KeystorePassword;
            }
        }

        public bool Delete()
        {
            var assetPath = ContainerPath + name + FileExtension;
            return AssetDatabase.DeleteAsset(assetPath);
        }

        public static void Create(string configName)
        {
            if(!BaseSettings.Exists)
            {
                BaseSettings.Create();
            }

            var asset = ScriptableObject.CreateInstance(typeof(BuildSet));
            string assetPath = AssetDatabase.GenerateUniqueAssetPath(PathForConfigName(configName));
            AssetDatabase.CreateAsset(asset, assetPath);
            AssetDatabase.SaveAssets();
        }

        public static BuildSet Load(string configName)
        {
            return LoadByPath(PathForConfigName(configName));
        }

        public static BuildSet LoadByPath(string path)
        {
            return AssetDatabase.LoadAssetAtPath<BuildSet>(path);
        }
    }
}