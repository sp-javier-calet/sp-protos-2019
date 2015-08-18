
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

        static BuildOptions GetBuildOptions()
        {
            BuildOptions result = BuildOptions.None;
            string[] buildOptions = GetCommandLineArgs("buildOptions");
            if (buildOptions != null)
            {
                foreach(var option in buildOptions)
                {
                    try
                    {
                        BuildOptions parsedOption = (BuildOptions)Enum.Parse(typeof(BuildOptions), option);
                        result = result | parsedOption;
                    }
                    catch
                    {
                        UnityEngine.Debug.LogError("Failed to parse build option: " + option);
                    }
                }
            }
            return result;
        }

        [MenuItem("File/AutoBuilder/Mac OSX/Intel")]
        static void PerformOSXIntelBuild()
        {
            SetDefines(BuildTargetGroup.Standalone);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.StandaloneOSXIntel);
            BuildOptions buildOptions = GetBuildOptions();
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/OSX-Intel/" + ProjectName + ".app", BuildTarget.StandaloneOSXIntel, buildOptions);

        }

        [MenuItem("File/AutoBuilder/iOS")]
        static void PerformiOSBuild()
        {
            BuildOptions buildOptions = GetBuildOptions();
#if UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6
            SetDefines(BuildTargetGroup.iPhone);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iPhone);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/iOS", BuildTarget.iPhone, buildOptions);
#else
            SetDefines(BuildTargetGroup.iOS);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.iOS);
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/iOS", BuildTarget.iOS, buildOptions);
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
            BuildOptions buildOptions = GetBuildOptions();
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Android/" + ProjectName + ".apk", BuildTarget.Android, buildOptions);
        }

        [MenuItem("File/AutoBuilder/Web/Standard")]
        static void PerformWebBuild()
        {
            SetDefines(BuildTargetGroup.WebPlayer);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayer);
            BuildOptions buildOptions = GetBuildOptions();
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Web", BuildTarget.WebPlayer, buildOptions);
        }

        [MenuItem("File/AutoBuilder/Web/Streamed")]
        static void PerformWebStreamedBuild()
        {
            SetDefines(BuildTargetGroup.WebPlayer);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.WebPlayerStreamed);
            BuildOptions buildOptions = GetBuildOptions();
            BuildPipeline.BuildPlayer(ScenePaths, "Builds/Web-Streamed", BuildTarget.WebPlayerStreamed, buildOptions);
        }
    }
}
