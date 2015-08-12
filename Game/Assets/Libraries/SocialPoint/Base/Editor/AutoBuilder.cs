
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace SocialPoint.Base
{
    public static class AutoBuilder
    {
        static string ProjectName
        {
            get
            {
                string[] s = Application.dataPath.Split(Path.DirectorySeparatorChar);
                return s[s.Length - 2];
            }
        }

        private static bool IsDebugScene(EditorBuildSettingsScene scene)
        {
            // Exclude '/Debug*' scenes from release builds
#if NO_ADMIN_PANEL
            return Path.GetFileName(scene.path).StartsWith("Debug");
#else
            return false;
#endif 
        }

        static string[] ScenePaths
        {
            get
            {
                List<string> scenes = new List<string>();
                for(int i = 0; i < EditorBuildSettings.scenes.Length; i++)
                {
                    EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                    if(scene.enabled && !IsDebugScene(scene))
                    {
                        scenes.Add(scene.path);
                    }
                }
                return scenes.ToArray();
            }
        }

        static string[] GetCommandLineArgs(string name)
        {
            List<string> values = new List<string>();

            string[] arguments = System.Environment.GetCommandLineArgs();
            foreach(var arg in arguments)
            {
                string argName = "+" + name + "=";
                if(arg.StartsWith(argName))
                {
                    values.Add(arg.Substring(argName.Length));
                }
            }
            return values.ToArray();
        }

        static void SetDefines(BuildTargetGroup group)
        {    
            string[] defines = GetCommandLineArgs("defines");
            string define = string.Empty;

            foreach(var def in defines)
            {
                define += def.ToUpper() + ";";
            }

            if(!string.IsNullOrEmpty(define))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(group, define);
            }
        }

        [MenuItem("File/AutoBuilder/Mac OSX/Intel")]
        static void PerformOSXIntelBuild()
        {
            SetDefines(BuildTargetGroup.Standalone);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/OSX-Intel/" + ProjectName + ".app", BuildTarget.StandaloneOSXIntel, BuildOptions.None);
        }

        [MenuItem("File/AutoBuilder/iOS")]
        static void PerformiOSBuild()
        {
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            SetDefines(BuildTargetGroup.iPhone);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iPhone);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/iOS", BuildTarget.iPhone, BuildOptions.None);
#else
            SetDefines(BuildTargetGroup.iOS);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/iOS", BuildTarget.iOS, BuildOptions.None);
#endif
        }

        [MenuItem("File/AutoBuilder/Android")]
        static void PerformAndroidBuild()
        {
            SetDefines(BuildTargetGroup.Android);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            EditorUserBuildSettings.androidBuildSubtarget = AndroidBuildSubtarget.ETC;
#else
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
#endif

            EditorPrefs.SetString("AndroidSdkRoot", System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal) + "/Development/android-sdk/");
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Android/" + ProjectName + ".apk", BuildTarget.Android, BuildOptions.None);
        }

        [MenuItem("File/AutoBuilder/Web/Standard")]
        static void PerformWebBuild()
        {
            SetDefines(BuildTargetGroup.WebPlayer);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayer);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Web", BuildTarget.WebPlayer, BuildOptions.None);
        }

        [MenuItem("File/AutoBuilder/Web/Streamed")]
        static void PerformWebStreamedBuild()
        {
            SetDefines(BuildTargetGroup.WebPlayer);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayerStreamed);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Web-Streamed", BuildTarget.WebPlayerStreamed, BuildOptions.None);
        }
    }
}
