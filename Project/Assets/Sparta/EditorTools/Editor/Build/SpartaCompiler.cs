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
            Compile(Application.dataPath + "/Sparta/SocialPoint");
        }

        static string InstallationPath
        {
            get
            {
                // Retrived running Unity instance installation path
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

        const string UnityManagedLibrariesPath = "/Unity.app/Contents/Frameworks/Managed/";
        const string UnityMonoPath = "/Unity.app/Contents/Frameworks/Mono/bin/gmcs";
        const string BinariesFolderPath = "/Sparta/Binaries";

        static string Compiler
        {
            get
            {
                return InstallationPath + UnityMonoPath;
            }
        }

        static string OutputPath
        {
            get
            {
                return Application.dataPath + UnityMonoPath;
            }
        }

        static string GetBuildCommand(string dllName, List<string> files, List<string> dependencies)
        {
            var filesList = new StringBuilder();
            foreach(var f in files)
            {
                //filesList.Append(f).Append(" ");
            }

            //FIXME TEST
            filesList.Append("/Users/manuelalvarez/repositories/sp-unity-BaseGame/Project/Assets/Sparta/SocialPoint/AdminPanel/AdminPanelController.cs ");

            var depList = new StringBuilder();
            foreach(var d in dependencies)
            {
                depList.Append("/addmodule:").Append(d).Append(" ");
            }

            var command = string.Format("/t:library /out:OutputPath/{0} {1} {2}", dllName, filesList, depList);
            Debug.Log(command);
            return command;
        }

        static void Compile(string modulePath)
        {
            var path = Application.dataPath;
            var filesToCompile = new List<string>();


            string[] allFiles = Directory.GetFiles(modulePath, "*.cs", SearchOption.AllDirectories);
            foreach(var f in allFiles)
            {   
                filesToCompile.Add(f);
            }

            var dependencies = new List<string>();
            dependencies.Add(InstallationPath + UnityManagedLibrariesPath + "UnityEngine.dll");
            dependencies.Add(InstallationPath + UnityManagedLibrariesPath + "UnityEditor.dll");

            NativeConsole.RunProcess(Compiler, GetBuildCommand("Sparta.dll", filesToCompile, dependencies), path, output => {
                Debug.Log(output);
            });
        }
    }
}