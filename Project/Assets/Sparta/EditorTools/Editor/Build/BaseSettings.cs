using UnityEngine;
using UnityEditor;
using System.IO;

namespace SpartaTools.Editor.Build
{
    class BaseSettings : BuildSet
    {
        public const string BaseSettingsAsset = ContainerPath + "BaseSettings" + FileExtension;

        public override bool OverrideIcon { get { return true; } set { /* ignore */ } }

        public override bool ForceBundleVersionCode { get { return true; } set { /* ignore */ } }

        public override bool UseKeystore { get { return true; } set { /* ignore */ } }

        public override bool Validate()
        {
            return  !string.IsNullOrEmpty(AndroidBundleIdentifier) &&
            !string.IsNullOrEmpty(IosBundleIdentifier) &&
            Icon != null;
        }

        public static bool Exists
        {
            get
            {
                return File.Exists(BaseSettingsAsset);
            }
        }

        public static BaseSettings Create()
        {
            var asset = ScriptableObject.CreateInstance(typeof(BaseSettings)) as BaseSettings;
            ImportConfig(asset);
            AssetDatabase.CreateAsset(asset, BaseSettingsAsset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static void ImportConfig(BaseSettings config)
        {
            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS);
            if(icons != null && icons.Length > 0)
            {
                config.Icon = icons[0];
            }
            
            config.IosBundleIdentifier = PlayerSettings.bundleIdentifier;
            config.IosFlags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

            config.AndroidBundleIdentifier = PlayerSettings.bundleIdentifier;
            config.AndroidFlags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            config.BundleVersionCode = PlayerSettings.Android.bundleVersionCode;
            config.KeystorePath = PlayerSettings.Android.keystoreName;
            config.KeystoreFilePassword = PlayerSettings.keystorePass;
            config.KeystoreAlias = PlayerSettings.Android.keyaliasName;
            config.KeystorePassword = PlayerSettings.Android.keyaliasPass;
        }

        public static void RevertToBase()
        {
            Load().Apply();
        }

        public static BaseSettings Load()
        {
            var baseSettings = AssetDatabase.LoadAssetAtPath<BaseSettings>(BaseSettingsAsset);
            if(baseSettings == null)
            {
                baseSettings = Create();
            }
            return baseSettings;
        }

        public override void Apply()
        {
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
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                PlayerSettings.bundleIdentifier = IosBundleIdentifier;
            }
            else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.bundleIdentifier = AndroidBundleIdentifier;
            }

            // Flags
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, CommonFlags + ";" + AndroidFlags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, CommonFlags + ";" + IosFlags);
            PlayerSettings.Android.bundleVersionCode = BundleVersionCode;

            // Android Keystore
            PlayerSettings.Android.keystoreName = KeystorePath;
            PlayerSettings.Android.keystorePass = KeystoreFilePassword;
            PlayerSettings.Android.keyaliasName = KeystoreAlias;
            PlayerSettings.Android.keyaliasPass = KeystorePassword;
        }
    }
}