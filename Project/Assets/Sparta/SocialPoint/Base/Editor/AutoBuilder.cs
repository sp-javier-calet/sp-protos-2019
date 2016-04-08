
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;

namespace SocialPoint.Base
{
    // In order to extend the autobuilder, you must create a static IAutoBuilderConfiguratorFactory method
    // marked with the attribute AutoBuilderConfiguratorFactory and with the following signature:
    //
    // static IAutoBuilderConfigurator CreateMyConfigurator(AutoBuilderConfiguration configuration)
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
#if ADMIN_PANEL
            return false;
#else
            return Path.GetFileName(scene.path).StartsWith("Debug");
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

        public static string[] GetCommandLineArgs(string name)
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

            if(values.Count > 0)
            {
                string stringizedList = "";
                foreach(var elem in values)
                {
                    stringizedList += string.IsNullOrEmpty(stringizedList) ? elem : " | " + elem;
                }
                UnityEngine.Debug.Log("Custom argument: [" + name + "]\n\tValue(s): [" + stringizedList + "]");
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
            if(buildOptions != null)
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

        static IAutoBuilderConfigurator CreateAutoBuilderConfigurator(AutoBuilderConfiguration configuration)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var assembly in assemblies)
            {
                foreach(var type in assembly.GetTypes())
                {
                    MethodInfo[] methods = type.GetMethods();
                    foreach(var method in methods)
                    {
                        foreach(var attribute in method.GetCustomAttributes(true))
                        {
                            if(attribute is AutoBuilderConfiguratorFactory)
                            {
                                IAutoBuilderConfigurator configurator = CreateAutoBuilderByMethod(method, configuration);
                                if(configurator != null)
                                {
                                    return configurator;
                                }
                            }
                        }
                    }
                }
            }
            return new EmptyAutoBuilderConfigurator();
        }

        static IAutoBuilderConfigurator CreateAutoBuilderByMethod(MethodInfo method, AutoBuilderConfiguration configuration)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if(method.IsStatic
               && parameters.Length == 1
               && parameters[0].ParameterType == typeof(AutoBuilderConfiguration)
               && method.ReturnType == typeof(IAutoBuilderConfigurator))
            {
                object result = method.Invoke(null, new object[]{ configuration });
                return result as IAutoBuilderConfigurator;
            }

            UnityEngine.Debug.LogWarning("[WARNING] - Found a method marked as AutoBuilderConfiguratorFactory with a wrong signature: " + method);
            return null;
        }

        static void PerformBuildWithConfiguration(AutoBuilderConfiguration configuration, BuildTargetGroup buildTargetGroup)
        {
            SetDefines(buildTargetGroup);
            IAutoBuilderConfigurator configurator = CreateAutoBuilderConfigurator(configuration);
            configuration = configurator.Configure(configuration);
            EditorUserBuildSettings.SwitchActiveBuildTarget(configuration.Target);
            configurator.Build(configuration);
        }

        [MenuItem("File/AutoBuilder/Mac OSX/Intel")]
        static void PerformOSXIntelBuild()
        {
            PerformBuildWithConfiguration(new AutoBuilderConfiguration() {
                Levels = ScenePaths,
                LocationPathName = "Builds/OSX-Intel/" + ProjectName + ".app",
                Target = BuildTarget.StandaloneOSXIntel,
                Options = GetBuildOptions()
            }, BuildTargetGroup.Standalone);
        }

        [MenuItem("File/AutoBuilder/iOS")]
        static void PerformiOSBuild()
        {
            PerformBuildWithConfiguration(new AutoBuilderConfiguration() {
                Levels = ScenePaths,
                LocationPathName = "Builds/iOS",
                Target = BuildTarget.iOS,
                Options = GetBuildOptions()
            }, BuildTargetGroup.iOS);
        }

        [MenuItem("File/AutoBuilder/Android")]
        static void PerformAndroidBuild()
        {
            UnityEngine.Debug.Log("************************\nStarting Android Unity Build\n************************");
            if(!Directory.Exists("Builds/Android/"))
            {
                Directory.CreateDirectory("Builds/Android/");
            }

            // Setup Android Version Info
            string[] buildNumber = GetCommandLineArgs("build");
            if(buildNumber.Length == 0)
            {
                string data = System.DateTime.Today.ToString("yyMMddhmm");
                int value = Int32.Parse(data);     
                PlayerSettings.Android.bundleVersionCode = value;
                UnityEngine.Debug.Log("[INFO] Android bundle version code set to: [" + PlayerSettings.Android.bundleVersionCode + "]");
            }
            PlayerSettings.Android.useAPKExpansionFiles = false;

            // Note: FAT means all supported/available architectures.
            string[] targetArch = GetCommandLineArgs("targetArch");
            if(targetArch.Length == 0)
            {
                UnityEngine.Debug.LogWarning("[WARNING] - Target architecture not specified. Defaulting to all supported architectures");
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.FAT;
            }
            else if(targetArch[0] == "arm+x86")
            {
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.FAT;
            }
            else if(targetArch[0] == "arm")
            {
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;
            }
            else if(targetArch[0] == "x86")
            {
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.x86;
            }

            SetDefines(BuildTargetGroup.Android);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTarget.Android);
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;

            AutoBuilderConfiguration configuration = new AutoBuilderConfiguration() {
                Levels = ScenePaths,
                LocationPathName = "Builds/Android/" + ProjectName + ".apk",
                Target = BuildTarget.Android,
                Options = GetBuildOptions()
            };

            IAutoBuilderConfigurator configurator = CreateAutoBuilderConfigurator(configuration);
            configuration = configurator.Configure(configuration);

            // Setup Android KeyStore
            if(!string.IsNullOrEmpty(configuration.AndroidKeyStoreName))
            {
                PlayerSettings.Android.keystoreName = configuration.AndroidKeyStoreName;
            }
            if(!string.IsNullOrEmpty(configuration.AndroidKeyStorePass))
            {
                PlayerSettings.Android.keystorePass = configuration.AndroidKeyStorePass;
            }
            if(!string.IsNullOrEmpty(configuration.AndroidKeyAliasName))
            {
                PlayerSettings.Android.keyaliasName = configuration.AndroidKeyAliasName;
            }
            if(!string.IsNullOrEmpty(configuration.AndroidKeyAliasPass))
            {
                PlayerSettings.Android.keyaliasPass = configuration.AndroidKeyAliasPass;
            }
            if(!string.IsNullOrEmpty(configuration.BundleIdentifier))
            {
                PlayerSettings.bundleIdentifier = configuration.BundleIdentifier;
            }
            configurator.Build(configuration);
        }



        [MenuItem("File/AutoBuilder/Web/Standard")]
        static void PerformWebBuild()
        {
            PerformBuildWithConfiguration(new AutoBuilderConfiguration() {
                Levels = ScenePaths,
                LocationPathName = "Builds/Web",
                Target = BuildTarget.WebPlayer,
                Options = GetBuildOptions()
            }, BuildTargetGroup.WebPlayer);
        }

        [MenuItem("File/AutoBuilder/Web/Streamed")]
        static void PerformWebStreamedBuild()
        {
            PerformBuildWithConfiguration(new AutoBuilderConfiguration() {
                Levels = ScenePaths,
                LocationPathName = "Builds/Web-Streamed",
                Target = BuildTarget.WebPlayerStreamed,
                Options = GetBuildOptions()
            }, BuildTargetGroup.WebPlayer);
        }
    }
}