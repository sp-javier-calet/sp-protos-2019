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
        const string BaseSettingsName = "Base Settings";
        static string CurrentMode;

        bool _dirty;
        Vector2 _scrollPosition = Vector2.right;

        Dictionary<string, BuildSetViewData> _buildSetData = null;
        BuildSetViewData _baseSettingsData;

        class BuildSetViewData
        {
            public bool Visible { get; set; }

            public string Name { get; private set; }

            public bool IsBase { get; private set; }

            public BuildSet Config { get; private set; }

            public BuildSetViewData(string name, BuildSet config, bool isBase)
            {
                Name = name;
                Config = config;
                Visible = false;
                IsBase = isBase;
            }
        }

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
            var buildSet = BuildSet.Load(configName);
            if(buildSet != null)
            {
                ApplyConfig(buildSet);
            }
            else
            {
                throw new FileNotFoundException(string.Format("BuildSet {0} not found", configName));
            }
        }

        static void ApplyConfig(BuildSet config)
        {
            config.Apply();
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
                    var bs = BuildSet.LoadByPath(path);
                    var assetName = Path.GetFileNameWithoutExtension(path);
                    var suffixIndex = assetName.IndexOf(BuildSet.FileSuffix);
                    if(suffixIndex > 0)
                    {
                        var configName = assetName.Substring(0, suffixIndex);
                        configs.Add(assetName, new BuildSetViewData(configName, bs, false));
                    }
                }
            }

            // Load base settings
            _baseSettingsData = new BuildSetViewData(BaseSettingsName, BaseSettings.Load(), true);

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
                EditorGUILayout.LabelField("Common", EditorStyles.boldLabel);
                config.CommonFlags = InheritableTextField("Flags", config.CommonFlags, data.IsBase);

                if(!data.IsBase)
                {
                    config.RebuildNativePlugins = EditorGUILayout.Toggle("Rebuild native plugins", config.RebuildNativePlugins);
                    config.OverrideIcon = EditorGUILayout.Toggle("Override Icon", config.OverrideIcon);
                }
                if(config.OverrideIcon || data.IsBase)
                {
                    config.Icon = (Texture2D)EditorGUILayout.ObjectField("Icon", config.Icon, typeof(Texture2D), false);
                }

                EditorGUILayout.Space();

                // IOS Condifguration
                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.IosBundleIdentifier = InheritableTextField("Bundle Identifier", config.IosBundleIdentifier, data.IsBase);
                config.IosFlags = InheritableTextField("Flags", config.IosFlags, data.IsBase);
                config.XcodeModsPrefixes = InheritableTextField("Xcodemods prefixes", config.XcodeModsPrefixes, data.IsBase);
                GUILayout.EndVertical();
                EditorGUILayout.Space();

                // Android Condifguration
                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.AndroidBundleIdentifier = InheritableTextField("Bundle Identifier", config.AndroidBundleIdentifier, data.IsBase);
                config.AndroidFlags = InheritableTextField("Flags", config.AndroidFlags, data.IsBase);

                if(!data.IsBase)
                {
                    config.ForceBundleVersionCode = EditorGUILayout.Toggle("Force Bundle Version Code", config.ForceBundleVersionCode);
                }
                if(config.ForceBundleVersionCode || data.IsBase)
                {
                    config.BundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", config.BundleVersionCode);
                }

                if(!data.IsBase)
                {
                    config.UseKeystore = EditorGUILayout.Toggle("Use release keystore", config.UseKeystore);
                }
                if(config.UseKeystore || data.IsBase)
                {
                    config.AndroidFlags = InheritableTextField("Keystore file", config.AndroidFlags, data.IsBase);
                    config.KeystoreFilePassword = InheritableTextField("Keystore password", config.KeystoreFilePassword, data.IsBase);
                    config.KeystoreAlias = InheritableTextField("Alias", config.KeystoreAlias, data.IsBase);
                    config.KeystorePassword = InheritableTextField("Password", config.KeystorePassword, data.IsBase);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();

                if(!data.IsBase)
                {
                    GUILayout.BeginHorizontal();
                    if(GUILayout.Button("Apply", Styles.ActionButtonOptions))
                    {
                        try
                        {
                            ApplyConfig(config);

                            EditorUtility.DisplayDialog("Config applied successfully", 
                                string.Format("{0} build set was applied successfully to Player Settings", data.Name), "Ok");
                        }
                        catch(Exception e)
                        {
                            EditorUtility.DisplayDialog("Error applying config", e.Message, "Ok");
                        }
                    }

                    if(GUILayout.Button("Delete", Styles.ActionButtonOptions))
                    {
                        if(config.Delete())
                        {
                            RefreshConfigs();
                        }
                    }
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        string InheritableTextField(string label, string value, bool isBaseConfig)
        {
            var newValue = value;
            if(!string.IsNullOrEmpty(value) || isBaseConfig)
            {
                newValue = EditorGUILayout.TextField(label, value);
            }
            else
            {
                var edit = EditorGUILayout.TextField(label, "<inherited>");
                if(edit != "<inherited>")
                {
                    newValue = edit;
                }
            }

            return newValue;
        }

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Read BuildSet definitions
            if(_buildSetData == null || _dirty)
            {
                _buildSetData = LoadViewConfig();
                _dirty = false;
            }

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            CreateConfigPanel(_baseSettingsData);

            GUILayout.Label("Build Sets", EditorStyles.boldLabel);

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
                RefreshConfigs();
            }
            if(GUILayout.Button("New Build Set"))
            {
                BuildSet.Create("NewConfig");
                RefreshConfigs();
            }

            EditorGUILayout.Space();
            EditorGUILayout.EndScrollView();
        }

        void RefreshConfigs()
        {
            _dirty = true;
        }

        #endregion
    }
}