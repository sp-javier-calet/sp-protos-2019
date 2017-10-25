using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SpartaTools.Editor.Utils;
using SpartaTools.iOS.Xcode;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Xcode project wrapper class.
    /// </summary>
    public class XcodeProject
    {
        public readonly string ProjectPath;
        public readonly string ProjectRootPath;
        public readonly string PbxPath;
        public readonly string BaseAppPath;

        readonly PBXProject _project;
        readonly XcodeEditorInternal _editor;

        /// <summary>
        /// XcodeProject constructor.
        /// </summary>
        /// <param name="xcodeProjectPath">Full Path to the '.xcodeproj' file</param>
        /// <param name="baseAppPath">Full base path of the current client application (i.e. the Unity's data path)</param>
        public XcodeProject(string xcodeProjectPath, string baseAppPath)
        {
            if(!Path.IsPathRooted(xcodeProjectPath) || !Path.IsPathRooted(baseAppPath))
            {
                throw new InvalidDataException("XcodeProject requires full project paths");
            }

            // Initialize paths
            ProjectPath = xcodeProjectPath;
            ProjectRootPath = Path.GetDirectoryName(xcodeProjectPath);
            PbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");
            BaseAppPath = baseAppPath;

            // Open Pbx file
            _project = new PBXProject();
            _project.ReadFromFile(PbxPath);

            // Create a default editor for the current project
            _editor = new XcodeEditorInternal(this, _project);
        }

        public XCodeProjectEditor Editor
        {
            get
            {
                return _editor;
            }
        }

        #region Internal Xcode Project Editor Implementation

        class XcodeEditorInternal : XCodeProjectEditor
        {
            #region Mods Editors and accessors

            readonly Dictionary<Type, IModEditor> ModEditors = new Dictionary<Type, IModEditor>() {
                { typeof(HeaderPathsModEditor),         new HeaderPathsModEditor()         },
                { typeof(LibraryPathsModEditor),        new LibraryPathsModEditor()        },
                { typeof(CopyFileModEditor),            new CopyFileModEditor()            },
                { typeof(FilesModEditor),               new FilesModEditor()               },
                { typeof(FolderModEditor),              new FolderModEditor()              },
                { typeof(LibraryModEditor),             new LibraryModEditor()             },
                { typeof(FrameworkModEditor),           new FrameworkModEditor()           },
                { typeof(BuildSettingsModEditor),       new BuildSettingsModEditor()       },
                { typeof(LocalizationModEditor),        new LocalizationModEditor()        },
                { typeof(PListModEditor),               new PListModEditor()               },
                { typeof(ShellScriptModEditor),         new ShellScriptModEditor()         },
                { typeof(SystemCapabilityModEditor),    new SystemCapabilityModEditor()    },
                { typeof(ProvisioningModEditor),        new ProvisioningModEditor()        },
                { typeof(KeychainAccessGroupModEditor), new KeychainAccessGroupModEditor() },
            };

            public T GetEditor<T>() where T : IModEditor
            {
                return ModEditors[typeof(T)] as T;
            }

            #endregion

            public const string ProjectPathVar = "XCODE_PROJECT_PATH";
            public const string ProjectRootVar = "XCODE_ROOT_PATH";
            public const string CurrentRootPath = "ROOT_PATH";
            public const string ModsBasePath = "Sparta/Mods";
            public const string DefaultPListFileName = "Info.plist";

            public readonly XcodeProject Project;
            public readonly PBXProject Pbx;
            public readonly string DefaultTargetGuid;

            readonly IDictionary<string, string> _projectVariables;

            public XcodeEditorInternal(XcodeProject project, PBXProject pbx)
            {
                Project = project;
                Pbx = pbx;
                DefaultTargetGuid = Pbx.TargetGuidByName(PBXProject.GetUnityTargetName());

                _projectVariables = new Dictionary<string, string> {
                    { ProjectPathVar,  project.ProjectPath     }, // Path to the .xcodeproj file
                    { ProjectRootVar,  project.ProjectRootPath }, // Path to the xcode project folder
                    { CurrentRootPath, project.BaseAppPath     }, // Path to the Unity project root
                    { SpartaPaths.SourcesVariable, SpartaPaths.SourcesDirAbsolute       },
                    { SpartaPaths.BinariesVariable, SpartaPaths.BinariesDirAbsolute     },
                    { SpartaPaths.CoreVariable, SpartaPaths.CoreDirAbsolute             },
                    { SpartaPaths.ExternalVariable, SpartaPaths.ExternalDirAbsolute     },
                    { SpartaPaths.ExtensionsVariable, SpartaPaths.ExtensionsDirAbsolute }
                };
            }

            public string ReplaceProjectVariables(string originalPath)
            {
                return SpartaPaths.ReplaceProjectVariables(originalPath, _projectVariables);
            }

            #region XCodeProjectEditor interface methods

            public void AddHeaderSearchPath(string path)
            {
                GetEditor<HeaderPathsModEditor>().Add(path);
            }

            public void AddLibrarySearchPath(string path)
            {
                GetEditor<LibraryPathsModEditor>().Add(path);
            }

            public void CopyFile(string basePath, string src, string dst)
            {
                GetEditor<CopyFileModEditor>().Add(basePath, src, dst);
            }

            public void AddFile(string path)
            {
                AddFile(path, new string[0]);
            }

            public void AddFile(string path, string[] flags)
            {
                GetEditor<FilesModEditor>().Add(path, Path.GetFileName(path), flags);
            }

            public void AddFolder(string path)
            {
                GetEditor<FolderModEditor>().Add(path, Path.GetFileName(path));
            }

            public void AddLibrary(string path, bool weak)
            {
                GetEditor<LibraryModEditor>().Add(path, Path.GetFileName(path), weak);
            }

            public void AddFramework(string framework, bool weak)
            {
                GetEditor<FrameworkModEditor>().Add(framework, weak);
            }

            public void SetBuildSetting(string name, string value)
            {
                GetEditor<BuildSettingsModEditor>().Add(name, value);
            }

            public void AddLocalization(string lang)
            {
                GetEditor<LocalizationModEditor>().Add(lang);
            }

            public void AddLocalization(string name, string path)
            {
                GetEditor<LocalizationModEditor>().Add(name, path);
            }

            public void AddLocalization(string name, string path, string variantGroup)
            {
                GetEditor<LocalizationModEditor>().Add(name, path, variantGroup);
            }

            public void AddPlistFields(IDictionary data)
            {
                GetEditor<PListModEditor>().Add(data);
            }

            public void AddShellScript(string script, string shell)
            {
                GetEditor<ShellScriptModEditor>().Add(script, shell);
            }

            public void AddShellScript(string script, string shell, int order)
            {
                GetEditor<ShellScriptModEditor>().Add(script, shell, order);
            }

            public void AddShellScript(string script, string shell, string target, int order)
            {
                GetEditor<ShellScriptModEditor>().Add(script, shell, target, order);
            }

            public void SetSystemCapability(string name, bool enabled)
            {
                GetEditor<SystemCapabilityModEditor>().Add(name, enabled);
            }

            public void SetProvisioningProfile(string path)
            {
                GetEditor<ProvisioningModEditor>().Add(path);
            }

            public void AddKeychainAccessGroup(string entitlementsFile, string accessGroup)
            {
                GetEditor<KeychainAccessGroupModEditor>().Add(entitlementsFile, accessGroup);
            }

            public void AddKeychainAccessGroup(string accessGroup)
            {
                GetEditor<KeychainAccessGroupModEditor>().Add(accessGroup);
            }

            public void Commit()
            {
                var modEditors = ModEditors.Values;
                foreach(var editor in modEditors)
                {
                    var err = editor.Validate(this);
                    if(!string.IsNullOrEmpty(err))
                    {
                        throw new InvalidOperationException(err);
                    }
                }
                foreach(var editor in modEditors)
                {
                    editor.Apply(this);
                }

                // Save project file
                File.WriteAllText(Project.PbxPath, Pbx.WriteToString());
            }

            #endregion

            #region Internal Mods

            /// <summary>
            /// Interface for the different Mods Editors
            /// </summary>
            public class IModEditor
            {
                /// <summary>
                /// Validate this instance.
                /// Throws an exception if there are inconsistent modifications
                /// returns null if success, or a string with the error.
                /// </summary>
                public virtual string Validate(XcodeEditorInternal editor)
                {
                    return null;
                }

                /// <summary>
                /// Apply mods to the enclosing Project
                /// </summary>
                public virtual void Apply(XcodeEditorInternal editor)
                {
                }
            }

            /// <summary>
            /// Header Search Paths Editor
            /// </summary>
            class HeaderPathsModEditor : IModEditor
            {
                const string HeadersSearchPathSettingsKey = "HEADER_SEARCH_PATHS";
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Path;
                }

                public void Add(string path)
                {
                    _mods.Add(new ModData{ Path = path });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var path = editor.ReplaceProjectVariables(mod.Path);
                        editor.Pbx.AddBuildProperty(editor.DefaultTargetGuid, HeadersSearchPathSettingsKey, path);
                    }
                }
            }

            /// <summary>
            /// Library Search Paths Editor
            /// </summary>
            class LibraryPathsModEditor : IModEditor
            {
                const string LibrarySearchPathSettingsKey = "LIBRARY_SEARCH_PATHS";
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Path;
                }

                public void Add(string path)
                {
                    _mods.Add(new ModData{ Path = path });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var path = editor.ReplaceProjectVariables(mod.Path);
                        editor.Pbx.AddBuildProperty(editor.DefaultTargetGuid, LibrarySearchPathSettingsKey, path);
                    }
                }
            }

            /// <summary>
            /// Copy File Editor
            /// </summary>
            class CopyFileModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Base;
                    public string Src;
                    public string Dst;
                }

                public void Add(string basePath, string src, string dst)
                {
                    _mods.Add(new ModData{ Base = basePath, Src = src, Dst = dst });
                }

                public override string Validate(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var filePath = editor.ReplaceProjectVariables(mod.Src);
                        if(filePath.EndsWith("/*"))
                        {
                            var tempFilePath = filePath.Substring(0, filePath.Length - 1);
                            if(!Directory.Exists(tempFilePath))
                            {
                                return string.Format("Directory '{0}' does not exists", tempFilePath); 
                            }
                        }
                        else
                        {
                            if(!File.Exists(filePath))
                            {
                                return string.Format("File '{0}' does not exists", filePath); 
                            }
                        }
                    }
                    return null;
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var fromPath = editor.ReplaceProjectVariables(mod.Src);
                        var toPath = editor.ReplaceProjectVariables(mod.Dst);
                        CopyFile(fromPath, toPath);
                    }
                }

                static void CopyFile(string source, string destination)
                {
                    var fromPath = Path.GetFullPath(source);
                    var toPath = Path.GetFullPath(destination);
                    if(Path.GetFileName(toPath).Length == 0)
                    {
                        toPath = Path.Combine(toPath, Path.GetFileName(fromPath));
                    }
                    string fromDir = null;
                    string fromFile = null;
                    string toFile = null;
                    string toDir = null;

                    var i = fromPath.IndexOf('*');
                    if(i == -1)
                    {
                        if(Directory.Exists(fromPath))
                        {
                            fromDir = fromPath;
                            fromFile = "*";
                            if(Directory.Exists(toPath))
                            {
                                toPath = Path.Combine(toPath, Path.GetFileName(fromPath));
                            }
                        }
                        else
                        {
                            fromDir = Path.GetDirectoryName(fromPath);
                            fromFile = Path.GetFileName(fromPath);
                        }
                        toDir = Path.GetDirectoryName(toPath);
                        toFile = Path.GetFileName(toPath);
                    }
                    else
                    {
                        i = fromPath.LastIndexOf(Path.DirectorySeparatorChar, i);
                        fromDir = fromPath.Substring(0, i);
                        i++;
                        fromFile = fromPath.Substring(i, fromPath.Length - i);
                        toDir = toPath;
                    }

                    foreach(string fromFilepath in Directory.GetFiles(fromDir, fromFile, SearchOption.AllDirectories))
                    {
                        var toFilePath = fromFilepath;
                        toFilePath = toFilePath.Replace(fromDir, toDir);
                        if(toFile != null)
                        {
                            i = toFilePath.LastIndexOf(fromFile);
                            if(i >= 0)
                            {
                                toFilePath = toFilePath.Substring(0, i) + toFile;
                            }
                        }

                        var toFileDir = Path.GetDirectoryName(toFilePath);
                        if(!Directory.Exists(toFileDir))
                        {
                            Directory.CreateDirectory(toFileDir);
                        }
                        File.Copy(fromFilepath, toFilePath, true);
                    }
                }
            }

            /// <summary>
            /// Files Editor
            /// </summary>
            class FilesModEditor : IModEditor
            {
                const string FilesBasePath = XcodeEditorInternal.ModsBasePath;
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Name;
                    public string[] Flags;
                }

                public void Add(string fullPath, string name, string[] flags)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Name = name, Flags = flags });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var fullPath = editor.ReplaceProjectVariables(mod.FullPath);
                        var projPath = Path.Combine(FilesBasePath, mod.Name);
                        var guid = editor.Pbx.AddFile(fullPath, projPath);

                        if(mod.Flags.Length > 0)
                        {
                            editor.Pbx.AddFileToBuildWithFlags(editor.DefaultTargetGuid, guid, string.Join(" ", mod.Flags));
                        }
                        else
                        {
                            editor.Pbx.AddFileToBuild(editor.DefaultTargetGuid, guid);
                        }
                    }
                }
            }

            /// <summary>
            /// Folder references Editor
            /// </summary>
            class FolderModEditor : IModEditor
            {
                const string FoldersBasePath = XcodeEditorInternal.ModsBasePath;
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Name;
                }

                public void Add(string fullPath, string name)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Name = name });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var fullPath = editor.ReplaceProjectVariables(mod.FullPath);
                        var projPath = Path.Combine(FoldersBasePath, mod.Name);
                        editor.Pbx.AddFolderReference(fullPath, projPath);
                    }
                }
            }

            /// <summary>
            /// Linked Library Editor
            /// </summary>
            class LibraryModEditor : IModEditor
            {
                const string LibBasePath = XcodeEditorInternal.ModsBasePath + "/Libraries";
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Name;
                    public bool Weak;
                }

                public void Add(string fullPath, string name, bool weak)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Name = name, Weak = weak });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        string fullPath;
                        PBXSourceTree sourceTree = PBXSourceTree.Source;
                        // If is only a filename, treat it as a System lib. 
                        // Otherwise, process the library path.
                        if(mod.Name.Equals(Path.GetFileName(mod.Name)))
                        {
                            fullPath = Path.Combine("usr/lib", mod.FullPath);
                            sourceTree = PBXSourceTree.Sdk;
                        }
                        else
                        {
                            fullPath = editor.ReplaceProjectVariables(mod.FullPath);
                        }

                        var projPath = Path.Combine(LibBasePath, mod.Name);
                        var guid = editor.Pbx.AddFile(fullPath, projPath, sourceTree);
                        editor.Pbx.AddFileToBuild(editor.DefaultTargetGuid, guid, mod.Weak);
                    }
                }
            }

            /// <summary>
            /// Frameworks Editor
            /// </summary>
            class FrameworkModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                public struct ModData
                {
                    public string Framework;
                    public bool Weak;
                }

                public void Add(string framework, bool weak)
                {
                    _mods.Add(new ModData{ Framework = framework, Weak = weak });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddFrameworkToProject(editor.DefaultTargetGuid, mod.Framework, mod.Weak);
                    }
                }
            }

            /// <summary>
            /// Build Settings Editor
            /// </summary>
            class BuildSettingsModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Symbol;
                    public string Value;
                }

                public void Add(string symbol, string value)
                {
                    _mods.Add(new ModData{ Symbol = symbol, Value = value });
                }

                public override string Validate(XcodeEditorInternal editor)
                {
                    var dic = new Dictionary<string, string>();
                    foreach(var mod in _mods)
                    {
                        string val;
                        if(dic.TryGetValue(mod.Symbol, out val))
                        {
                            if(val != mod.Value)
                            {
                                return "Conflicting Build Settings";
                            }
                        }
                        else
                        {
                            dic.Add(mod.Symbol, mod.Value);
                        }
                    }

                    return null;
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.SetBuildProperty(editor.DefaultTargetGuid, mod.Symbol, mod.Value);
                    }
                }
            }

            /// <summary>
            /// Localization Editor
            /// </summary>
            class LocalizationModEditor : IModEditor
            {
                const string LocalizableContent = "/* Sparta autogenerated localization file */";
                const string DefaultLocalizationGroupName = "Localizable.strings";
                const string DefaultLocalizationProjSuffix = ".lproj";
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Language;
                    public bool CreateLocFile;
                    public string Name;
                    public string Path;
                    public string Group;
                }

                public void Add(string lang)
                {
                    Add(lang, true, string.Empty, string.Empty, DefaultLocalizationGroupName);
                }

                public void Add(string name, string path)
                {
                    Add(string.Empty, false, name, path, DefaultLocalizationGroupName);
                }

                public void Add(string name, string path, string variantGroup)
                {
                    Add(string.Empty, false, name, path, variantGroup);
                }

                public void Add(string lang, bool createFile, string name, string path, string variantGroup)
                {
                    _mods.Add(new ModData {
                        Language = lang,
                        CreateLocFile = createFile,
                        Name = name,
                        Path = path,
                        Group = variantGroup
                    });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        if(mod.CreateLocFile)
                        {
                            var folderName = string.Format("{0}{1}", mod.Language, DefaultLocalizationProjSuffix);
                            var groupPath = Path.Combine(editor.Project.ProjectPath, folderName);
                            var filePath = Path.Combine(groupPath, DefaultLocalizationGroupName);

                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                            File.WriteAllText(filePath, LocalizableContent);

                            editor.Pbx.AddLocalization(filePath, mod.Language, DefaultLocalizationGroupName);
                        }
                        else
                        {
                            var filePath = editor.ReplaceProjectVariables(mod.Path);
                            editor.Pbx.AddLocalization(filePath, mod.Name, mod.Group);
                        }
                    }
                }
            }

            /// <summary>
            /// Plist Editor
            /// </summary>
            class PListModEditor : IModEditor
            {
                const bool DefaultOverride = true;
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public IDictionary Data;
                    public bool Override;
                }

                public void Add(IDictionary data)
                {
                    Add(data, DefaultOverride);
                }

                public void Add(IDictionary data, bool overrides)
                {
                    _mods.Add(new ModData{ Data = data, Override = overrides });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    if(_mods.Count > 0)
                    {
                        var plistPath = Path.Combine(editor.Project.ProjectRootPath, XcodeEditorInternal.DefaultPListFileName);
                        var plist = new PlistDocument();
                        plist.ReadFromFile(plistPath);

                        var root = plist.root;

                        foreach(var mod in _mods)
                        {
                            Combine(root, mod.Data, mod.Override);
                        }

                        plist.WriteToFile(plistPath);
                    }
                }

                void Combine(PlistElementDict dic, IDictionary data, bool overrides)
                {
                    foreach(DictionaryEntry entry in data)
                    {
                        var key = (string)entry.Key;

                        if(entry.Value is IDictionary)
                        {
                            PlistElement child = dic[key];
                            if(child == null)
                            {
                                child = dic.CreateDict(key);
                            }
                            else if(!overrides && !(child is PlistElementDict))
                            {
                                throw new ConflictingDataException(child);
                            }

                            Combine(child as PlistElementDict, entry.Value as IDictionary, overrides);
                        }
                        else if(entry.Value is ArrayList)
                        {
                            PlistElement child = dic[key];
                            if(child == null)
                            {
                                child = dic.CreateArray(key);
                            }
                            else if(!overrides && !(child is PlistElementArray))
                            {
                                throw new ConflictingDataException(child);
                            }

                            Combine(child as PlistElementArray, entry.Value as ArrayList, overrides);   
                        }
                        else if(entry.Value is string)
                        {
                            PlistElement child = dic[key];
                            if(!overrides && child != null)
                            {
                                throw new ConflictingDataException(child);

                            }
                            dic[key] = new PlistElementString((string)entry.Value);
                        }
                        else if(entry.Value is bool)
                        {
                            PlistElement child = dic[key];
                            if(!overrides && child != null)
                            {
                                throw new ConflictingDataException(child);

                            }
                            dic[key] = new PlistElementBoolean((bool)entry.Value);
                        }
                        else if(entry.Value is int)
                        {
                            PlistElement child = dic[key];
                            if(!overrides && child != null)
                            {
                                throw new ConflictingDataException(child);

                            }
                            dic[key] = new PlistElementInteger((int)entry.Value);
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                    }
                }

                void Combine(PlistElementArray list, ArrayList data, bool overrides)
                {
                    foreach(var entry in data)
                    {
                        if(entry is IDictionary)
                        {
                            var dic = list.AddDict();
                            Combine(dic, entry as IDictionary, overrides);
                        }
                        else if(entry is string)
                        {
                            list.AddString((string)entry);
                        }
                        else if(entry is bool)
                        {
                            list.AddBoolean((bool)entry);
                        }
                        else if(entry is int)
                        {
                            list.AddInteger((int)entry);
                        }
                        else
                        {
                            throw new InvalidDataException();
                        }
                    }
                }

                /// <summary>
                /// Plist editor exceptions
                /// </summary>
                class PlistEditorException : Exception
                {
                    public PlistEditorException(string msg) : base(msg)
                    {
                    }
                }

                class ConflictingDataException : PlistEditorException
                {
                    public ConflictingDataException(PlistElement elem)
                        : base("Conflicting data in " + elem)
                    {
                    }
                }

                class InvalidDataException : PlistEditorException
                {
                    public InvalidDataException()
                        : base("Unrecognised data type")
                    {
                    }
                }
            }

            /// <summary>
            /// Shell Scripts Editor
            /// </summary>
            class ShellScriptModEditor : IModEditor
            {
                const int DefaultOrder = -1;
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Script;
                    public string Shell;
                    public string Target;
                    public int Order;
                }

                public void Add(string script, string shell)
                {
                    Add(script, shell, string.Empty, DefaultOrder);
                }

                public void Add(string script, string shell, int order)
                {
                    Add(script, shell, string.Empty, order);
                }

                public void Add(string script, string shell, string target, int order)
                {
                    _mods.Add(new ModData {
                        Script = script,
                        Shell = shell,
                        Target = target,
                        Order = order
                    });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var targetGuid = editor.DefaultTargetGuid;
                        if(!string.IsNullOrEmpty(mod.Target))
                        {
                            targetGuid = editor.Pbx.TargetGuidByName(mod.Target);
                        }
                        editor.Pbx.AddShellScript(targetGuid, mod.Script, mod.Shell, mod.Order);
                    }
                }
            }

            /// <summary>
            /// System Capabilities Editor
            /// </summary>
            class SystemCapabilityModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Name;
                    public bool Enabled;
                }

                public void Add(string name, bool enabled)
                {
                    _mods.Add(new ModData{ Name = name, Enabled = enabled });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.SetSystemCapability(editor.DefaultTargetGuid, mod.Name, mod.Enabled);
                    }
                }
            }

            /// <summary>
            /// Provisioning Profile Editor
            /// </summary>
            class ProvisioningModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Path;
                }

                public void Add(string path)
                {
                    _mods.Add(new ModData{ Path = path });
                }

                public override string Validate(XcodeEditorInternal editor)
                {
                    string provisioning = null;
                    foreach(var mod in _mods)
                    {
                        
                        if(string.IsNullOrEmpty(provisioning))
                        {
                            provisioning = mod.Path;
                        }
                        else if(provisioning != mod.Path)
                        {
                            return "Conflicting Provisioning Profile";
                        }
                    }
                    return null;
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var path = editor.ReplaceProjectVariables(mod.Path);
                        var plistPath = Path.Combine(editor.Project.ProjectRootPath, XcodeEditorInternal.DefaultPListFileName);
                        editor.Pbx.SetProvisioningProfile(editor.DefaultTargetGuid, path, plistPath);
                    }
                }
            }

            /// <summary>
            /// Keychain Access Groups Editor
            /// </summary>
            class KeychainAccessGroupModEditor : IModEditor
            {
                const string DefaultEntitlementsFile = "Unity-iPhone.entitlements";
                const string KeychainAccessGroupsKey = "keychain-access-groups";

                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string EntitlementsFile;
                    public string AccessGroup;
                }

                public void Add(string accessGroup)
                {
                    Add(DefaultEntitlementsFile, accessGroup);
                }

                public void Add(string entitlementsFile, string accessGroup)
                {
                    _mods.Add(new ModData{ EntitlementsFile = entitlementsFile, AccessGroup = accessGroup });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    var groups = new Dictionary<string, List<string>>();
                    foreach(var mod in _mods)
                    {
                        List<string> list;
                        if(!groups.TryGetValue(mod.EntitlementsFile, out list))
                        {
                            list = new List<string>();
                            groups.Add(mod.EntitlementsFile, list);
                        }

                        list.Add(mod.AccessGroup);
                    }

                    foreach(var entitlementFile in groups.Keys)
                    {
                        var path = Path.Combine(editor.Project.ProjectRootPath, entitlementFile);

                        var plist = new PlistDocument();

                        if(!File.Exists(path))
                        {
                            File.Create(path).Dispose();
                            editor.Pbx.AddFile(path, entitlementFile);
                        }
                        else
                        {
                            plist.ReadFromFile(path);
                        }

                        var el = plist.root[KeychainAccessGroupsKey] ?? plist.root.CreateArray(KeychainAccessGroupsKey);
                        var list = el.AsArray();
                        foreach(var gr in groups[entitlementFile])
                        {
                            list.AddString(gr);
                        }

                        plist.WriteToFile(path);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}

