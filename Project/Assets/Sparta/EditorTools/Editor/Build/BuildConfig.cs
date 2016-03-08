using UnityEngine;
using UnityEditor;

namespace SpartaTools.Editor.Build
{
    public class BuildConfig : EditorWindow
    {
        enum Mode
        {
            Debug,
            Release
        }

        struct ConfigSet
        {
            public string CommonFlags;
            public string AndroidFlags;
            public string IosFlags;

            public bool UseKeytore;
            public string KeystorePath;
            public string KeystoreFilePassword;
            public string KeystoreAlias;
            public string KeystorePassword;

        }

        static Mode CurrentMode = Mode.Debug;

        const string DebugSymbols = "ADMIN_PANELTEST"; // TODO Read from...?

        static readonly BuildTargetGroup[] AvailableTargets = { BuildTargetGroup.Android, BuildTargetGroup.iOS };

        ConfigSet CurrentConfig;

        #region Editor options

        [MenuItem("Sparta/Build/Debug", false, 1)]
        public static void SetDebugConfig()
        {
            CurrentMode = Mode.Debug;
            SetFlags(DebugSymbols);
        }

        [MenuItem("Sparta/Build/Debug", true)]
        static bool ValidateSetDebugConfig()
        {
            return CurrentMode != Mode.Debug;
        }

        [MenuItem("Sparta/Build/Release", false, 2)]
        public static void SetReleaseConfig()
        {
            CurrentMode = Mode.Release;
        }

        [MenuItem("Sparta/Build/Release", true)]
        static bool ValidateSetReleaseConfig()
        {
            return CurrentMode != Mode.Release;
        }

        [MenuItem("Sparta/Build/Build settings...", false, 3)]
        public static void ShowBuildSettings()
        {
            EditorWindow.GetWindow(typeof(BuildConfig), false, "Sparta Build Settings", true);
        }

        #endregion

        static void SetFlags(string defineSymbols)
        {
            foreach(var target in AvailableTargets)
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(target, defineSymbols);    
            }
        }

        void OnGUI()
        {
            CurrentConfig.CommonFlags = EditorGUILayout.TextField("Common Flags", CurrentConfig.CommonFlags);

            EditorGUILayout.LabelField("IOS");
            CurrentConfig.IosFlags = EditorGUILayout.TextField("Flags", CurrentConfig.IosFlags);

            EditorGUILayout.LabelField("Android");
            CurrentConfig.AndroidFlags = EditorGUILayout.TextField("Flags", CurrentConfig.AndroidFlags);
            CurrentConfig.UseKeytore = EditorGUILayout.Toggle("Use release keystore", CurrentConfig.UseKeytore);
            CurrentConfig.KeystorePath = EditorGUILayout.TextField("Keystore file", CurrentConfig.KeystorePath);
            CurrentConfig.KeystoreFilePassword = EditorGUILayout.TextField("Keystore password", CurrentConfig.KeystoreFilePassword);
            CurrentConfig.KeystoreAlias = EditorGUILayout.TextField("Alias", CurrentConfig.KeystoreAlias);
            CurrentConfig.KeystorePassword = EditorGUILayout.TextField("Password", CurrentConfig.KeystorePassword);
        }
    }
}