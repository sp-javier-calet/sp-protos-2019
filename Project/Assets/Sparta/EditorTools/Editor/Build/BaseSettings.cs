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
        public override bool UseKeystore { get{ return true; } set { /* ignore */ } }

        public override bool Validate()
        {
            return true;
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
            AssetDatabase.CreateAsset(asset, BaseSettingsAsset);
            AssetDatabase.SaveAssets();
            return asset;
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