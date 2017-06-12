using UnityEngine;
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

        static string[] ActiveScenes
        {
            get
            {
                var scenes = new List<string>();
                foreach(var scene in EditorBuildSettings.scenes)
                {
                    if(scene.enabled)
                    {
                        scenes.Add(scene.path);
                    }
                }
                return scenes.ToArray();
            }
        }

        static string GetCommandLineArg(string name, string defaultValue)
        {
            string value = defaultValue;
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

        static string GetDefaultLocationForTarget(BuildTarget target)
        {
            switch(target)
            {
            case BuildTarget.Android: return "Builds/Android";
            case BuildTarget.iOS: return "Builds/iOS";
            case BuildTarget.tvOS: return "Builds/tvOS";
            case BuildTarget.StandaloneOSXIntel: return "Builds/OSX-Intel";
            default:
                throw new NotSupportedException("Unsupported platform " + target);
            }
        }

        static string GetLocationForTarget(BuildTarget target, string outputPath, string projectName)
        {
            if(string.IsNullOrEmpty(outputPath))
            {
                outputPath = GetDefaultLocationForTarget(target);
            }

            string path;

            switch(target)
            {
            case BuildTarget.Android:
                path = Path.Combine(outputPath, projectName + ".apk");
                break;
            case BuildTarget.iOS:
            case BuildTarget.tvOS:
                path = outputPath;
                break;
            case BuildTarget.StandaloneOSXIntel:
                path = Path.Combine(outputPath, projectName + ".app");
                break;
            default:
                throw new NotSupportedException("Unsupported platform " + target);
            }

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

                Log("Defined Android compilation for ARMv7 with ETC1 compression");
            }

            Log(string.Format("Defined Build Target: '{0}'", EditorUserBuildSettings.activeBuildTarget));
        }

        static void OverrideVersion(string version)
        {
            if(!string.IsNullOrEmpty(version))
            {
                PlayerSettings.bundleVersion = version;
            }

            Log(string.Format("Defined bundle version: '{0}'", PlayerSettings.bundleVersion));
        }

        static void OverrideBuildNumber(int buildNumber)
        {
            // Set build number. May be overriden by Build Set.
            if(buildNumber == 0)
            {
                buildNumber = Int32.Parse(DateTime.Today.ToString("yyMMddhhmm"));
            }

            if(buildNumber > 0)
            {
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            }

            Log(string.Format("Defined Build Number (Bundle Version Code): '{0}'", buildNumber));
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

        static void Log(string message)
        {
            Debug.Log(string.Format("Sparta-Autobuilder: {0}", message));
        }

        #region Public builder interface

        public static void BuildWithArgs()
        {
            // Parse Build number argument
            int versionNumber = 0;
            try
            {
                versionNumber = Int32.Parse(GetCommandLineArg("build", null));
            }
            catch(Exception e)
            {
                throw new ArgumentException("A valid build number must be provided for the 'build' argument", e);
            }

            // Parse optional arguments
            var versionName = GetCommandLineArg("version", string.Empty);
            var builSetName = GetCommandLineArg("config", BuildSet.DebugConfigName);
            var outputPath = GetCommandLineArg("output", string.Empty);

            // Launch build
            Build(EditorUserBuildSettings.activeBuildTarget, builSetName, versionNumber, versionName, outputPath);
        }

        /// <summary>
        /// Build project for the specified target and config.
        /// </summary>
        /// <param name="target">Target Platform</param>
        /// <param name="buildSetName">Build set name to apply before compiling</param>
        /// <param name="versionNumber">Version number. If zero, local current timestamp will be used. If negative, it will use the one defined in Unity Player Settings. </param>
        /// <param name="versionName">Short Version Name. If it is null or empty, it will use the one defined in Unity Player Settings. </param>
        /// <param name="outPath">Output location. </param>
        public static void Build(BuildTarget target, string buildSetName, int versionNumber = -1, string versionName = null, string outputPath = null)
        {
            string detailedOutput = "";
            Application.LogCallback Callback = (msg, stack, type) => {
                if(type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
                {
                    detailedOutput += (type + " - " + msg + "\n" + stack + "\n\n");
                }
            };
            Application.logMessageReceived += Callback;
            Log(string.Format("Starting Build <{0}> for target <{1}> with config set <{2}>", 
                versionNumber, target, buildSetName));

            if(BuildPipeline.isBuildingPlayer)
            {
                throw new InvalidOperationException("Already building a player");
            }

            SetTarget(target);

            var buildSet = LoadBuildSetByName(buildSetName);
            Log(string.Format("Applying '{0}' Build Set with extended features...", buildSet.Name));
            buildSet.ApplyExtended();

            /*
             * Settings Override
             */ 
            if(!buildSet.App.OverrideBuild)
            {
                OverrideBuildNumber(versionNumber);
            }

            OverrideVersion(versionName);

            /*
             * Start build
             */ 
            string[] activeScenes = ActiveScenes;
            Log(string.Format("Building player with active scenes: '{0}'", string.Join(", ", activeScenes)));

            var location = GetLocationForTarget(target, outputPath, ProjectName);
            Log(string.Format("Building player in path '{0}", location));

            var options = buildSet.Options;
            Log(string.Format("Building player with options: '{0}'", options));

            // Dump config report after apply config
            new BuildReport()
                .CollectBaseSettings()
                .AddBuildSetInfo(buildSet)
                .CollectPlayerSettings()
                .Dump();

            Log("Starting Player Build");
            string result = BuildPipeline.BuildPlayer(activeScenes, location, target, options);

            Log(string.Format("Player Build finished with result: '{0}'", result));
            Log(string.Format("Detailed log:\n '{0}' \nEnd of detailed log\n", detailedOutput));
            Application.logMessageReceived -= Callback;
            
            if(!string.IsNullOrEmpty(result))
            {
                throw new CompilerErrorException(result);
            }
        }

        #endregion
    }
}

