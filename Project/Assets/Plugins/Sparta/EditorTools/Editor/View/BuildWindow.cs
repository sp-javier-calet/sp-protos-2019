using System.IO;
using SpartaTools.Editor.Build;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.View
{
    public static class BuildWindow
    {
        #region Build Player options

        [MenuItem("Sparta/Build/Player/Android Release", false, 181)]
        public static void BuildAndroidRelease()
        {
            AutoBuilder.Build(BuildTarget.Android, BuildSet.ReleaseConfigName);
        }

        [MenuItem("Sparta/Build/Player/Android Debug", false, 182)]
        public static void BuildAndroidDebug()
        {
            AutoBuilder.Build(BuildTarget.Android, BuildSet.DebugConfigName);
        }

        [MenuItem("Sparta/Build/Player/Ios Release", false, 201)]
        public static void BuildIosRelease()
        {
            AutoBuilder.Build(BuildTarget.iOS, BuildSet.ReleaseConfigName);
        }

        [MenuItem("Sparta/Build/Player/Ios Debug", false, 202)]
        public static void BuildIosDebug()
        {
            AutoBuilder.Build(BuildTarget.iOS, BuildSet.DebugConfigName);
        }

        #endregion

        #region Build Plugins options

        [MenuItem("Sparta/Build/Plugins/Android Java Plugins", false, 182)]
        public static void CompileAndroid()
        {
            NativeBuild.CompileAndroid();
        }

        [MenuItem("Sparta/Build/Plugins/Android Native Plugins", false, 183)]
        public static void CompileAndroidNative()
        {
            NativeBuild.CompileAndroidNative();
        }

        [MenuItem("Sparta/Build/Plugins/iOS Plugins", false, 201)]
        public static void CompileIOS()
        {
            NativeBuild.CompileIOS();
        }

        [MenuItem("Sparta/Build/Plugins/tvOS Plugins", false, 202)]
        public static void CompileTVOS()
        {
            NativeBuild.CompileTVOS();
        }

        [MenuItem("Sparta/Build/Plugins/OSX Plugins", false, 203)]
        public static void CompileOSX()
        {
            NativeBuild.CompileOSX();

            // Show prompt to restart editor in order to reload native librariess
            if(EditorUtility.DisplayDialog("Restart required", "Editor must be restarted to apply changes in native plugins", "Restart Now", "Continue without restart"))
            {
                if(UnityEditor.SceneManagement.EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorApplication.OpenProject(Path.Combine(Application.dataPath, ".."));
                }
            }
        }

        [MenuItem("Sparta/Build/Plugins/Build All", false, 220)]
        public static void CompileAll()
        {
            NativeBuild.CompileAll();
        }

        #endregion
    }
}