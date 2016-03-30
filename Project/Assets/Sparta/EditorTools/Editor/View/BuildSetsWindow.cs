using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Build;

namespace SpartaTools.Editor.View
{
    public class BuildSetsWindow : EditorWindow
    {
        const string DebugConfigName = "Debug";
        const string ReleaseConfigName = "Release";
        const string BaseSettingsName = "Base Settings";
        const string InheritedLabel = "<inherited>";
        const string CurrentModeKey = "SpartaCurrentBuildSet";
        const string AutoApplyKey = "SpartaAutoApplyBuildSetEnabled";

        #region Static platform and buildset management

        static string _currentMode;
        static string CurrentMode
        {
            get
            {
                if(_currentMode == null)
                {
                    _currentMode = EditorPrefs.GetString(CurrentModeKey);
                }
                return _currentMode;
            }
            set
            {
                _currentMode = value;
                EditorPrefs.SetString(CurrentModeKey, value);
            }
        }

        static bool? _autoApply;
        static bool AutoApply
        {
            get
            {
                if(_autoApply == null)
                {
                    _autoApply = EditorPrefs.GetBool(AutoApplyKey);
                }
                return _autoApply.Value;
            }
            set
            {
                _autoApply = value;
                EditorPrefs.SetBool(AutoApplyKey, value);
            }
        }

        public BuildSetsWindow()
        {
            // Remove previous event if exists
            EditorUserBuildSettings.activeBuildTargetChanged -= OnTargetChanged;
            EditorUserBuildSettings.activeBuildTargetChanged += OnTargetChanged;
        }

        static void OnTargetChanged()
        {
            // Reapply the current config after change target platform
            var mode = CurrentMode;
            if(AutoApply && !string.IsNullOrEmpty(mode))
            {
                ApplyConfig(mode);
            }
        }

        #endregion

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
            EditorWindow.GetWindow(typeof(BuildSetsWindow), false, "Build Set", true);
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
            CurrentMode = config.Name;
            config.Apply();
        }

        #endregion

        #region Editor GUI

        void OnEnable()
        {
            titleContent = new GUIContent("Build Set", Sparta.Icon, "Sparta Build Set configurations");
        }

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

        void GUIConfigPanel(BuildSetViewData data)
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
                config.CommonFlags = InheritableTextField("Flags", "Defined symbols for all platforms", config.CommonFlags, data.IsBase);

                if(!data.IsBase)
                {
                    config.RebuildNativePlugins = EditorGUILayout.Toggle("Rebuild native plugins", config.RebuildNativePlugins);
                    config.OverrideIcon = EditorGUILayout.Toggle("Override Icon", config.OverrideIcon);
                }
                if(config.OverrideIcon)
                {
                    config.Icon = (Texture2D)EditorGUILayout.ObjectField("Icon", config.Icon, typeof(Texture2D), false);
                }

                EditorGUILayout.Space();

                // IOS Condifguration
                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.IosBundleIdentifier = InheritableTextField("Bundle Identifier", "iOS bundle identifier", config.IosBundleIdentifier, data.IsBase);
                config.IosFlags = InheritableTextField("Flags", "iOS specific defined symbols", config.IosFlags, data.IsBase);
                config.IosRemovedResources = InheritableTextField("Remove Resources", "Folders and file to remove before build. AutoBuilder only", config.IosRemovedResources, data.IsBase);
                config.XcodeModsPrefixes = InheritableTextField("Xcodemods prefixes", "Xcodemods prefixes to execute" , config.XcodeModsPrefixes, data.IsBase);
                GUILayout.EndVertical();
                EditorGUILayout.Space();

                // Android Condifguration
                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.AndroidBundleIdentifier = InheritableTextField("Bundle Identifier", "Android buindle identifier", config.AndroidBundleIdentifier, data.IsBase);
                config.AndroidFlags = InheritableTextField("Flags", "Android specific defined symbols", config.AndroidFlags, data.IsBase);
                config.AndroidRemovedResources = InheritableTextField("Remove Resources", "Folders and files to remove before build. AutoBuilder only", config.AndroidRemovedResources, data.IsBase);
                if(!data.IsBase)
                {
                    config.ForceBundleVersionCode = EditorGUILayout.Toggle("Force Bundle Version Code", config.ForceBundleVersionCode);
                }
                if(config.ForceBundleVersionCode)
                {
                    config.BundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", config.BundleVersionCode);
                }

                if(!data.IsBase)
                {
                    config.UseKeystore = EditorGUILayout.Toggle("Use release keystore", config.UseKeystore);
                }
                if(config.UseKeystore)
                {
                    config.KeystorePath = InheritableTextField("Keystore file", "Android keystore file path. Relative to project", config.KeystorePath, data.IsBase);
                    config.KeystoreFilePassword = InheritableTextField("Keystore password", "Keystore file password", config.KeystoreFilePassword, data.IsBase);
                    config.KeystoreAlias = InheritableTextField("Alias", "Keystore alias", config.KeystoreAlias, data.IsBase);
                    config.KeystorePassword = InheritableTextField("Password", "Keystore alias password", config.KeystorePassword, data.IsBase);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();


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
                if(!data.IsBase)
                {
                    if(GUILayout.Button("Delete", Styles.ActionButtonOptions))
                    {
                        if(config.Delete())
                        {
                            RefreshConfigs();
                        }
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }

            EditorGUILayout.Space();
            GUILayout.EndVertical();
        }

        string InheritableTextField(string label, string tooltip, string value, bool isBaseConfig)
        {
            var newValue = value;
            if(!string.IsNullOrEmpty(value) || isBaseConfig)
            {
                newValue = EditorGUILayout.TextField(new GUIContent(label, tooltip), value);
            }
            else
            {
                var edit = EditorGUILayout.TextField(new GUIContent(label, tooltip), InheritedLabel);
                if(edit != InheritedLabel)
                {
                    newValue = edit;
                }
            }

            return newValue;
        }

        void GUIToolBar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            var mode = CurrentMode;
            var currentAutoApply = AutoApply;

            if(string.IsNullOrEmpty(mode))
            {
                mode = "Custom";
            }

            GUILayout.Label(new GUIContent(mode, "Current Build Set"), EditorStyles.toolbarTextField);
           

            GUILayout.FlexibleSpace();

            // Common Buttons
            if(GUILayout.Button(new GUIContent("Refresh", "Refresh Build Set data from scriptable objects"), EditorStyles.toolbarButton))
            {
                RefreshConfigs();
            }
            if(GUILayout.Button(new GUIContent("Add Build Set", "Create a new Build Set"), EditorStyles.toolbarButton))
            {
                BuildSet.Create("NewConfig");
                RefreshConfigs();
            }

            EditorGUILayout.Space();

            var autoApply = GUILayout.Toggle(currentAutoApply, new GUIContent("Auto Apply", "Apply current configuration on platform switch"), EditorStyles.toolbarButton);
            if(autoApply != currentAutoApply)
            {
                AutoApply = autoApply;
            }

            GUILayout.EndHorizontal();
        }

        void OnGUI()
        {
            GUIToolBar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            // Read BuildSet definitions
            if(_buildSetData == null || _dirty)
            {
                _buildSetData = LoadViewConfig();
                _dirty = false;
            }

            GUILayout.Label("Base Settings", EditorStyles.boldLabel);

            GUIConfigPanel(_baseSettingsData);

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
                    GUIConfigPanel(config);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        void RefreshConfigs()
        {
            _dirty = true;
        }

        #endregion
    }
}