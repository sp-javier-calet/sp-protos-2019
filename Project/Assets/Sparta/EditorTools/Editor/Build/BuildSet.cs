using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

namespace SpartaTools.Editor.Build
{
    public class BuildSet : ScriptableObject
    {
        public const string DebugConfigName = "Debug";
        public const string ReleaseConfigName = "Release";
        public const string ShippingConfigName = "Shipping";
        public const string BaseSettingsName = "Base Settings";
        public const string DebugScenePrefix = "Debug";
        public const string XcodeModSchemesPrefsKey = "XCodeModSchemes";

        public const string ContainerPath = "Assets/Sparta/Config/BuildSet/";
        public const string FileSuffix = "-BuildSet";
        public const string FileExtension = ".asset";

        static readonly char[] ListSeparator = { ';' };

        #region Scriptable Object Data

        /* 
         * Icon configuration 
         */
        [Serializable]
        public struct AppConfiguration
        {
            public string ProductName;
            public Texture2D IconTexture;
            public bool OverrideIcon;
        }

        public AppConfiguration App;

        /* 
         * Common configuration 
         */
        [Serializable]
        public struct CommonConfiguration
        {
            public string Flags;
            public bool RebuildNativePlugins;
            public bool IsDevelopmentBuild;
            public bool IncludeDebugScenes;
        }

        public CommonConfiguration Common;

        /*
         * iOS configuration 
         */
        [Serializable]
        public struct IosConfiguration
        {
            public string BundleIdentifier;
            public string Flags;
            public string XcodeModSchemes;
            public string RemovedResources;
        }

        public IosConfiguration Ios;

        /* 
         * Android configuration 
         */
        [Serializable]
        public struct AndroidKeystoreConfiguration
        {
            public string Path;
            public string FilePassword;
            public string Alias;
            public string Password;
        }

        [Serializable]
        public struct AndroidConfiguration
        {
            public string BundleIdentifier;
            public int BundleVersionCode;
            public bool ForceBundleVersionCode;
            public string Flags;
            public string RemovedResources;
            public bool UseKeystore;
            public AndroidKeystoreConfiguration Keystore;
        }

        public AndroidConfiguration Android;


        public string Name
        {
            get
            {
                var nameLength = name.IndexOf(FileSuffix);
                return nameLength > 0 ? name.Substring(0, nameLength) : name;
            }
        }

        public static string PathForConfigName(string configName)
        {
            return ContainerPath + configName + FileSuffix + FileExtension;
        }

        public bool IsDefaultConfig
        {
            get
            {
                return IsDebugConfig || IsReleaseConfig || IsShippingConfig;
            }
        }

        public bool IsDebugConfig
        {
            get
            {
                return Name.Equals(DebugConfigName);
            }
        }

        public bool IsReleaseConfig
        {
            get
            {
                return Name.Equals(ReleaseConfigName);
            }
        }

        public bool IsShippingConfig
        {
            get
            {
                return Name.Equals(ShippingConfigName);
            }
        }

        #endregion

        #region Validation

        protected delegate bool ValidatorDelegate(BuildSet bs);

        protected struct Validator
        {
            public ValidatorDelegate Validate;

            public string ErrorMessage;
        }

        readonly List<Validator> _validators = new List<Validator> { 
            new Validator {
                Validate = (BuildSet bs) => !bs.IsDebugConfig || bs.Ios.XcodeModSchemes.Contains("debug"),
                ErrorMessage = "Debug Build Set must define the 'debug' scheme for XcodeMods"
            },
            new Validator {
                Validate = (BuildSet bs) => !bs.IsReleaseConfig || bs.Ios.XcodeModSchemes.Contains("release"),
                ErrorMessage = "Release Build Set must define the 'release' scheme for XcodeMods"
            },
            new Validator {
                Validate = (BuildSet bs) => !bs.IsShippingConfig || bs.Ios.XcodeModSchemes.Contains("shipping"),
                ErrorMessage = "Shipping Build Set must define the 'shipping' scheme for XcodeMods"
            },
            new Validator {
                Validate = (BuildSet bs) => !bs.IsShippingConfig || bs.Android.UseKeystore,
                ErrorMessage = "Shipping Build Set must use a release keystore"
            },
            new Validator {
                Validate = (BuildSet bs) => !bs.IsShippingConfig || !bs.Common.IsDevelopmentBuild,
                ErrorMessage = "Shipping Build Set cannot be set as a Development Build"
            },
            new Validator {
                Validate = (BuildSet bs) => !bs.IsShippingConfig || !bs.Android.ForceBundleVersionCode,
                ErrorMessage = "Shipping Build Set cannot force bundle version code"
            }
        };

        protected virtual List<Validator> Validators
        {
            get
            {
                return _validators;
            }
        }

        public bool IsValid(out string error)
        {
            bool validBuildSet = true;
            error = string.Empty;

            if(Validators != null)
            {
                var errorBuilder = new StringBuilder();
                foreach(var v in Validators)
                {
                    var valid = v.Validate(this);
                    validBuildSet &= valid;
                    if(!valid)
                    {
                        errorBuilder.AppendLine(v.ErrorMessage);
                    }
                }

                error = errorBuilder.ToString();
            }
            return validBuildSet;
        }

        public void Validate()
        {
            string error;
            if(!IsValid(out error))
            {
                throw new InvalidOperationException(string.Format("Invalid configuration for '{0}'. \n{1}", name, error));
            }
        }

        protected void SetXcodeModSchemes(string schemes)
        {
            if(string.IsNullOrEmpty(schemes))
            {
                EditorPrefs.DeleteKey(XcodeModSchemesPrefsKey);
            }
            else
            {
                EditorPrefs.SetString(XcodeModSchemesPrefsKey, schemes);
            }
        }

        #endregion

        public void SelectScenes(bool includeDebug)
        {
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            foreach(var scene in scenes)
            {
                if(Path.GetFileName(scene.path).StartsWith(DebugScenePrefix))
                {
                    scene.enabled = includeDebug;
                }
            }
            EditorBuildSettings.scenes = scenes;
        }

        public virtual void Apply()
        {
            var baseSettings = BaseSettings.Load();

            // Revert to base settings
            baseSettings.Apply();

            Validate();

            if(!string.IsNullOrEmpty(App.ProductName))
            {
                PlayerSettings.productName = App.ProductName;
            }

            if(App.OverrideIcon)
            {
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
            }

            SelectScenes(Common.IncludeDebugScenes);

            /* 
             * Per Platform Flags
             */
            var commonFlags = string.IsNullOrEmpty(Common.Flags) ? baseSettings.Common.Flags : Common.Flags;
            var androidFlags = string.IsNullOrEmpty(Android.Flags) ? baseSettings.Android.Flags : Android.Flags;
            var iosFlags = string.IsNullOrEmpty(Ios.Flags) ? baseSettings.Ios.Flags : Ios.Flags;

            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, commonFlags + ";" + androidFlags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, commonFlags + ";" + iosFlags);

            /*
             * Android-only configuration
             */
            if(Android.ForceBundleVersionCode)
            {
                PlayerSettings.Android.bundleVersionCode = Android.BundleVersionCode;
            }

            // Android Keystore
            if(Android.UseKeystore && !string.IsNullOrEmpty(Android.Keystore.Path))
            {       
                PlayerSettings.Android.keystoreName = Android.Keystore.Path;
                PlayerSettings.Android.keystorePass = Android.Keystore.FilePassword;
                PlayerSettings.Android.keyaliasName = Android.Keystore.Alias;
                PlayerSettings.Android.keyaliasPass = Android.Keystore.Password;
            }
            else
            {
                PlayerSettings.Android.keystoreName = string.Empty;
                PlayerSettings.Android.keystorePass = string.Empty;
                PlayerSettings.Android.keyaliasName = string.Empty;
                PlayerSettings.Android.keyaliasPass = string.Empty;
            }

            /*
             * Editor build settings
             */
            EditorUserBuildSettings.development = Common.IsDevelopmentBuild;

            /*
             * Set XcodeMods custom prefixes
             */
            SetXcodeModSchemes(Ios.XcodeModSchemes);

            /*
             * Override shared configuration for the active target platform
             */
            Platform.OnApply(this);
        }

        public void ApplyExtended()
        {
            Apply();
            Platform.OnApplyExtended(this);
        }

        public BuildOptions Options
        {
            get
            {
                var options = BuildOptions.None;

                if(Common.IsDevelopmentBuild)
                {
                    options |= BuildOptions.Development;
                }

                return options;
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

        #region Platform Processors

        static readonly Dictionary<BuildTarget, PlatformProcessor> PlatformProcessors = new  Dictionary<BuildTarget, PlatformProcessor> {
            { BuildTarget.Android, new AndroidPlatformProcessor() },
            { BuildTarget.iOS, new IosPlatformProcessor() },
            { BuildTarget.tvOS, new TvosPlatformProcessor() }
        };

        static PlatformProcessor Platform
        {
            get
            {
                var processor = PlatformProcessors[EditorUserBuildSettings.activeBuildTarget];

                if(processor == null)
                {
                    processor = new PlatformProcessor();
                }

                return processor;
            }
        }

        class PlatformProcessor
        {
            public virtual void OnApply(BuildSet buildSet)
            {
            }

            public virtual void OnApplyExtended(BuildSet buildSet)
            {
            }

            protected void SetBundleIdentifier(string bundleIdentifier)
            {
                if(!string.IsNullOrEmpty(bundleIdentifier))
                {
                    PlayerSettings.bundleIdentifier = bundleIdentifier;
                }
            }

            protected void DiscardResources(string resources)
            {
                if(string.IsNullOrEmpty(resources))
                {
                    return;
                }

                string[] resourceList = resources.Split(ListSeparator);
                foreach(var r in resourceList)
                {
                    var resPath = Path.Combine(Application.dataPath, r);
                    bool result = FileUtil.DeleteFileOrDirectory(resPath);

                    if(result)
                    {
                        Debug.Log("Removed resources in " + resPath);
                    }
                    else
                    {
                        Debug.LogWarning("Could not remove resources in " + resPath);
                    }
                }
            }
        }

        class AndroidPlatformProcessor : PlatformProcessor
        {
            public override void OnApply(BuildSet buildSet)
            {
                SetBundleIdentifier(buildSet.Android.BundleIdentifier);
            }

            public override void OnApplyExtended(BuildSet buildSet)
            {
                DiscardResources(buildSet.Android.RemovedResources);

                if(buildSet.Common.RebuildNativePlugins)
                {
                    NativeBuild.CompileAndroid();
                    NativeBuild.CompileAndroidNative();
                }
            }
        }

        class IosPlatformProcessor : PlatformProcessor
        {
            public override void OnApply(BuildSet buildSet)
            {
                SetBundleIdentifier(buildSet.Ios.BundleIdentifier);
            }

            public override void OnApplyExtended(BuildSet buildSet)
            {
                DiscardResources(buildSet.Ios.RemovedResources);

                if(buildSet.Common.RebuildNativePlugins)
                {
                    NativeBuild.CompileIOS();
                }
            }
        }

        class TvosPlatformProcessor : PlatformProcessor
        {
            public override void OnApply(BuildSet buildSet)
            {
                SetBundleIdentifier(buildSet.Ios.BundleIdentifier);
            }

            public override void OnApplyExtended(BuildSet buildSet)
            {
                DiscardResources(buildSet.Ios.RemovedResources);

                if(buildSet.Common.RebuildNativePlugins)
                {
                    NativeBuild.CompileTVOS();
                }
            }
        }

        #endregion
    }
}