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

        public const string ContainerPath = "Assets/Sparta/Config/BuildSet/";
        public const string FileSuffix = "-BuildSet";
        public const string FileExtension = ".asset";

        static readonly char[] ListSeparator = { ';' };

        #region Scriptable Object Data

        /* 
         * Icon configuration 
         */
        [Serializable]
        public struct IconConfiguration
        {
            public Texture2D Texture;
            public bool Override;
        }

        public IconConfiguration Icon;

        /* 
         * Common configuration 
         */
        [Serializable]
        public struct CommonConfiguration
        {
            public string Flags;
            public bool RebuildNativePlugins;
            public bool IsDevelopmentBuild;
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
            public string XcodeModsPrefixes;
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
                return name.Substring(0, name.IndexOf(FileSuffix));
            }
        }

        public static string PathForConfigName(string configName)
        {
            return ContainerPath + configName + FileSuffix + FileExtension;
        }

        #endregion

        #region Validation

        protected delegate bool ValidatorDelegate(BuildSet bs);

        protected struct Validator
        {
            public ValidatorDelegate Validate;

            public string ErrorMessage;
        }

        protected virtual List<Validator> Validators
        {
            get
            {
                return null;
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

        #endregion

        public virtual void Apply()
        {
            var baseSettings = BaseSettings.Load();

            // Revert to base settings
            baseSettings.Apply();

            Validate();

            if(Icon.Override)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] {
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture
                });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] {
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture,
                    Icon.Texture
                });
            }

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