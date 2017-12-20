using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpartaTools.Editor.SpartaProject;
using SpartaTools.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Build
{
    public class ModuleCompiler
    {
        #region Unity Engine Libraries paths

        // Internal Unity paths may be different depending on the Unity version and the current platform

        static readonly string[] UnityExtensionsAvailablePaths = {
            "Unity.app/Contents/UnityExtensions/Unity/"
        };

        static readonly string[] MonoFrameworkLibrariesAvailablePaths = { 
            "Unity.app/Contents/Frameworks/Mono/lib/mono/2.0/",
            "Unity.app/Contents/Mono/lib/mono/2.0/" 
        };

        static readonly string[] UnityManagedLibrariesAvailablePaths = {
            "Unity.app/Contents/Frameworks/Managed/",
            "Unity.app/Contents/Managed/" 
        };

        static readonly string[] UnityMonoAvailablePaths = { 
            "Unity.app/Contents/Frameworks/Mono/bin/gmcs", 
            "Unity.app/Contents/Mono/bin/gmcs" 
        };

        static string _unityExtensionsPath;

        static string UnityExtensionsPath
        {
            get
            {
                if(string.IsNullOrEmpty(_unityExtensionsPath))
                {
                    _unityExtensionsPath = GetCurrentLibraryPath(UnityExtensionsAvailablePaths);
                }
                return _unityExtensionsPath;
            }
        }

        static string _monoFrameworkLibrariesPath;

        static string MonoFrameworkLibrariesPath
        {
            get
            {
                if(string.IsNullOrEmpty(_monoFrameworkLibrariesPath))
                {
                    _monoFrameworkLibrariesPath = GetCurrentLibraryPath(MonoFrameworkLibrariesAvailablePaths);
                }
                return _monoFrameworkLibrariesPath;
            }
        }

        static string _unityManagedLibrariesPath;

        static string UnityManagedLibrariesPath
        {
            get
            {
                if(string.IsNullOrEmpty(_unityManagedLibrariesPath))
                {
                    _unityManagedLibrariesPath = GetCurrentLibraryPath(UnityManagedLibrariesAvailablePaths);
                }
                return _unityManagedLibrariesPath;
            }
        }

        static string _unityMonoPath;

        static string UnityMonoPath
        {
            get
            {
                if(string.IsNullOrEmpty(_unityMonoPath))
                {
                    _unityMonoPath = GetCurrentLibraryPath(UnityMonoAvailablePaths);
                }

                return _unityMonoPath;
            }
        }

        static string GetCurrentLibraryPath(string[] availablePaths)
        {
            string existing = null;
            for(var i = 0; i < availablePaths.Length; ++i)
            {
                var path = Path.Combine(InstallationPath, availablePaths[i]);
                if(File.Exists(path) || Directory.Exists(path))
                {
                    existing = path;
                    break;
                }
            }

            if(string.IsNullOrEmpty(existing))
            {
                throw new CompilerConfigurationException("Required Unity Library path not found");
            }
            return existing;
        }

        #endregion

        static string InstallationPath
        {
            get
            {
                // Retrived running Unity instance installation path
                return Path.GetDirectoryName(EditorApplication.applicationPath);
            }
        }

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
                return Path.Combine(Project.BasePath, BinariesFolderPath);
            }
        }

        class FilterData
        {
            public string Filter;
            public string Name;

            bool? _enabled;

            public bool Enabled
            {
                get
                {
                    // Enabled by default.
                    return !_enabled.HasValue || _enabled.Value;
                }
                set
                {
                    if(_enabled.HasValue && _enabled.Value != value)
                    {
                        throw new CompilerConfigurationException(string.Format("Conflicting filter configuration. {0} filter already defined as {1}", Name, _enabled.Value));
                    }
                    _enabled = value;
                }
            }

            public FilterData(string name)
            {
                Name = name;
                Filter = string.Format("/{0}/", name);
                _enabled = null;
            }
        }

        // Name of the main Core library module. Workaround for module dependencies.
        const string SpartaCoreModule = "Sparta Core";
        const string BinariesFolderPath = "Temp/Sparta/Assemblies";
        const string RspFile = "build_command.rsp";

        const string ScriptFilePattern = "*.cs";
        const string LibraryFilePattern = "*.dll";
        const string BuildCommandPattern = "/target:\"library\" /out:\"{0}\" {1} {2} {3} {4}";
        const string LibraryOption = "/lib:\"";
        const string DefineOption = "/define:\"";
        const string ReferenceOption = "/reference:\"";
        const string CloseQuote = "\" ";
        const string Space = " ";
        const string SingleQuote = "\'";
        const string DoubleQuote = "\"";

        const string EditorFilter = "Editor";
        const string TestsFilter = "Tests";
        const string StandaloneFilter = "Standalone/Photon";
        const string PlatformWSAFilter = "Plugins/WSA";
        const string PlatformUwpFilter = "Plugins/Uwp";
        const string PlatformAndroidFilter = "Plugins/Android";
        const string PlatformIosFilter = "Plugins/Ios";

        List<string> _files;
        List<string> _references;
        StringBuilder _libraries;
        StringBuilder _defines;
        StringBuilder _logContent;
        string _moduleName;
        bool? _editorAssembly;
        BuildTarget _target;
        Dictionary<string, FilterData> _filters;

        public bool HasErrorsLogs { get; private set; }

        public bool Compiled { get; private set; }

        public bool HasWarnings
        {
            get
            {
                return Compiled && HasErrorsLogs;
            }
        }

        ModuleCompiler(string name)
        {
            _moduleName = name;
            _files = new List<string>();
            _references = new List<string>();
            _libraries = new StringBuilder();
            _defines = new StringBuilder();
            _logContent = new StringBuilder();
            _target = BuildTarget.iOS;
            _filters = new Dictionary<string, FilterData>();

            // Default filters
            _filters.Add(EditorFilter, new FilterData(EditorFilter));
            _filters.Add(TestsFilter, new FilterData(TestsFilter));
            _filters.Add(PlatformWSAFilter, new FilterData(PlatformWSAFilter));
            _filters.Add(PlatformUwpFilter, new FilterData(PlatformUwpFilter));
            _filters.Add(PlatformAndroidFilter, new FilterData(PlatformAndroidFilter));
            _filters.Add(PlatformIosFilter, new FilterData(PlatformIosFilter));
            _filters.Add(StandaloneFilter, new FilterData(StandaloneFilter));

            // Initialize log entry
            _logContent.Append(Path.GetFileName(name)).AppendLine(" module compilation");
        }

        public static ModuleCompiler Create(string name)
        {
            return new ModuleCompiler(name);
        }

        public ModuleCompiler ConfigureAs(ICompilerConfiguration configuration)
        {
            configuration.Configure(this);
            return this;
        }

        public ModuleCompiler SetTarget(BuildTarget target)
        {
            _target = target;
            return this;
        }

        public ModuleCompiler SetEditorAssembly(bool isEditorAssembly)
        {
            if(_editorAssembly.HasValue && _editorAssembly != isEditorAssembly)
            {
                throw new CompilerConfigurationException(string.Format("Conflicting EditorAssembly configuration. Already defined as {0}", _editorAssembly));
            }

            _editorAssembly = isEditorAssembly;
            return this;
        }

        public ModuleCompiler EnableFilter(string filter)
        {
            FilterData data;
            if(_filters.TryGetValue(filter, out data))
            {
                data.Enabled = true;
            }
            else
            {
                data = new FilterData(filter);
                data.Enabled = true;
                _filters.Add(filter, data);
            }
            return this;
        }

        public ModuleCompiler DisableFilter(string filter)
        {
            FilterData data;
            if(_filters.TryGetValue(filter, out data))
            {
                data.Enabled = false;
            }
            else
            {
                data = new FilterData(filter);
                data.Enabled = false;
                _filters.Add(filter, data);
            }
            return this;
        }

        public ModuleCompiler AddFile(string file)
        {
            _files.Add(file);
            return this;
        }

        public ModuleCompiler AddFile(IList<string> files)
        {
            foreach(var file in files)
            {
                AddFile(file);
            }
            return this;
        }

        public ModuleCompiler AddLibraryPath(string path)
        {
            _libraries.Append(LibraryOption).Append(path).Append(CloseQuote);
            return this;
        }

        public ModuleCompiler AddLibraryPath(IList<string> paths)
        {
            foreach(var path in paths)
            {
                AddLibraryPath(path);
            }
            return this;
        }

        public ModuleCompiler AddReference(string reference)
        {
            if(Path.IsPathRooted(reference) && !File.Exists(reference))
            {
                throw new DependencyNotFoundException("Referenced file '" + reference + "' not found", Path.GetFileName(reference));
            }

            _references.Add(reference);
            return this;
        }

        public ModuleCompiler AddReference(IList<string> references)
        {
            foreach(var reference in references)
            {
                AddReference(reference);
            }
            return this;
        }

        public ModuleCompiler AddDefinedSymbol(string define)
        {
            _defines.Append(DefineOption).Append(define).Append(CloseQuote);
            return this;
        }

        public ModuleCompiler AddDefinedSymbol(IList<string> defines)
        {
            foreach(var define in defines)
            {
                AddDefinedSymbol(define);
            }
            return this;
        }

        bool FilterPath(string path)
        {
            foreach(var filter in _filters.Values)
            {
                if(filter.Enabled && path.Contains(filter.Filter))
                {
                    return true;
                }
            }

            return false;
        }

        string GetFilteredFiles()
        {
            var files = new StringBuilder();
            foreach(var file in _files)
            {
                if(!FilterPath(file))
                {
                    files.Append(SingleQuote);
                    files.Append(file);
                    files.Append(SingleQuote);
                    files.Append(Space);
                }
            }
            return files.ToString();
        }

        string GetFilteredReferences()
        {
            var references = new StringBuilder();
            foreach(var reference in _references)
            {
                if(!FilterPath(reference))
                {
                    references.Append(ReferenceOption).Append(reference).Append(CloseQuote);
                }
            }
            return references.ToString();
        }

        static string BuildCommandToResponseFileCommand(string buildCommand)
        {
            string responseFileFullPath = Path.Combine(OutputPath, RspFile);
            var fileStream = File.CreateText(responseFileFullPath);
            fileStream.Write(buildCommand);
            fileStream.Flush();
            fileStream.Close();

            return string.Format("{0}@{1}{2}", DoubleQuote, responseFileFullPath, DoubleQuote);
        }

        public void Compile()
        {
            var filteredFiles = GetFilteredFiles();
            var filteredReferences = GetFilteredReferences();

            // Check if there are files to compile.
            if(string.IsNullOrEmpty(filteredFiles))
            {
                _logContent.AppendLine("No files to compile");
                throw new EmptyModuleException("No files to compile");
            }

            // Generate build command
            var dllPath = GetTempDllPathForModule(_moduleName, _target, _editorAssembly.HasValue && _editorAssembly.Value);

            // Create output directory if needed
            var dir = Path.GetDirectoryName(dllPath);
            Directory.CreateDirectory(dir);

            var buildCommand = string.Format(BuildCommandPattern, dllPath, _libraries, filteredReferences, _defines, filteredFiles);

            buildCommand = BuildCommandToResponseFileCommand(buildCommand);

            // Launch mono compiler
            try
            {   
                var result = NativeConsole.RunProcess(Compiler, buildCommand, Application.dataPath);

                _logContent.AppendLine(result.Output);
                HasErrorsLogs = result.HasError;

                if(result.Code != 0)
                {
                    _logContent.AppendLine(string.Format("Error while compiling library. Exit code {0}", result.Code));
                    throw new CompilerErrorException(_logContent.ToString());
                }

                _logContent.AppendLine("Compilation success");
                Compiled = true;
            }
            catch(System.ComponentModel.Win32Exception e)
            {
                _logContent.AppendLine(e.ToString());
                throw new CompilerErrorException(_logContent.ToString());
            }
        }

        public string GetLog()
        {
            return _logContent.ToString();
        }


        #region Static methods

        public class CompilationResult
        {
            public bool Success;
            public string Log;

            public CompilationResult(bool success, string log)
            {
                Success = success;
                Log = log;
            }
        }

        public static CompilationResult Compile(Module module, BuildTarget target, bool editorAssembly)
        {
            var compiler = ModuleCompiler.Create(module.Name)
                .SetTarget(target)
                .SetEditorAssembly(editorAssembly);

            // Global configuration for Unity
            compiler.AddLibraryPath(UnityManagedLibrariesPath);
            compiler.AddLibraryPath(MonoFrameworkLibrariesPath);
            compiler.AddLibraryPath(UnityExtensionsPath);

            compiler.AddReference("UnityEngine.dll");
            compiler.AddReference("System.Xml.Linq.dll");
            compiler.AddReference("System.Xml.dll");
            compiler.AddReference("GUISystem/UnityEngine.UI.dll");
            compiler.AddReference("GUISystem/Editor/UnityEditor.UI.dll");
            compiler.AddReference("Networking/UnityEngine.Networking.dll");

            var activeCompilationDefines = EditorUserBuildSettings.activeScriptCompilationDefines;
            for(int i = 0, activeCompilationDefinesLength = activeCompilationDefines.Length; i < activeCompilationDefinesLength; i++)
            {
                var item = activeCompilationDefines[i];
                if(item.StartsWith("UNITY_5"))
                {
                    compiler.AddDefinedSymbol(item);
                }
            }

            /* Platform configuration */
            if(editorAssembly)
            {
                compiler.ConfigureAs(new EditorConfiguration());
            }

            switch(target)
            {
            case BuildTarget.Android:
                compiler.ConfigureAs(new AndroidPlatformConfiguration());
                break;
            case BuildTarget.iOS:
                compiler.ConfigureAs(new IosPlatformConfiguration());
                break;
            case BuildTarget.tvOS:
                compiler.ConfigureAs(new TvosPlatformConfiguration());
                break;
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
                compiler.ConfigureAs(new StandAloneLinuxPlatformConfiguration());
                break;
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                compiler.ConfigureAs(new StandAloneWindowsPlatformConfiguration());
                break;
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneOSXIntel64:
            #if UNITY_2017
            case BuildTarget.StandaloneOSX:
            #else
            case BuildTarget.StandaloneOSXUniversal:
            #endif
                compiler.ConfigureAs(new StandAloneOSXPlatformConfiguration());
                break;
            default:
                throw new CompilerConfigurationException(string.Format("Unsupported platform {0}", target));
            }

            // If it is an extension module, it always depends on Core
            if(module.Type == Module.ModuleType.Extension)
            {
                compiler.ConfigureAs(new ExtensionModuleConfiguration(target, editorAssembly));
            }

            compiler.ConfigureAs(new ModuleConfiguration(module));

            compiler.Compile();
            return new CompilationResult(!compiler.HasWarnings, compiler.GetLog());
        }

        static string GetTempDllPathForModule(string moduleName, BuildTarget target, bool editorAssembly)
        {
            var dllName = moduleName.Replace(" ", "").Replace("/", "_") + "_" + target + (editorAssembly ? "-Editor" : "") + ".dll";
            return Path.Combine(OutputPath, dllName);
        }

        public static void ClearOutputPath()
        {
            if(Directory.Exists(OutputPath))
            {
                Directory.Delete(OutputPath, true);
            }
        }

        #endregion

        #region Compiler Configurations

        public interface ICompilerConfiguration
        {
            void Configure(ModuleCompiler compiler);
        }

        class StandAloneLinuxPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddDefinedSymbol("UNITY_STANDALONE");
                compiler.AddDefinedSymbol("UNITY_STANDALONE_LINUX");
            }
        }

        class StandAloneWindowsPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddDefinedSymbol("UNITY_STANDALONE");
                compiler.AddDefinedSymbol("UNITY_STANDALONE_WIN");
            }
        }

        class StandAloneOSXPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddDefinedSymbol("UNITY_STANDALONE");
                compiler.AddDefinedSymbol("UNITY_STANDALONE_OSX");
            }
        }

        class AndroidPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddDefinedSymbol("UNITY_ANDROID");
                compiler.DisableFilter(PlatformAndroidFilter);
            }
        }

        class IosPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                var PlayerPath = Path.Combine(InstallationPath, "PlaybackEngines/iOSSupport/");

                if(!Directory.Exists(PlayerPath))
                {
                    var error = string.Format("PlayerPath: '{0}' does not exists.", PlayerPath);
                    throw new CompilerErrorException(error);
                }

                compiler.AddLibraryPath(PlayerPath);

                compiler.AddReference("UnityEditor.iOS.Extensions.dll");
                compiler.AddReference("UnityEditor.iOS.Extensions.Common.dll");
                compiler.AddReference("UnityEditor.iOS.Extensions.Xcode.dll");

                compiler.AddDefinedSymbol("UNITY_IOS");
                compiler.AddDefinedSymbol("UNITY_IPHONE");
                compiler.AddDefinedSymbol("NO_GPGS");
                compiler.DisableFilter(PlatformIosFilter);
            }
        }

        class TvosPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                var PlayerPath = Path.Combine(InstallationPath, "PlaybackEngines/AppleTVSupport/");

                if(!Directory.Exists(PlayerPath))
                {
                    var error = string.Format("PlayerPath: '{0}' does not exists.", PlayerPath);
                    throw new CompilerErrorException(error);
                }

                compiler.AddLibraryPath(PlayerPath);

                compiler.AddDefinedSymbol("UNITY_TVOS");
            }
        }

        class EditorConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddReference("UnityEditor.dll");
                compiler.AddReference("UnityEditor.Graphs.dll");

                compiler.AddDefinedSymbol("UNITY_EDITOR");
                compiler.SetEditorAssembly(true);

                compiler.DisableFilter(EditorFilter);
            }
        }

        class ModuleConfiguration : ICompilerConfiguration
        {
            readonly Module _module;

            public ModuleConfiguration(Module module)
            {
                _module = module;
            }

            public void Configure(ModuleCompiler compiler)
            {
                var modulePath = Path.Combine(Project.BasePath, _module.RelativePath);
                string[] allFiles = Directory.GetFiles(modulePath, ScriptFilePattern, SearchOption.AllDirectories);
                foreach(var f in allFiles)
                {   
                    compiler.AddFile(f);
                }

                string[] internalDependencies = Directory.GetFiles(modulePath, LibraryFilePattern, SearchOption.AllDirectories);
                foreach(var d in internalDependencies)
                {   
                    compiler.AddReference(d);
                }

                foreach(var dependency in _module.Dependencies)
                {
                    var depPath = Path.Combine(Project.BasePath, dependency);
                    var attr = File.GetAttributes(depPath);
                    if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        string[] depFiles = Directory.GetFiles(depPath, ScriptFilePattern, SearchOption.AllDirectories);
                        foreach(var f in depFiles)
                        {
                            compiler.AddFile(f);
                        }

                        string[] libFiles = Directory.GetFiles(depPath, LibraryFilePattern, SearchOption.AllDirectories);
                        foreach(var lib in libFiles)
                        {
                            compiler.AddReference(lib);
                        }
                    }
                    else
                    {
                        // Reference .dll dependencies
                        var file = new FileInfo(depPath);
                        if(file.Extension == ".dll")
                        {
                            compiler.AddReference(depPath);
                        }
                    }
                }
            }
        }

        class ExtensionModuleConfiguration : ICompilerConfiguration
        {
            BuildTarget _target;
            readonly bool _editorAssembly;

            public ExtensionModuleConfiguration(BuildTarget target, bool editorAssembly)
            {
                _target = target;
                _editorAssembly = editorAssembly;
            }

            public void Configure(ModuleCompiler compiler)
            {
                // Add references to compiled libraries
                var modules = Project.GetModules(Project.BasePath);
                foreach(var module in modules.Values)
                {
                    if(module.Type == Module.ModuleType.Core && module.Name.Equals(SpartaCoreModule))
                    {
                        // Add reference to already compiled module
                        compiler.AddReference(GetTempDllPathForModule(module.Name, _target, _editorAssembly));
                       
                        // Library dependencies
                        foreach(var dependency in module.Dependencies)
                        {
                            var depPath = Path.Combine(Project.BasePath, dependency);
                            var attr = File.GetAttributes(depPath);
                            if((attr & FileAttributes.Directory) == FileAttributes.Directory)
                            {
                                string[] libFiles = Directory.GetFiles(depPath, LibraryFilePattern, SearchOption.AllDirectories);
                                foreach(var lib in libFiles)
                                {
                                    compiler.AddReference(lib);
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion
    }
}
