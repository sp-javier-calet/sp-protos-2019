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

        static string InstallationPath
        {
            get
            {
                // Retrived running Unity instance installation path
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

        #region Editor options

        [MenuItem("Sparta/Build/Plugins/Android Plugins", false, 101)]
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

            AsyncProcess.Start(progress => {
                NativeConsole.RunProcess(path + "/gradlew", string.Format("generateUnityPlugin -PunityInstallationPath='{0}'", unityPath), path, (type, output) => {
                    commandOutput.AppendLine(output);
                    progress.Update(output.Substring(0, Mathf.Min(output.Length, 100)), 1.0f);
                });
                Debug.Log(commandOutput.ToString());
            });
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
            foreach(var plugin in dirs)
            {
                var pluginDir = Path.GetFileName(plugin);
                var msg = string.Format("Compiling native plugin {0}", pluginDir);
                Debug.Log(msg);
                commandOutput.AppendLine(msg);

                NativeConsole.RunProcess(Path.Combine(path, "build_native_plugin.sh"), string.Format("{0} {1}", pluginDir, AndroidNDKPath), path, (type, output) => commandOutput.AppendLine(output));
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

            var msg = string.Format("Building target '{0}' for SPUnityPlugins '{1}'", target, path);
            Debug.Log(msg);
            commandOutput.AppendLine(msg);

            NativeConsole.RunProcess("xcodebuild", string.Format("-target {0}", target), path, (type, output) => commandOutput.AppendLine(output));
            Debug.Log(commandOutput.ToString());
        }
        #endregion
    }
}
