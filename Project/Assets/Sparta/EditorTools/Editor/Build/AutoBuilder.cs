﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;


namespace SpartaTools.Editor.Build
{
    public static class AutoBuilder
    {
        static string ProjectName
        {
            get
            {
                // Returns main project folder as ProjectName
                string[] s = Application.dataPath.Split(Path.DirectorySeparatorChar);
                return s[s.Length - 2];
            }
        }

        static bool IncludedScene(EditorBuildSettingsScene scene)
        {
            if(!scene.enabled)
            {
                return false;
            }
                
#if ADMIN_PANEL || DEBUG
            return true;
#else
            // Exclude '/Debug*' scenes from release builds
            return !Path.GetFileName(scene.path).StartsWith("Debug");
#endif 
        }

        static string[] ScenePaths
        {
            get
            {
                var scenes = new List<string>();
                foreach(var scene in EditorBuildSettings.scenes)
                {
                    if(IncludedScene(scene))
                    {
                        scenes.Add(scene.path);
                    }
                }
                return scenes.ToArray();
            }
        }

        static string GetCommandLineArg(string name)
        {
            string value = null;
            string[] arguments = Environment.GetCommandLineArgs();
            foreach(var arg in arguments)
            {
                string argName = "+" + name + "=";
                if(arg.StartsWith(argName))
                {
                    value = arg.Substring(argName.Length);
                    break;
                }
            }

            return value;
        }

        static string GetLocationForTarget(BuildTarget target, string projectName)
        {
            string path;

            switch(target)
            {
            case BuildTarget.Android:
                path = "Builds/Android/" + projectName + ".apk";
                break;
            case BuildTarget.iOS:
                path = "Builds/iOS";
                break;
            case BuildTarget.tvOS:
                path = "Builds/tvOS";
                break;
            case BuildTarget.StandaloneOSXIntel:
                path = "Builds/OSX-Intel/" + projectName + ".app";
                break;
            default:
                throw new NotSupportedException("Unsupported platform " + target);
            }

            // TODO Correct??
            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return path;
        }

        static void SetTarget(BuildTarget target)
        {
            // Select target platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(target);

            if(target == BuildTarget.Android)
            {
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;
            }
        }

        static void SetBuildNumber(int buildNumber)
        {
            // Set build number. May be overriden by Build Set.
            if(buildNumber == 0)
            {
                buildNumber = Int32.Parse(DateTime.Today.ToString("yyMMddhhmm"));
            }
            PlayerSettings.Android.bundleVersionCode = buildNumber;
        }

        static BuildSet LoadBuildSetByName(string buildSetName)
        {
            var buildSet = BuildSet.Load(buildSetName);
            if(buildSet == null)
            {
                throw new FileNotFoundException("No Build Set found for name: " + buildSetName); 
            }

            return buildSet;
        }

        #region Public builder interface

        public static void BuildWithArgs()
        {
            // Parse Build number argument
            int versionNumber = 0;
            string versionArg = GetCommandLineArg("version");
            if(versionArg != null)
            {
                versionNumber = Int32.Parse(versionArg);
            }

            // Select build set
            string builSetName = "Release";
            string buildSetArg = GetCommandLineArg("build");
            if(buildSetArg != null)
            {
                builSetName = buildSetArg;
            }

            // Launch build
            Build(EditorUserBuildSettings.activeBuildTarget, builSetName, versionNumber);
        }

        public static void Build(BuildTarget target, string buildSetName, int buildNumber = 0)
        {
            Debug.Log(string.Format("Sparta AutoBuilder: Starting Build <{0}> for target <{1}> with config set <{2}>", buildNumber, target, buildSetName));

            if(BuildPipeline.isBuildingPlayer)
            {
                throw new InvalidOperationException("Already building a player");
            }

            SetTarget(target);

            SetBuildNumber(buildNumber);

            var buildSet = LoadBuildSetByName(buildSetName);
            buildSet.ApplyExtended();

            // Start build
            var location = GetLocationForTarget(target, ProjectName);
            string result = BuildPipeline.BuildPlayer(ScenePaths, location, target, buildSet.Options);
            Debug.Log(result);
        }

        #endregion

        #region Editor options

        [MenuItem("Sparta/Build/Player/Android Release", false, 101)]
        public static void BuildAndroidRelease()
        {
            AutoBuilder.Build(BuildTarget.Android, "Release");
        }

        [MenuItem("Sparta/Build/Player/Android Debug", false, 102)]
        public static void BuildAndroidDebug()
        {
            AutoBuilder.Build(BuildTarget.Android, "Debug");
        }

        [MenuItem("Sparta/Build/Player/Ios Release", false, 201)]
        public static void BuildIosRelease()
        {
            AutoBuilder.Build(BuildTarget.iOS, "Release");
        }

        [MenuItem("Sparta/Build/Player/Ios Debug", false, 202)]
        public static void BuildIosDebug()
        {
            AutoBuilder.Build(BuildTarget.iOS, "Debug");
        }

        #endregion
    }
}

