﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;

namespace SpartaTools.Editor.Build
{
    class BaseSettings : BuildSet
    {
        public const string BaseSettingsAsset = ContainerPath + "BaseSettings" + FileExtension;

        readonly List<Validator> _validators = new List<Validator> { 
            new Validator {
                Validate = (BuildSet bs) => !string.IsNullOrEmpty(bs.Android.BundleIdentifier),
                ErrorMessage = "Android Bundle Identifier must be defined"
            },
            new Validator {
                Validate = (BuildSet bs) => !string.IsNullOrEmpty(bs.Ios.BundleIdentifier),
                ErrorMessage = "Ios Bundle Identifier must be defined"
            },
            new Validator {
                Validate = (BuildSet bs) => bs.App.IconTexture != null,
                ErrorMessage = "Default Icon must be defined"
            },
            new Validator {
                Validate = (BuildSet bs) => !string.IsNullOrEmpty(bs.App.ProductName),
                ErrorMessage = "Default Product Name must be provided"
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
            var asset = ScriptableObject.CreateInstance(typeof(BaseSettings)) as BaseSettings;
            ImportConfig(asset);
            AssetDatabase.CreateAsset(asset, BaseSettingsAsset);
            AssetDatabase.SaveAssets();
            return asset;
        }

        static void ImportConfig(BaseSettings config)
        {
            config.App.ProductName = PlayerSettings.productName;

            var icons = PlayerSettings.GetIconsForTargetGroup(BuildTargetGroup.iOS);
            if(icons != null && icons.Length > 0)
            {
                config.App.IconTexture = icons[0];
            }

            config.Ios.BundleIdentifier = PlayerSettings.bundleIdentifier;
            config.Ios.Flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS);

            config.Android.BundleIdentifier = PlayerSettings.bundleIdentifier;
            config.Android.Flags = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android);
            config.Android.BundleVersionCode = PlayerSettings.Android.bundleVersionCode;
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
            var baseSettings = AssetDatabase.LoadAssetAtPath<BaseSettings>(BaseSettingsAsset);
            if(baseSettings == null)
            {
                baseSettings = Create();
            }
            return baseSettings;
        }

        public override void Apply()
        {
            Validate();

            PlayerSettings.productName = App.ProductName;

            // Always override Icon
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] {
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture
            });
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] {
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture,
                App.IconTexture
            });
                
            // Bundle Identifier
            if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.iOS)
            {
                PlayerSettings.bundleIdentifier = Ios.BundleIdentifier;
            }
            else if(EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                PlayerSettings.bundleIdentifier = Android.BundleIdentifier;
            }

            // Flags
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, Common.Flags + ";" + Android.Flags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, Common.Flags + ";" + Ios.Flags);
            PlayerSettings.Android.bundleVersionCode = Android.BundleVersionCode;

            // Android Keystore
            PlayerSettings.Android.keystoreName = Android.Keystore.Path;
            PlayerSettings.Android.keystorePass = Android.Keystore.FilePassword;
            PlayerSettings.Android.keyaliasName = Android.Keystore.Alias;
            PlayerSettings.Android.keyaliasPass = Android.Keystore.Password;
        }
    }
}