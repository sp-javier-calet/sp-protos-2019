using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpartaTools.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Build
{
    public static class NativeBuild
    {
        static readonly List<string> IgnoredAndroidNativeModules = new List<string>{ "lib" };

        static string _sourcesDirectoryPath;

        static string SourcesDirectoryPath
        {
            get
            {
                if(string.IsNullOrEmpty(_sourcesDirectoryPath))
                {
                    _sourcesDirectoryPath = SpartaPaths.SourcesDirAbsolute;
                }

                return _sourcesDirectoryPath;
            }
        }

        static string AndroidSDKPath
        {
            get
            {
                return EditorPrefs.GetString("AndroidSdkRoot");
            }
        }

        static string AndroidNDKPath
        {
            get
            {
                // Retrieve stored NDK path from Unity settings
                return EditorPrefs.GetString("AndroidNdkRoot");
            }
        }

        static string GlobalProvisioningProfileUuid
        {
            get
            {
                return EditorPrefs.GetString("XCodeProvisioningProfileUuid");
            }
        }

        static string InstallationPath
        {
            get
            {
                // Retrived running Unity instance installation path
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

        static void ValidateResult(NativeConsole.Result result)
        {
            if(result.Code != 0)
            {
                throw new CompilerErrorException(string.Format("Sparta Native Build compilation failed:\n{0}\n\nFull output:\n{1}", result.Error, result.Output));
            }
        }

        public static void CompileAndroid()
        {
            CompileAndroid(
                path => EditorUtility.DisplayProgressBar("Compiling Android plugin", path, 0.1f), 
                EditorUtility.ClearProgressBar);
        }

        public static void CompileAndroid(Action<string> onBuildStart, Action onBuildEnd)
        {
            var commandOutput = new StringBuilder("Compile SPUnityPlugins for Android");
            var path = Path.Combine(SourcesDirectoryPath, "Android/sp_unity_plugins");
            var unityPath = InstallationPath;

            /* Check prerequisites. 
             * Android build requires a project-update to update the local configuration, 
             * including the path to the Android SDK. This file is created by Android Studio 
             * after importing, but we can fake it using the SDK location configured within Unity.
             */
            var localFile = Path.Combine(path, "local.properties");
            if(!File.Exists(localFile))
            {
                File.WriteAllText(localFile, "sdk.dir=" + AndroidSDKPath); 
            }

            // Start Build
            var msg = string.Format("Building Android SPUnityPlugins {0}", path);
            Debug.Log(msg);
            commandOutput.AppendLine(msg);

            if(onBuildStart != null)
            {
                onBuildStart(path);
            }

            var bin = path + "/gradlew";
            var param = string.Format("generateUnityPlugin -PunityInstallationPath='{0}'", unityPath);
            Debug.Log(string.Format("Running build command: {0} {1}", bin, param)); 
            var result = NativeConsole.RunProcess(bin, param, path);

            commandOutput.AppendLine(result.Output);
            Debug.Log(commandOutput.ToString());

            if(onBuildEnd != null)
            {
                onBuildEnd();
            }

            ValidateResult(result);
        }


        public static void CompileAndroidNative()
        {
            CompileAndroidNative(
                (progress, msg) => EditorUtility.DisplayProgressBar("Compiling native plugin", msg, progress), 
                EditorUtility.ClearProgressBar);
        }

        public static void CompileAndroidNative(Action<float, string> onProgress, Action onModuleEnd)
        {
            if(string.IsNullOrEmpty(AndroidNDKPath))
            {
                Debug.LogError("Error compiling Android native plugins. No NDK Path configured");
                return;
            }

            var commandOutput = new StringBuilder("Compile Android Native Plugins");
            var path = Path.Combine(SourcesDirectoryPath, "Android/sp_unity_native_plugins");

            var dirs = Directory.GetDirectories(path);

            float step = 1.0f / (dirs.Length + 1);
            float currentStep = step;

            foreach(var plugin in dirs)
            {
                var pluginDir = Path.GetFileName(plugin);
                if(IgnoredAndroidNativeModules.Contains(pluginDir))
                {
                    continue;
                }

                var msg = string.Format("Compiling native plugin {0}", pluginDir);
                Debug.Log(msg);
                commandOutput.AppendLine(msg);

                if(onProgress != null)
                {
                    onProgress(currentStep, msg);
                }
                currentStep += step;

                var bin = Path.Combine(path, "build_native_plugin.sh");
                var param = string.Format("{0} {1}", pluginDir, AndroidNDKPath);
                Debug.Log(string.Format("Running build command: {0} {1}", bin, param)); 
                var result = NativeConsole.RunProcess(bin, param, path);
                commandOutput.AppendLine(result.Output);

                if(onModuleEnd != null)
                {
                    onModuleEnd();
                }

                ValidateResult(result);
            }
            Debug.Log(commandOutput.ToString());
        }

        public static void CompileIOS()
        {
            CompileAppleProjectTarget("generateUnityPlugin");
        }

        public static void CompileTVOS()
        {
            CompileAppleProjectTarget("generateUnityPlugin_tvOS");
        }

        public static void CompileOSX()
        {
            CompileAppleProjectTarget("generateUnityPlugin_macOS");
        }

        public static void CompileAll()
        {
            CompileAndroid();
            CompileAndroidNative();
            CompileIOS();
            CompileTVOS();
            CompileOSX();
        }

        static void CompileAppleProjectTarget(string target)
        {
            var commandOutput = new StringBuilder(string.Format("Compile SPUnityPlugins {0} for Apple Platforms", target));
            var path = Path.Combine(SourcesDirectoryPath, "Apple/sp_unity_plugins");

            var paramsBuilder = new StringBuilder();
            paramsBuilder.AppendFormat(" -target {0} ", target);

            var provisioningUuid = GlobalProvisioningProfileUuid;
            var provisioningMessage = string.Empty;
            if(!string.IsNullOrEmpty(provisioningUuid))
            {
                paramsBuilder.AppendFormat(" PROVISIONING_PROFILE={0} ", provisioningUuid);
                provisioningMessage = "Using provisioning profile " + provisioningUuid;
            }

            var msg = string.Format("Building target '{0}' for SPUnityPlugins '{1}'. {2}", target, path, provisioningMessage);
            commandOutput.AppendLine(msg);
            EditorUtility.DisplayProgressBar("Compiling native plugin", msg, 0.1f);

            const string bin = "xcodebuild";

            var param = "-version";
            Debug.Log(string.Format("Running build command: {0} {1}", bin, param)); 
            var result = NativeConsole.RunProcess(bin, param, path);
            commandOutput.AppendLine(result.Output);

            param = paramsBuilder.ToString();
            Debug.Log(string.Format("Running build command: {0} {1}", bin, param)); 
            result = NativeConsole.RunProcess(bin, param, path);
            commandOutput.AppendLine(result.Output);

            Debug.Log(commandOutput.ToString());

            EditorUtility.ClearProgressBar();

            ValidateResult(result);
        }
    }
}
