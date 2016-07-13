using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.Build
{
    public static class NativeBuild
    {
        static string SourcesDirectoryPath
        {
            get
            {
                return Path.Combine(Application.dataPath, "../Sources");
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

        #region Editor options

        [MenuItem("Sparta/Build/Plugins/Android Java Plugins", false, 101)]
        public static void CompileAndroid()
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

            EditorUtility.DisplayProgressBar("Compiling Android plugin", msg, 0.1f);

            var result = NativeConsole.RunProcess(path + "/gradlew", string.Format("generateUnityPlugin -PunityInstallationPath='{0}'", unityPath), path);

            commandOutput.AppendLine(result.Output);
            Debug.Log(commandOutput.ToString());

            EditorUtility.ClearProgressBar();

            ValidateResult(result);
        }

        [MenuItem("Sparta/Build/Plugins/Android Native Plugins", false, 102)]
        public static void CompileAndroidNative()
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
                var msg = string.Format("Compiling native plugin {0}", pluginDir);
                Debug.Log(msg);
                commandOutput.AppendLine(msg);

                EditorUtility.DisplayProgressBar("Compiling native plugin", msg, currentStep);
                currentStep += step;

                var result = NativeConsole.RunProcess(Path.Combine(path, "build_native_plugin.sh"), string.Format("{0} {1}", pluginDir, AndroidNDKPath), path);
                commandOutput.AppendLine(result.Output);

                EditorUtility.ClearProgressBar();

                ValidateResult(result);
            }
            Debug.Log(commandOutput.ToString());
        }

        [MenuItem("Sparta/Build/Plugins/iOS Plugins", false, 201)]
        public static void CompileIOS()
        {
            CompileAppleProjectTarget("generateUnityPlugin");
        }

        [MenuItem("Sparta/Build/Plugins/tvOS Plugins", false, 202)]
        public static void CompileTVOS()
        {
            CompileAppleProjectTarget("generateUnityPlugin_tvOS");
        }

        [MenuItem("Sparta/Build/Plugins/OSX Plugins", false, 202)]
        public static void CompileOSX()
        {
            CompileAppleProjectTarget("generateUnityPlugin_macOS");
        }

        [MenuItem("Sparta/Build/Plugins/Build All", false, 500)]
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

            var result = NativeConsole.RunProcess("xcodebuild", paramsBuilder.ToString(), path);
            commandOutput.AppendLine(result.Output);

            Debug.Log(commandOutput.ToString());

            EditorUtility.ClearProgressBar();

            ValidateResult(result);
        }

        #endregion
    }
}
