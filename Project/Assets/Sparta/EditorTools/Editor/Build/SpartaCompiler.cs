using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.Build
{
    public static class SpartaCompiler
    {
        [MenuItem("Sparta/Build/Sparta libraries", false, 001)]
        public static void CompileSparta()
        {
            Compile(Path.Combine(Application.dataPath, "../Sources/Sparta/SocialPoint"), true);
        }

        static string InstallationPath
        {
            get
            {
                // Retrived running Unity instance installation path
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

        const string UnityExtensionsPath = "Unity.app/Contents/UnityExtensions/Unity/";
        const string UnityManagedLibrariesPath = "Unity.app/Contents/Frameworks/Managed/";
        const string UnityMonoPath = "Unity.app/Contents/Frameworks/Mono/bin/gmcs";
        const string BinariesFolderPath = "Sparta/Binaries";

        static string Compiler
        {
            get
            {
                return Path.Combine(InstallationPath, UnityMonoPath);
            }
        }

        static string OutputPath
        {
            get
            {
                return Path.Combine(Application.dataPath, UnityMonoPath);
            }
        }

        static string GetBuildCommand(string dllName, List<string> files, List<string> dependencies, List<string> defines)
        {
            var filesList = new StringBuilder();
            foreach(var f in files)
            {
                filesList.Append(f).Append(" ");
            }

            var depList = new StringBuilder();
            foreach(var d in dependencies)
            {
                depList.Append("/reference:\"").Append(d).Append("\" ");
            }

            var defList = new StringBuilder();
            foreach(var d in defines)
            {
                defList.Append("/define:\"").Append(d).Append("\" ");
            }

            var command = string.Format("/t:\"library\" /out:\"{0}\" {1} {2} {3}", Path.Combine(Application.dataPath, Path.Combine(BinariesFolderPath, dllName)), filesList, depList, defList);
            Debug.Log(command);
            return command;
        }

        static void Compile(string modulePath, bool editorAssembly)
        {
            var path = Application.dataPath;
            var filesToCompile = new List<string>();

            // Assembly files
            string[] allFiles = Directory.GetFiles(modulePath, "*.cs", SearchOption.AllDirectories);
            foreach(var f in allFiles)
            {   
                if(!f.Contains("/Editor/"))
                {
                    filesToCompile.Add(f);
                }
            }

            // Binary ependencies
            var dependencies = new List<string>();
            var ManagedLibrariesPath = Path.Combine(InstallationPath, UnityManagedLibrariesPath);
            var ExtensionPath = Path.Combine(InstallationPath, UnityExtensionsPath);
            dependencies.Add(Path.Combine(ManagedLibrariesPath, "UnityEngine.dll"));
            dependencies.Add(Path.Combine(ExtensionPath, "GUISystem/UnityEngine.UI.dll"));
            if(editorAssembly)
            {
                dependencies.Add(Path.Combine(ExtensionPath, "UnityEditor.dll"));
            }

            string[] internalDependencies = Directory.GetFiles(modulePath, "*.dll", SearchOption.AllDirectories);
            foreach(var d in internalDependencies)
            {   
                if(!d.Contains("/Editor/") || editorAssembly)
                {
                    dependencies.Add(d);
                }
            }

            // Defines
            var defines = new List<string>();
            defines.Add("UNITY_5_3");

            if(editorAssembly)
            {
                defines.Add("UNITY_EDITOR");
            }
            
            //defines.Add("UNITY_ANDROID");
            defines.Add("UNITY_IOS");

            try
            {
                int code = NativeConsole.RunProcess(Compiler, GetBuildCommand("Sparta.dll", filesToCompile, dependencies, defines), path, output => {
                    Debug.Log(output);
                });

                if(code != 0)
                {
                    Debug.LogError("Error while compiling library");
                }
            }
            catch(System.ComponentModel.Win32Exception e)
            {
                var v = e.Message;

                Debug.LogError(e);
            }
        }
    }
}