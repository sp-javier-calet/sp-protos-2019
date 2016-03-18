﻿using UnityEngine;
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

        bool _dirty;
        Vector2 _scrollPosition = Vector2.right;

        Dictionary<string, BuildSetViewData> _buildSetData = null;

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

            // Add base config
            configs.Add("Base", new BuildSetViewData("Base", BaseSettings.Load(), true));

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
                config.CommonFlags = EditorGUILayout.TextField("Flags", config.CommonFlags);
                config.RebuildNativePlugins = EditorGUILayout.Toggle("Rebuild native plugins", config.RebuildNativePlugins);

                config.OverrideIcon = EditorGUILayout.Toggle("Override Icon", config.OverrideIcon);
                if(config.OverrideIcon)
                {
                    config.Icon = (Texture2D)EditorGUILayout.ObjectField("Icon", config.Icon, typeof(Texture2D), false);
                }

                EditorGUILayout.Space();

                EditorGUILayout.LabelField("IOS", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.IosBundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", config.IosBundleIdentifier);
                config.IosFlags = EditorGUILayout.TextField("Flags", config.IosFlags);
                config.XcodeModsPrefixes = EditorGUILayout.TextField("Xcodemods prefixes", config.XcodeModsPrefixes);
                GUILayout.EndVertical();
                EditorGUILayout.Space();

                EditorGUILayout.LabelField("Android", EditorStyles.boldLabel);
                GUILayout.BeginVertical();
                config.AndroidBundleIdentifier = EditorGUILayout.TextField("Bundle Identifier", config.AndroidBundleIdentifier);
                config.AndroidFlags = EditorGUILayout.TextField("Flags", config.AndroidFlags);

                config.ForceBundleVersionCode = EditorGUILayout.Toggle("Force Bundle Version Code", config.ForceBundleVersionCode);
                if(config.ForceBundleVersionCode)
                {
                    config.BundleVersionCode = EditorGUILayout.IntField("Bundle Version Code", config.BundleVersionCode);
                }

                config.UseKeystore = EditorGUILayout.Toggle("Use release keystore", config.UseKeystore);
                if(config.UseKeystore)
                {
                    config.KeystorePath = EditorGUILayout.TextField("Keystore file", config.KeystorePath);
                    config.KeystoreFilePassword = EditorGUILayout.TextField("Keystore password", config.KeystoreFilePassword);
                    config.KeystoreAlias = EditorGUILayout.TextField("Alias", config.KeystoreAlias);
                    config.KeystorePassword = EditorGUILayout.TextField("Password", config.KeystorePassword);
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

        void OnGUI()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Build Sets", EditorStyles.boldLabel);

            // Read BuildSet definitions
            if(_buildSetData == null || _dirty)
            {
                _buildSetData = LoadViewConfig();
                _dirty = false;
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