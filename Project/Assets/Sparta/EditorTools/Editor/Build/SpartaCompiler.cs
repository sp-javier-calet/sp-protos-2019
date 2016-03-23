using UnityEngine;
using UnityEditor;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
using SpartaTools.Editor.Utils;
using SpartaTools.Editor.SpartaProject;

namespace SpartaTools.Editor.Build
{
    public class SpartaCompiler : EditorWindow
    {
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
        const string MonoFrameworkLibrariesPath = "Unity.app/Contents/Frameworks/Mono/lib/mono/2.0/";
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

        static string GetBuildCommand(string dllPath, List<string> files, List<string> dependencies, List<string> defines)
        {
            var filesList = new StringBuilder();
            foreach(var f in files)
            {
                filesList.Append(f).Append(" ");
            }

            var depList = new StringBuilder();
            foreach(var d in dependencies)
            {
                if(!File.Exists(d))
                {
                    throw new Exception("Dependency file '" + d + "' not found");
                }
                depList.Append("/reference:\"").Append(d).Append("\" ");
            }

            var defList = new StringBuilder();
            foreach(var d in defines)
            {
                defList.Append("/define:\"").Append(d).Append("\" ");
            }

            var command = string.Format("/target:\"library\" /out:\"{0}\" {1} {2} {3}", dllPath, filesList, depList, defList);
            Debug.Log(command);
            return command;
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
            var MonoLibrariesPath = Path.Combine(InstallationPath, MonoFrameworkLibrariesPath);
            var ExtensionPath = Path.Combine(InstallationPath, UnityExtensionsPath);
            var PlayerPath = Path.Combine(InstallationPath, "PlaybackEngines/iOSSupport/");
            dependencies.Add(Path.Combine(ManagedLibrariesPath, "UnityEngine.dll"));
            dependencies.Add(Path.Combine(MonoLibrariesPath, "System.Xml.Linq.dll"));
            dependencies.Add(Path.Combine(MonoLibrariesPath, "System.Xml.dll"));
            dependencies.Add(Path.Combine(ExtensionPath, "GUISystem/UnityEngine.UI.dll"));


            if(target == BuildTarget.iOS)
            {
                dependencies.Add(Path.Combine(PlayerPath, "UnityEditor.iOS.Extensions.dll"));
                dependencies.Add(Path.Combine(PlayerPath, "UnityEditor.iOS.Extensions.Common.dll"));
                dependencies.Add(Path.Combine(PlayerPath, "UnityEditor.iOS.Extensions.Xcode.dll"));
            }


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
                var attr = File.GetAttributes(depPath);
                if((attr & FileAttributes.Directory) == FileAttributes.Directory)
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


            if(module.Type == Module.ModuleType.Extension)
            {
                // Add compiled sparta core dll
                dependencies.Add(GetTempDllPathForModule("Sparta Core", target, editorAssembly));

                var modules = Project.GetModules(Application.dataPath);
                var core = modules["Sparta Core"];
                // Dependencies
                foreach(var dependency in core.Dependencies)
                {
                    var depPath = Path.Combine(Application.dataPath + "/..", dependency);
                    var attr = File.GetAttributes(depPath);
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
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
            }
                

            // Defines
            var defines = new List<string>();
            defines.Add("UNITY_5");
            defines.Add("UNITY_5_3");

            if(editorAssembly)
            {
                defines.Add("UNITY_EDITOR");
            }

            if(target == BuildTarget.Android)
            {
                defines.Add("UNITY_ANDROID");
            }
            else
            if(target == BuildTarget.iOS)
            {
                defines.Add("UNITY_IOS");
                defines.Add("UNITY_IPHONE"); // For old code. 
            }

            if(filesToCompile.Count == 0)
            {
                Debug.LogWarning("No files to compile");
                return;
            }
                
            try
            {
                var dllPath = GetTempDllPathForModule(module.Name, target, editorAssembly);
                int code = NativeConsole.RunProcess(Compiler, GetBuildCommand(dllPath, filesToCompile, dependencies, defines), path, output => {
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

        static string GetTempDllPathForModule(string moduleName, BuildTarget target, bool editorAssembly)
        {
            var dllName = moduleName.Replace(" ", "").Replace("/", "_") + "_" + target + (editorAssembly ? "-Editor" : "") + ".dll";
            return Path.Combine(Application.dataPath, Path.Combine(BinariesFolderPath, dllName));
        }

        #region Draw GUI

        Dictionary<string, Module> _modules;

        void OnGUI()
        {
            if(_modules == null)
            {
                _modules = Project.GetModules(Application.dataPath);
            }

            foreach(var module in _modules.Values)
            {
                if(GUILayout.Button("Compile " + module.Name + " for Android"))
                {
                    Compile(module, BuildTarget.Android, false);
                }

                if(GUILayout.Button("Compile " + module.Name + " for Android - Editor"))
                {
                    Compile(module, BuildTarget.Android, true);
                }

                if(GUILayout.Button("Compile " + module.Name + " for iOS"))
                {
                    Compile(module, BuildTarget.iOS, false);
                }

                if(GUILayout.Button("Compile " + module.Name + " for iOS - Editor"))
                {
                    Compile(module, BuildTarget.iOS, true);
                }
            }
        }

        #endregion
    }
}