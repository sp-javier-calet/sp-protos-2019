using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

//FIXME
using SpartaTools.Editor.Sync.View;

namespace SpartaTools.Editor.Build
{
    public class BuildConfig : EditorWindow
    {
        const string DebugConfigName = "Debug";
        const string ReleaseConfigName = "Release";
        static string CurrentMode;

        Vector2 _scrollPosition = Vector2.right;
        Dictionary<string, BuildSetViewData> _buildSetData = null;

        class BuildSetViewData
        {
            public bool Visible { get; set; }

            public string Name { get; private set; }

            public BuildSet Config { get; private set; }

            public BuildSetViewData(string name, BuildSet config)
            {
                Name = name;
                Config = config;
                Visible = false;
            }
        }

        static readonly BuildTargetGroup[] AvailableTargets = { BuildTargetGroup.Android, BuildTargetGroup.iOS };

        #region Editor options

        [MenuItem("Sparta/Build/Debug", false, 1)]
        public static void SetDebugConfig()
        {
            CurrentMode = DebugConfigName;
            ApplyConfig(CurrentMode);
        }

        [MenuItem("Sparta/Build/Debug", true)]
        static bool ValidateSetDebugConfig()
        {
            return CurrentMode != DebugConfigName;
        }

        [MenuItem("Sparta/Build/Release", false, 2)]
        public static void SetReleaseConfig()
        {
            CurrentMode = ReleaseConfigName;
            ApplyConfig(CurrentMode);
        }

        [MenuItem("Sparta/Build/Release", true)]
        static bool ValidateSetReleaseConfig()
        {
            return CurrentMode != ReleaseConfigName;
        }

        [MenuItem("Sparta/Build/Build settings...", false, 3)]
        public static void ShowBuildSettings()
        {
            EditorWindow.GetWindow(typeof(BuildConfig), false, "Sparta BuildSet", true);
        }

        #endregion

        #region Static functions

        static void ApplyConfig(string configName)
        {
            var configPath = BuildSet.ContainerPath + configName + BuildSet.FileSuffix + BuildSet.FileExtension;
            var buildSet = AssetDatabase.LoadAssetAtPath<BuildSet>(configPath);
            if(buildSet != null)
            {
                ApplyConfig(buildSet);
            }
        }

        static void ApplyConfig(BuildSet config)
        {
            if(config.OverrideIcon)
            {
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Android, new Texture2D[] {
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon
                });
                PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.iOS, new Texture2D[] {
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon,
                    config.Icon
                });
            }

            // Bundle
            PlayerSettings.bundleIdentifier = config.BundleIdentifier;

            // Flags
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, config.CommonFlags + ";" + config.AndroidFlags);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.iOS, config.CommonFlags + ";" + config.IosFlags);

            if(config.ForceBundleVersionCode)
            {
                PlayerSettings.Android.bundleVersionCode = config.BundleVersionCode;
            }

            // Android Keystore
            PlayerSettings.Android.keystoreName = config.KeystorePath;
            PlayerSettings.Android.keystorePass = config.KeystoreFilePassword;
            PlayerSettings.Android.keyaliasName = config.KeystoreAlias;
            PlayerSettings.Android.keyaliasPass = config.KeystorePassword;


        }

        #endregion

        #region Editor GUI

        Dictionary<string, BuildSetViewData> LoadViewConfig()
        {
            var configs = new Dictionary<string, BuildSetViewData>();
            foreach(var path in Directory.GetFiles(BuildSet.ContainerPath))
            {
                if(path.EndsWith(BuildSet.FileExtension))
                {
                    var bs = AssetDatabase.LoadAssetAtPath<BuildSet>(path);
                    var assetName = Path.GetFileNameWithoutExtension(path);
                    var suffixIndex = assetName.IndexOf(BuildSet.FileSuffix);
                    if(suffixIndex > 0)
                    {
                        var configName = assetName.Substring(0, suffixIndex);
                        configs.Add(assetName, new BuildSetViewData(configName, bs));
                    }
                }
            }

            return configs;
        }

        void CreateConfigPanel(BuildSetViewData data)
        {
            var config = data.Config;
            GUILayout.BeginVertical();

            data.Visible = EditorGUILayout.Foldout(data.Visible, data.Name);
            if(data.Visible)
            {
                if(!config.Validate())
                {
                    GUILayout.Label("Invalid BuildSet", Styles.InvalidProjectLabel);
                }

                GUILayout.BeginVertical(Styles.Group);
                config.CommonFlags = EditorGUILayout.TextField("Common Flags", config.CommonFlags);
                config.RebuildNativePlugins = EditorGUILayout.Toggle("Rebuild native plugins", config.RebuildNativePlugins);
                config.BundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", config.BundleIdentifier);
                config.OverrideIcon = EditorGUILayout.Toggle("Override Icon", config.OverrideIcon);
                if(config.OverrideIcon)
                {
                    config.Icon = (Texture2D)EditorGUILayout.ObjectField("Icon", config.Icon, typeof(Texture2D), false);
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.IosFlags = EditorGUILayout.TextField("Flags", config.IosFlags);
                config.XcodeModsPrefixes = EditorGUILayout.TextField("Xcodemods prefixes", config.XcodeModsPrefixes);
                GUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.AndroidFlags = EditorGUILayout.TextField("Flags", config.AndroidFlags);

                config.ForceBundleVersionCode = EditorGUILayout.Toggle("Force Bundle Version Code", config.ForceBundleVersionCode);
                if(config.ForceBundleVersionCode)
                {
                    config.BundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", config.BundleVersionCode);
                }

                config.UseKeytore = EditorGUILayout.Toggle("Use release keystore", config.UseKeytore);
                if(config.UseKeytore)
                {
                    config.KeystorePath = EditorGUILayout.TextField("Keystore file", config.KeystorePath);
                    config.KeystoreFilePassword = EditorGUILayout.TextField("Keystore password", config.KeystoreFilePassword);
                    config.KeystoreAlias = EditorGUILayout.TextField("Alias", config.KeystoreAlias);
                    config.KeystorePassword = EditorGUILayout.TextField("Password", config.KeystorePassword);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Apply", Styles.ActionButtonOptions))
                {
                    ApplyConfig(config);
                }

                if(GUILayout.Button("Delete", Styles.ActionButtonOptions))
                {
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Build Sets", EditorStyles.boldLabel);

            // Read BuildSet definitions
            if(_buildSetData == null)
            {
                _buildSetData = LoadViewConfig();
            }

            // Inflate config panels
            if(_buildSetData.Count == 0)
            {
                EditorGUILayout.LabelField("No build sets defined");
            }
            else
            {
                foreach(var config in _buildSetData.Values)
                {
                    CreateConfigPanel(config);
                }
            }

            // Common Buttons
            if(GUILayout.Button("Refresh"))
            {
                _buildSetData = LoadViewConfig();
            }
            if(GUILayout.Button("New Build Set"))
            {
                BuildSet.CreateBuildSet("NewConfig");
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        #endregion
    }
}