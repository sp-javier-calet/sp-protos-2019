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
    public class ModuleCompiler
    {
        const string UnityExtensionsPath = "Unity.app/Contents/UnityExtensions/Unity/";
        const string MonoFrameworkLibrariesPath = "Unity.app/Contents/Frameworks/Mono/lib/mono/2.0/";
        const string UnityManagedLibrariesPath = "Unity.app/Contents/Frameworks/Managed/";
        const string UnityMonoPath = "Unity.app/Contents/Frameworks/Mono/bin/gmcs";
        const string BinariesFolderPath = "Temp/Sparta/Binaries";

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

        const string ScriptFilePattern = "*.cs";
        const string LibraryFilePattern = "*.dll";
        const string BuildCommandPattern = "/target:\"library\" /out:\"{0}\" {1} {2} {3} {4}";
        const string LibraryOption = "/lib:\"";
        const string DefineOption = "/define:\"";
        const string ReferenceOption = "/reference:\"";
        const string CloseQuote = "\" ";
        const string Space = " ";

        const string EditorFilter = "Editor";
        const string TestsFilter = "Tests";

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
                throw new DependencyNotFoundException("Referenced file '" + reference + "' not found");
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
                    files.Append(file).Append(Space);
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

            // Launch mono compiler
            try
            {   
                int code = NativeConsole.RunProcess(Compiler, buildCommand, Application.dataPath, (type, output) => {
                    if(type == NativeConsole.OutputType.Error)
                    {   
                        _logContent.AppendLine(type.ToString()).AppendLine(output);
                        HasErrorsLogs = true;
                    }
                    else
                    {
                        _logContent.AppendLine(output);
                    }
                });
                
                if(code != 0)
                {
                    _logContent.AppendLine(string.Format("Error while compiling library. Exit code {0}", code));
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
            compiler.AddLibraryPath(Path.Combine(InstallationPath, UnityManagedLibrariesPath));
            compiler.AddLibraryPath(Path.Combine(InstallationPath, MonoFrameworkLibrariesPath));
            compiler.AddLibraryPath(Path.Combine(InstallationPath, UnityExtensionsPath));

            compiler.AddReference("UnityEngine.dll");
            compiler.AddReference("System.Xml.Linq.dll");
            compiler.AddReference("System.Xml.dll");
            compiler.AddReference("GUISystem/UnityEngine.UI.dll");

            // FIXME Read default symbols from unity
            compiler.AddDefinedSymbol("UNITY_5");
            compiler.AddDefinedSymbol("UNITY_5_3");

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

        #endregion

        #region Compiler Configurations

        public interface ICompilerConfiguration
        {
            void Configure(ModuleCompiler compiler);
        }

        class AndroidPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddDefinedSymbol("UNITY_ANDROID");
            }
        }

        class IosPlatformConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                var PlayerPath = Path.Combine(InstallationPath, "PlaybackEngines/iOSSupport/");
                compiler.AddLibraryPath(PlayerPath);

                compiler.AddReference("UnityEditor.iOS.Extensions.dll");
                compiler.AddReference("UnityEditor.iOS.Extensions.Common.dll");
                compiler.AddReference("UnityEditor.iOS.Extensions.Xcode.dll");

                compiler.AddDefinedSymbol("UNITY_IOS");
                compiler.AddDefinedSymbol("UNITY_IPHONE");
            }
        }

        class EditorConfiguration : ICompilerConfiguration
        {
            public void Configure(ModuleCompiler compiler)
            {
                compiler.AddReference("UnityEditor.dll");

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
                }
            }
        }

        class ExtensionModuleConfiguration : ICompilerConfiguration
        {
            BuildTarget _target;
            bool _editorAssembly;

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