using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Build;
using UnityEditor.Build;

namespace SpartaTools.Editor.View
{
    [UnityEditor.InitializeOnLoad]
    public static class BuildSetApplier
    {
        class TargetChangedListener : IActiveBuildTargetChanged
        {
            public int callbackOrder { get; set; }

            public void OnActiveBuildTargetChanged(BuildTarget previousTarget, BuildTarget newTarget)
            {
                // Reapply the current config after change target platform
                if (BuildSetApplier.AutoApply)
                {
                    BuildSetApplier.Reapply();
                }
            }
        }

        const string CurrentModeKey = "SpartaCurrentBuildSet";
        const string AutoApplyKey = "SpartaAutoApplyBuildSetEnabled";
        const string AutoApplyLastTimeKey = "SpartaAutoApplyLastTime";

        static bool? _autoApply;

        #pragma warning disable 414
        static TargetChangedListener _targetChangedListener;
        #pragma warning restore 414

        public static bool AutoApply
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

        static string _currentMode;

        public static string CurrentMode
        {
            get
            {
                if(_currentMode == null)
                {
                    _currentMode = EditorPrefs.GetString(CurrentModeKey);
                }
                return _currentMode;
            }
            private set
            {
                _currentMode = value;
                EditorPrefs.SetString(CurrentModeKey, value);
            }
        }

        static float AutoApplyLastTime
        {
            get
            {
                return EditorPrefs.GetFloat(AutoApplyLastTimeKey, float.MaxValue);
            }
            set
            {
                EditorPrefs.SetFloat(AutoApplyLastTimeKey, value);
            }
        }

        static BuildSetApplier()
        {
            _targetChangedListener = new TargetChangedListener();

            var playing = EditorApplication.isPlayingOrWillChangePlaymode;
            var compiling = EditorApplication.isCompiling;

            if(AutoApply && !playing && !compiling)
            {
                float currentTime = (float)EditorApplication.timeSinceStartup;
                var requiresApply = currentTime <= AutoApplyLastTime;

                if(requiresApply)
                {
                    Reapply();
                }

                AutoApplyLastTime = currentTime;
            }
        }

        #region Static functions

        public static void ApplyConfig(string configName)
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

        public static void ApplyConfig(BuildSet config)
        {
            CurrentMode = config.Name;
            config.Apply();
        }

        public static void ApplyExtendedConfig(BuildSet config)
        {
            CurrentMode = config.Name;
            config.ApplyExtended();
        }

        public static void Reapply()
        {
            var mode = CurrentMode;
            try
            {
                if(!string.IsNullOrEmpty(mode))
                {
                    Debug.Log(string.Format("Applying BuildSet '{0}'", mode));
                    ApplyConfig(mode);
                }
                else
                {
                    Debug.LogWarning("No BuildSet to apply");
                }
            }
            catch(FileNotFoundException e)
            {
                Debug.LogError(e.Message);    
                CurrentMode = string.Empty;
            }
        }

        #endregion
    }

    public class BuildSetsWindow : EditorWindow
    {
        const string InheritedLabel = "<inherited>";


        #region Static platform and buildset management

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

        [MenuItem("Sparta/Build/Debug", false, 101)]
        public static void SetDebugConfig()
        {
            BuildSetApplier.ApplyConfig(BuildSet.DebugConfigName);
        }

        [MenuItem("Sparta/Build/Debug", true)]
        static bool ValidateSetDebugConfig()
        {
            return BuildSetApplier.CurrentMode != BuildSet.DebugConfigName;
        }

        [MenuItem("Sparta/Build/Release", false, 102)]
        public static void SetReleaseConfig()
        {
            BuildSetApplier.ApplyConfig(BuildSet.ReleaseConfigName);
        }

        [MenuItem("Sparta/Build/Release", true)]
        static bool ValidateSetReleaseConfig()
        {
            return BuildSetApplier.CurrentMode != BuildSet.ReleaseConfigName;
        }

        [MenuItem("Sparta/Build/Shipping", false, 103)]
        public static void SetShippingConfig()
        {
            BuildSetApplier.ApplyConfig(BuildSet.ShippingConfigName);
        }

        [MenuItem("Sparta/Build/Shipping", true)]
        static bool ValidateSetShippingConfig()
        {
            return BuildSetApplier.CurrentMode != BuildSet.ShippingConfigName;
        }

        [MenuItem("Sparta/Build/Apply Current &s", false, 120)]
        public static void ApplyCurrent()
        {
            BuildSetApplier.Reapply();
        }

        [MenuItem("Sparta/Build/Build Set... &#s", false, 121)]
        public static void ShowBuildSettings()
        {
            EditorWindow.GetWindow(typeof(BuildSetsWindow), false, "Build Set", true);
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
            if(!Directory.Exists(BuildSet.ContainerPath))
            {
                Directory.CreateDirectory(BuildSet.ContainerPath);
            }

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
                config.App.ProductName = InheritableTextField("Product Name", "Application name", config.App.ProductName, data.IsBase);
                config.App.Version = InheritableTextField("Version", "Human-readable build version", config.App.Version, data.IsBase);
                if(!data.IsBase)
                {
                    config.App.OverrideBuild = EditorGUILayout.Toggle("Override Build Number", config.App.OverrideBuild);
                }

                if(data.IsBase || config.App.OverrideBuild)
                {
                    config.App.BuildNumber = EditorGUILayout.IntField("Build Number", config.App.BuildNumber);
                }

                config.Common.Flags = InheritableTextField("Flags", "Defined symbols for all platforms", config.Common.Flags, data.IsBase);
                config.Common.LogLevel = (BuildSet.LogLevel)EditorGUILayout.EnumPopup("Log Level", config.Common.LogLevel);
                TextDataLabel("Environment Url", "Jenkins Forced Environment Url", BuildSet.EnvironmentUrl);

                if(!data.IsBase)
                {
                    config.Common.EnableAdminPanel = EditorGUILayout.Toggle(new GUIContent("Enable Admin Panel", "Enable Admin Panel features"), config.Common.EnableAdminPanel);
                    config.Common.EnableDependencyInspection = EditorGUILayout.Toggle(new GUIContent("Enable Dependency Inspection", "Enable Dependency Inspection features"), config.Common.EnableDependencyInspection);
                    config.Common.RebuildNativePlugins = EditorGUILayout.Toggle(new GUIContent("Rebuild native plugins", "Extended Feature. Build platform plugins before build player"), config.Common.RebuildNativePlugins);
                    config.Common.IsDevelopmentBuild = EditorGUILayout.Toggle(new GUIContent("Development build", "Build as development build"), config.Common.IsDevelopmentBuild);
                    config.Common.AppendBuild = EditorGUILayout.Toggle(new GUIContent("Append build", "Append changes to build instead of replace all"), config.Common.AppendBuild);
                    config.Common.IncludeDebugScenes = EditorGUILayout.Toggle(new GUIContent("Include debug scenes", "Include scene files starting with 'Debug'"), config.Common.IncludeDebugScenes);
                    config.App.OverrideIcon = EditorGUILayout.Toggle("Override Icon", config.App.OverrideIcon);
                }

                if(data.IsBase || config.App.OverrideIcon)
                {
                    config.App.IconTexture = (Texture2D)EditorGUILayout.ObjectField("Icon", config.App.IconTexture, typeof(Texture2D), false);
                }
                GUILayout.EndVertical();

                EditorGUILayout.Space();

                // IOS Condifguration
                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);

                EditorGUILayout.Space();

                config.Ios.BundleIdentifier = InheritableTextField("Bundle Identifier", "iOS bundle identifier", config.Ios.BundleIdentifier, data.IsBase);
                config.Ios.Flags = InheritableTextField("Flags", "iOS specific defined symbols", config.Ios.Flags, data.IsBase);
                config.Ios.RemovedResources = InheritableTextField("Remove Resources", "Extended Feature. Folders and files under Assets to be removed before build", config.Ios.RemovedResources, data.IsBase);
                config.Ios.XcodeModSchemes = InheritableTextField("XcodeMod Schemes", "Xcodemods schemes to apply. 'base' and 'editor' schemes are managed automatically", config.Ios.XcodeModSchemes, data.IsBase);

                if(!data.IsBase)
                {
                    config.Ios.UseEnvironmentProvisioningUuid = EditorGUILayout.Toggle(new GUIContent("Use Global Provisioning", 
                        "Uses the Provisioning profile UUID defined by the 'SP_XCODE_PROVISIONING_PROFILE_UUID' environment variable"), 
                        config.Ios.UseEnvironmentProvisioningUuid);

                    if(config.Ios.UseEnvironmentProvisioningUuid)
                    {
                        TextDataLabel("Environment", "Local value of 'SP_XCODE_PROVISIONING_PROFILE_UUID'", BuildSet.EnvironmentProvisioningUuid);
                        TextDataLabel("Current", "Active Environment UUID value", BuildSet.CurrentGlobalProvisioningUuid);
                    }
                }

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
                    config.Android.UseKeystore = EditorGUILayout.Toggle("Use release keystore", config.Android.UseKeystore);
                }

                if(data.IsBase || config.Android.UseKeystore)
                {
                    config.Android.Keystore.Path = InheritableTextField("Keystore file", "Android keystore file path. Relative to project", config.Android.Keystore.Path, data.IsBase);
                    config.Android.Keystore.FilePassword = InheritableTextField("Keystore password", "Keystore file password", config.Android.Keystore.FilePassword, data.IsBase);
                    config.Android.Keystore.Alias = InheritableTextField("Alias", "Keystore alias", config.Android.Keystore.Alias, data.IsBase);
                    config.Android.Keystore.Password = InheritableTextField("Password", "Keystore alias password", config.Android.Keystore.Password, data.IsBase);
                }

                config.Android.UseAPKExpansionFile = EditorGUILayout.Toggle("Use APK Expansion Files", config.Android.UseAPKExpansionFile);

                EditorGUILayout.Space();

                GUILayout.BeginHorizontal();
                if(GUILayout.Button("Apply", Styles.ActionButtonOptions))
                {
                    try
                    {
                        BuildSetApplier.ApplyConfig(config);

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
                            BuildSetApplier.ApplyExtendedConfig(config);

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

            // Update Asset content
            EditorUtility.SetDirty(data.Config);
        }

        void TextDataLabel(string label, string tooltip, string value)
        {
            var content = new GUIContent("<none>");
            if(!string.IsNullOrEmpty(value))
            {
                content = new GUIContent(value);
            }

            EditorGUILayout.LabelField(new GUIContent(label, tooltip), content);
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

            var mode = BuildSetApplier.CurrentMode;
            var currentAutoApply = BuildSetApplier.AutoApply;

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
                BuildSetApplier.AutoApply = autoApply;
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
