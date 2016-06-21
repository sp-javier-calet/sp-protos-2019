﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Build;

namespace SpartaTools.Editor.View
{
    public class BuildSetsWindow : EditorWindow
    {
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

        bool _editEnabled;

        bool EditEnabled
        {
            set
            {
                bool changed = _editEnabled != value;
                _editEnabled = value;
                if(changed)
                {
                    RefreshIcon();
                }
            }
            get
            {
                return _editEnabled;
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
            ApplyConfig(BuildSet.DebugConfigName);
        }

        [MenuItem("Sparta/Build/Debug", true)]
        static bool ValidateSetDebugConfig()
        {
            return CurrentMode != BuildSet.DebugConfigName;
        }

        [MenuItem("Sparta/Build/Release", false, 2)]
        public static void SetReleaseConfig()
        {
            ApplyConfig(BuildSet.ReleaseConfigName);
        }

        [MenuItem("Sparta/Build/Release", true)]
        static bool ValidateSetReleaseConfig()
        {
            return CurrentMode != BuildSet.ReleaseConfigName;
        }

        [MenuItem("Sparta/Build/Shipping", false, 2)]
        public static void SetShippingConfig()
        {
            ApplyConfig(BuildSet.ShippingConfigName);
        }

        [MenuItem("Sparta/Build/Shipping", true)]
        static bool ValidateSetShippingConfig()
        {
            return CurrentMode != BuildSet.ShippingConfigName;
        }

        [MenuItem("Sparta/Build/Build Set...", false, 3)]
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

        static void ApplyConfig(BuildSet config, bool extended = false)
        {
            CurrentMode = config.Name;
            if(extended)
            {
                config.ApplyExtended();
            }
            else
            {
                config.Apply();
            }
        }

        #endregion

        #region Editor GUI

        void OnFocus()
        {
            RefreshIcon();
        }

        void RefreshIcon()
        {
            Sparta.SetIcon(this, "Build Set", "Sparta Build Set configurations", EditEnabled);
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
            _baseSettingsData = new BuildSetViewData(BuildSet.BaseSettingsName, BaseSettings.Load(), true);

            return configs;
        }

        void GUIConfigPanel(BuildSetViewData data)
        {
            var config = data.Config;
            GUILayout.BeginVertical();

            data.Visible = EditorGUILayout.Foldout(data.Visible, data.Name);
            if(data.Visible)
            {
                string error;
                if(!config.IsValid(out error))
                {
                    GUILayout.Label(new GUIContent("Invalid BuildSet", error), Styles.InvalidProjectLabel);
                }

                GUILayout.BeginVertical(Styles.Group);
                EditorGUILayout.LabelField("Common", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                GUILayout.BeginVertical();
                config.Common.Flags = InheritableTextField("Flags", "Defined symbols for all platforms", config.Common.Flags, data.IsBase);

                if(!data.IsBase)
                {
                    config.Common.RebuildNativePlugins = EditorGUILayout.Toggle(new GUIContent("Rebuild native plugins", "Extended Feature. Build platform plugins before build player"), config.Common.RebuildNativePlugins);
                    config.Common.IsDevelopmentBuild = EditorGUILayout.Toggle(new GUIContent("Development build", "Build as development build"), config.Common.IsDevelopmentBuild);
                    config.Icon.Override = EditorGUILayout.Toggle("Override Icon", config.Icon.Override);
                }

                if(data.IsBase || config.Icon.Override)
                {
                    config.Icon.Texture = (Texture2D)EditorGUILayout.ObjectField("Icon", config.Icon.Texture, typeof(Texture2D), false);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();

                // IOS Condifguration
                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                config.Ios.BundleIdentifier = InheritableTextField("Bundle Identifier", "iOS bundle identifier", config.Ios.BundleIdentifier, data.IsBase);
                config.Ios.Flags = InheritableTextField("Flags", "iOS specific defined symbols", config.Ios.Flags, data.IsBase);
                config.Ios.RemovedResources = InheritableTextField("Remove Resources", "Extended Feature. Folders and files under Assets to be removed before build", config.Ios.RemovedResources, data.IsBase);
                config.Ios.XcodeModsPrefixes = InheritableTextField("Xcodemods prefixes", "Xcodemods prefixes to execute", config.Ios.XcodeModsPrefixes, data.IsBase);

                EditorGUILayout.Space();

                // Android Condifguration
                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                config.Android.BundleIdentifier = InheritableTextField("Bundle Identifier", "Android bundle identifier", config.Android.BundleIdentifier, data.IsBase);
                config.Android.Flags = InheritableTextField("Flags", "Android specific defined symbols", config.Android.Flags, data.IsBase);
                config.Android.RemovedResources = InheritableTextField("Remove Resources", "Extended Feature. Folders and files under Assets to be to removed before build", config.Android.RemovedResources, data.IsBase);

                EditorGUILayout.Space();

                if(!data.IsBase)
                {
                    config.Android.ForceBundleVersionCode = EditorGUILayout.Toggle("Force Bundle Version Code", config.Android.ForceBundleVersionCode);
                }

                if(data.IsBase || config.Android.ForceBundleVersionCode)
                {
                    config.Android.BundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", config.Android.BundleVersionCode);
                }

                EditorGUILayout.Space();

                if(!data.IsBase)
                {
                    config.Android.UseKeystore = EditorGUILayout.Toggle("Use release keystore", config.Android.UseKeystore);
                }

                if(data.IsBase || config.Android.UseKeystore)
                {
                    config.Android.Keystore.Path = InheritableTextField("Keystore file", "Android keystore file path. Relative to project", config.Android.Keystore.Path, data.IsBase);
                    config.Android.Keystore.FilePassword = InheritableTextField("Keystore password", "Keystore file password", config.Android.Keystore.FilePassword, data.IsBase);
                    config.Android.Keystore.Alias = InheritableTextField("Alias", "Keystore alias", config.Android.Keystore.Alias, data.IsBase);
                    config.Android.Keystore.Password = InheritableTextField("Password", "Keystore alias password", config.Android.Keystore.Password, data.IsBase);
                }

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

                if(EditEnabled)
                {
                    if(GUILayout.Button(new GUIContent("Apply+", "Apply Build Set with extended features"), Styles.ActionButtonOptions))
                    {
                        try
                        {
                            ApplyConfig(config, true);

                            EditorUtility.DisplayDialog("Config applied successfully with extended features", 
                                string.Format("{0} build set was applied successfully to Player Settings.", data.Name), "Ok");
                        }
                        catch(Exception e)
                        {
                            EditorUtility.DisplayDialog("Error applying config", e.Message, "Ok");
                        }
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

            EditorGUILayout.Space();

            EditEnabled = GUILayout.Toggle(EditEnabled, new GUIContent("Advanced Mode", "Enables edition mode for project file"), EditorStyles.toolbarButton);

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