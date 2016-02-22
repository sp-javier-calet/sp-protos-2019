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
        static ProgressHandler _handler;

        static string SourcesDirectoryPath
        {
            get
            {
                return Path.Combine(Application.dataPath, "../Sources");
            }
        }

        #region Editor options

        [MenuItem("Window/Sparta/Build/Android Plugins", false, 101)]
        public static void CompileAndroid()
        {
            var commandOutput = new StringBuilder();
            var path = Path.Combine(SourcesDirectoryPath, "Android/sp_unity_plugins");

            AsyncProcess.Start(progress => {

                NativeConsole.RunProcess(path + "/gradlew", "generateUnityPlugin", path, output => {
                    commandOutput.AppendLine(output);
                    progress.Update(output.Substring(0, Mathf.Min(output.Length, 100)), 1.0f);
                });
                Debug.Log(commandOutput.ToString());
            });
        }

        [MenuItem("Window/Sparta/Build/Android Native Plugins", false, 102)]
        public static void CompileAndroidNative()
        {
            var commandOutput = new StringBuilder();
            var path = Path.Combine(SourcesDirectoryPath, "Android/sp_unity_native_plugins/sp_unity_curl"); // TODO Iterates and build
            NativeConsole.RunProcess(path + "/build_plugin.sh", string.Empty, path, output => commandOutput.AppendLine(output));
            Debug.Log(commandOutput.ToString());
        }

        [MenuItem("Window/Sparta/Build/iOS Plugins", false, 201)]
        public static void CompileIOS()
        {
            var commandOutput = new StringBuilder();
            var path = Path.Combine(SourcesDirectoryPath, "Ios/sp_unity_plugins"); // TODO Apple folders
            NativeConsole.RunProcess("xcodebuild", "-target PluginDependencies", path, output => commandOutput.AppendLine(output));
            Debug.Log(commandOutput.ToString());
        }

        [MenuItem("Window/Sparta/Build/tvOS Plugins", false, 202)]
        public static void CompileTVOS()
        {
            // TODO Compile Xcode project
        }

        [MenuItem("Window/Sparta/Build/OSX Plugins", false, 202)]
        public static void CompileOSX()
        {
            // TODO Compile Xcode project
        }

        [MenuItem("Window/Sparta/Build/Build All", false, 500)]
        public static void CompileAll()
        {
            CompileAndroid();
            CompileAndroidNative();
            CompileIOS();
            CompileTVOS();
            CompileOSX();
        }

        #endregion
    }
}
