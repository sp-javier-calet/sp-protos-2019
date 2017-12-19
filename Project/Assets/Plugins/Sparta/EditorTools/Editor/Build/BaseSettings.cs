using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace SpartaTools.Editor.Build
{
    class BaseSettings : BuildSet
    {
        public const string BaseSettingsAsset = ContainerPath + "BaseSettings" + FileExtension;

        readonly List<Validator> _validators = new List<Validator> { 
            new Validator {
                Validate = bs => !string.IsNullOrEmpty(bs.Android.BundleIdentifier),
                ErrorMessage = "Android Bundle Identifier must be defined"
            },
            new Validator {
                Validate = bs => !string.IsNullOrEmpty(bs.Ios.BundleIdentifier),
                ErrorMessage = "Ios Bundle Identifier must be defined"
            },
            new Validator {
                Validate = bs => bs.App.IconTexture != null,
                ErrorMessage = "Default Icon must be defined"
            },
            new Validator {
                Validate = bs => !string.IsNullOrEmpty(bs.App.ProductName),
                ErrorMessage = "Default Product Name must be provided"
            },
            new Validator {
                Validate = bs => !string.IsNullOrEmpty(bs.Android.Keystore.Path) &&
                !string.IsNullOrEmpty(bs.Android.Keystore.FilePassword) &&
                !string.IsNullOrEmpty(bs.Android.Keystore.Alias) &&
                !string.IsNullOrEmpty(bs.Android.Keystore.Password),
                ErrorMessage = "A Default Release Keystore data must be provided"
            }
        };

        protected override List<Validator> Validators
        {
            get
            {
                return _validators;
            }
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
            var asset = ScriptableObject.CreateInstance<BaseSettings>();

            if(!Directory.Exists(ContainerPath))
            {
                Directory.CreateDirectory(ContainerPath);
            }

            ImportConfig(asset);
            AssetDatabase.CreateAsset(asset, BaseSettingsAsset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static void ImportConfig(BaseSettings config)
        {
            config.App.ProductName = PlayerSettings.productName;
            config.App.Version = PlayerSettings.bundleVersion;

            var buildNumber = 1;
            try
            {
                buildNumber = int.Parse(PlayerSettings.iOS.buildNumber);
            }
            catch(Exception)
            {
            }
            config.App.BuildNumber = buildNumber;

            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS);
            if(icons != null && icons.Length > 0)
            {
                config.App.IconTexture = icons[0];
            }

            config.Ios.BundleIdentifier = PlayerSettings.bundleIdentifier;
            config.Ios.Flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

            config.Android.BundleIdentifier = PlayerSettings.bundleIdentifier;
            config.Android.Flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            config.Android.Keystore.Path = PlayerSettings.Android.keystoreName;
            config.Android.Keystore.FilePassword = PlayerSettings.keystorePass;
            config.Android.Keystore.Alias = PlayerSettings.Android.keyaliasName;
            config.Android.Keystore.Password = PlayerSettings.Android.keyaliasPass;

            EditorUtility.SetDirty(config);
        }

        public static void RevertToBase()
        {
            Load().Apply();
        }

        public static BaseSettings Load()
        {
            var baseSettings = AssetDatabase.LoadAssetAtPath<BaseSettings>(BaseSettingsAsset) ?? Create();
            return baseSettings;
        }

        public override void Apply()
        {
            Validate();

            PlayerSettings.productName = App.ProductName;
            PlayerSettings.bundleVersion = App.Version;
            PlayerSettings.Android.bundleVersionCode = App.BuildNumber;
            PlayerSettings.iOS.buildNumber = App.BuildNumber.ToString();

            // Always override Icon
            SetIcon(App.IconTexture);
                
            // Bundle Identifier
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                PlayerSettings.bundleIdentifier = Ios.BundleIdentifier;
            }
            else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.bundleIdentifier = Android.BundleIdentifier;
            }
                
            // Due to plugins constraints, we need to compile always using Gradle
            EditorUserBuildSettings.exportAsGoogleAndroidProject = false;
            EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;
            EditorUserBuildSettings.forceInstallation = false;

            // Flags
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, Common.Flags + ";" + Android.Flags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, Common.Flags + ";" + Ios.Flags);

            // Android Keystore
            PlayerSettings.Android.keystoreName = Android.Keystore.Path;
            PlayerSettings.Android.keystorePass = Android.Keystore.FilePassword;
            PlayerSettings.Android.keyaliasName = Android.Keystore.Alias;
            PlayerSettings.Android.keyaliasPass = Android.Keystore.Password;

            SetXcodeModSchemes(Ios.XcodeModSchemes);
        }
    }
}