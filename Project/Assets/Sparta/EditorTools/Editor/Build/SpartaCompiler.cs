using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Utils;
using SpartaTools.Editor.Sync;

namespace SpartaTools.Editor.Build
{
    public class SpartaCompiler : EditorWindow
    {
        [MenuItem("Sparta/Build/Sparta libraries", false, 001)]
        public static void CompileSparta()
        {
            //Compile(Path.Combine(Application.dataPath, "../Sources/Sparta/SocialPoint"), true);
            Compile(Path.Combine(Application.dataPath, "Sparta/SocialPoint"), true);
        }

        [MenuItem("Sparta/Build/Sparta Modules", false, 001)]
        public static void CompileModule()
        {
            EditorWindow.GetWindow(typeof(SpartaCompiler), false, "Modules", true);
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
        //const string BinariesFolderPath = "Sparta/Binaries";
        const string BinariesFolderPath = "../Tmp/Sparta/Binaries";

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
                dependencies.Add(Path.Combine(ManagedLibrariesPath, "UnityEditor.dll"));
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

        static void Compile(Module module, BuildTarget target, bool editorAssembly)
        {
            var path = Application.dataPath;
            var filesToCompile = new List<string>();
            var modulePath = Path.Combine(Application.dataPath, module.RelativePath);

            // Assembly files
            string[] allFiles = Directory.GetFiles(modulePath, "*.cs", SearchOption.AllDirectories);
            foreach(var f in allFiles)
            {   
                if(!f.Contains("/Tests/") && (!f.Contains("/Editor/") || editorAssembly))
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
                dependencies.Add(Path.Combine(ManagedLibrariesPath, "UnityEditor.dll"));
            }

            string[] internalDependencies = Directory.GetFiles(modulePath, "*.dll", SearchOption.AllDirectories);
            foreach(var d in internalDependencies)
            {   
                if(!d.Contains("/Tests/") && (!d.Contains("/Editor/") || editorAssembly))
                {
                    dependencies.Add(d);
                }
            }

            // Dependencies
            foreach(var dependency in module.Dependencies)
            {
                var depPath = Path.Combine(Application.dataPath + "/..", dependency);
                //if(Directory.Exists(depPath))
                {
                    string[] depFiles = Directory.GetFiles(depPath, "*.cs", SearchOption.AllDirectories);
                    foreach(var f in depFiles)
                    {   
                        if(!f.Contains("/Editor/") || editorAssembly)
                        {
                            filesToCompile.Add(f);
                        }
                    }

                    string[] libFiles = Directory.GetFiles(depPath, "*.dll", SearchOption.AllDirectories);
                    foreach(var lib in libFiles)
                    {
                        if(!lib.Contains("/Editor/") || editorAssembly)
                        {
                            dependencies.Add(lib);
                        }
                    }
                }
            }

            // Defines
            var defines = new List<string>();
            defines.Add("UNITY_5_3");

            if(editorAssembly)
            {
                defines.Add("UNITY_EDITOR");
            }

            if(target == BuildTarget.Android)
            {
                defines.Add("UNITY_ANDROID");
            }
            else if(target == BuildTarget.iOS)
            {
                defines.Add("UNITY_IOS");
                defines.Add("UNITY_IPHONE"); // For old code. 
            }

            try
            {
                var dllName = module.Name.Replace(" ", "") + "_" + target + (editorAssembly? "-Editor" : "" ) +".dll";
                int code = NativeConsole.RunProcess(Compiler, GetBuildCommand(dllName, filesToCompile, dependencies, defines), path, output => {
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

        #region Draw GUI

        Dictionary<string, Module> _modules;

        void OnGUI()
        {
            if(_modules == null)
            {
                _modules = Sync.SyncTools.GetProjectModules(Application.dataPath);
            }

            foreach(var module in _modules.Values)
            {
                if(GUILayout.Button("Compile " + module.Name + " for Android"))
                {
                    Compile(module, BuildTarget.Android, false);
                    Compile(module, BuildTarget.Android, true);
                }

                if(GUILayout.Button("Compile " + module.Name + " for iOS"))
                {
                    Compile(module, BuildTarget.iOS, false);
                    Compile(module, BuildTarget.iOS, true);
                }
            }
        }

        #endregion
    }
}