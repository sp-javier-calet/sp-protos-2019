using System;
using System.IO;
using System.Collections.Generic;
using SpartaTools.iOS.Xcode;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Xcode project wrapper class.
    /// </summary>
    public class XcodeProject
    {
        public readonly string ProjectPath;
        public readonly string PbxPath;
        public readonly string BasePath;

        readonly PBXProject _project;
        readonly XcodeEditorInternal _editor;

        public XcodeProject(string xcodeProjectPath, string basePath)
        {
            ProjectPath = xcodeProjectPath;
            PbxPath = Path.Combine(xcodeProjectPath, "project.pbxproj");
            BasePath = basePath;

            _project = new PBXProject();
            _project.ReadFromFile(PbxPath);

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
                { typeof(VariantGroupModEditor),        new VariantGroupModEditor()        },
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

            public readonly XcodeProject Project;
            public readonly PBXProject Pbx;
            public readonly string DefaultTargetGuid;

            readonly IDictionary<string, string> _projectVariables;

            public XcodeEditorInternal(XcodeProject project, PBXProject pbx)
            {
                Project = project;
                Pbx = pbx;
                DefaultTargetGuid = Pbx.TargetGuidByName(PBXProject.GetUnityTargetName());

                _projectVariables = new Dictionary<string, string>() {
                    { ProjectPathVar,  project.ProjectPath }, // Path to the .xcodeproj folder
                    { ProjectRootVar,  project.ProjectPath }, // FIXME
                    { CurrentRootPath, project.BasePath    }  // Path to the Unity project root
                };
            }

            public string ReplaceProjectVariables(string originalPath)
            {
                var path = originalPath;
                foreach(var entry in _projectVariables)
                {
                    var pattern = string.Format("{{{0}}}", entry.Key);
                    path = path.Replace(pattern, entry.Value);
                }
                return Path.GetFullPath(path);
            }

            #region XCodeProjectEditor interface methods

            public override void AddHeaderSearchPath(string path)
            {
                GetEditor<HeaderPathsModEditor>().Add(path);
            }

            public override void AddLibrarySearchPath(string path)
            {
                GetEditor<LibraryPathsModEditor>().Add(path);
            }

            public override void CopyFile(string basePath, string src, string dst)
            {
                GetEditor<CopyFileModEditor>().Add(basePath, src, dst);
            }

            public override void AddFile(string path)
            {
                AddFile(path, new string[0]);
            }

            public override void AddFile(string path, string[] flags)
            {
                GetEditor<FilesModEditor>().Add(path, Path.GetFileName(path), flags);
            }

            public override void AddFolder(string path)
            {
                GetEditor<FolderModEditor>().Add(path, Path.GetFileName(path));
            }

            public override void AddLibrary(string path)
            {
                GetEditor<LibraryModEditor>().Add(path, Path.GetFileName(path));
            }

            public override void AddFramework(string framework, bool weak)
            {
                GetEditor<FrameworkModEditor>().Add(framework, weak);
            }

            public override void SetBuildSetting(string name, string value)
            {
                GetEditor<BuildSettingsModEditor>().Add(name, value);
            }

            public override void AddVariantGroup(string variantGroup, string key, string value)
            {
                GetEditor<VariantGroupModEditor>().Add(variantGroup, key, value);
            }

            public override void SetPlistField(string name, Dictionary<string, object>  value)
            {
                GetEditor<PListModEditor>().Add(name, value);
            }

            public override void AddShellScript(string script)
            {
                GetEditor<ShellScriptModEditor>().Add(script);
            }

            public override void SetSystemCapability(string name, bool enabled)
            {
                GetEditor<SystemCapabilityModEditor>().Add(name, enabled);
            }

            public override void SetProvisioningProfile(string path)
            {
                GetEditor<ProvisioningModEditor>().Add(path);
            }

            public override void AddKeychainAccessGroup(string accessGroup)
            {
                GetEditor<KeychainAccessGroupModEditor>().Add(accessGroup);
            }

            public override void Commit()
            {
                var modEditors = ModEditors.Values;
                foreach(var editor in modEditors)
                {
                    var err = editor.Validate();
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
                public virtual string Validate()
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
                    // TODO Same than project?
                    public string Src;
                    public string Dst;
                }

                public void Add(string basePath, string src, string dst)
                {
                    _mods.Add(new ModData{ Base = basePath, Src = src, Dst = dst });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        var fromPath = Path.Combine(mod.Base, editor.ReplaceProjectVariables(mod.Src));
                        var toPath = Path.Combine(mod.Base, editor.ReplaceProjectVariables(mod.Dst));
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
                const string FilesBasePath = XcodeEditorInternal.ModsBasePath + "/Classes";
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
                        var projPath = Path.Combine(LibBasePath, mod.Name);
                        var guid = editor.Pbx.AddFile(fullPath, projPath);
                        editor.Pbx.AddFileToBuild(editor.DefaultTargetGuid, guid);
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

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddBuildProperty(editor.DefaultTargetGuid, mod.Symbol, mod.Value);
                    }
                }
            }

            /// <summary>
            /// Variant Groups Editor
            /// </summary>
            class VariantGroupModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Group;
                    public string Key;
                    public string Value;
                }

                public void Add(string variantGroup, string key, string value)
                {
                    _mods.Add(new ModData{ Group = variantGroup, Key = key, Value = value });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        // TODO
                    }
                }
            }

            /// <summary>
            /// Plist Editor
            /// </summary>
            class PListModEditor : IModEditor
            {
                const string DefaultPListFileName = "Info.plist";

                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Key;
                    public Dictionary<string, object> Dic;
                }

                public void Add(string key, Dictionary<string, object> dic)
                {
                    _mods.Add(new ModData{ Key = key, Dic = dic });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    if(_mods.Count > 0)
                    {
                        var plistPath = Path.Combine(editor.Project.ProjectPath, DefaultPListFileName);
                        var plist = new PlistDocument();
                        plist.ReadFromFile(plistPath);

                        foreach(var mod in _mods)
                        {
                            var v = plist.root[mod.Key];
                            if(v == null) // FIXME WORKS?
                            {
                                v = plist.root.CreateArray(mod.Key);
                            }

                        }
                        // TODO
                        /*
                         var urlTypes = plist.root["CFBundleURLTypes"].AsArray();
                         var dict = urlTypes.AddDict();
                         var array = dict.CreateArray("CFBundleURLSchemes");
                         array.AddString(urlSchemeString);
                        */
                        plist.WriteToFile(plistPath);
                    }
                }
            }

            /// <summary>
            /// Shell Scripts Editor
            /// </summary>
            class ShellScriptModEditor : IModEditor
            {
                const string DefaultShell = "/bin/sh";
                const int DefaultOrder = -1;

                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Script;
                    public string Shell;
                    public string Target;
                    public int Order;
                }

                public void Add(string path)
                {
                    Add(path, DefaultShell);
                }

                public void Add(string script, string shell)
                {
                    _mods.Add(new ModData {
                        Script = script,
                        Shell = shell,
                        Target = string.Empty,
                        Order = DefaultOrder
                    });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    // TODO
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
                        // TODO
                        // editor.Pbx.SetBuildProperty(editor.DefaultTargetGuid, mod.Name, mod.Enabled);
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

                public override void Apply(XcodeEditorInternal editor)
                {
                    // TODO
                }
            }

            /// <summary>
            /// Keychain Access Groups Editor
            /// </summary>
            class KeychainAccessGroupModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string AccessGroup;
                }

                public void Add(string accessGroup)
                {
                    _mods.Add(new ModData{ AccessGroup = accessGroup });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    // TODO
                }
            }

            #endregion
        }

        #endregion
    }
}

