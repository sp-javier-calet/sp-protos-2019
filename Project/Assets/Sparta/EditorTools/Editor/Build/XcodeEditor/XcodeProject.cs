using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.iOS.Xcode;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Xcode project wrapper class.
    /// </summary>
    public class XcodeProject
    {
        public readonly string ProjectPath;

        readonly PBXProject _project;
        readonly XcodeEditorInternal _editor;

        public XcodeProject(string path)
        {
            ProjectPath = path;
            //var cPath = PBXProject.GetPBXProjectPath(); // TEST
            _project = new PBXProject();
            _project.ReadFromFile(ProjectPath);

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

            const string DefaultTargetName = "Unity-iPhone";

            public readonly XcodeProject Project;
            public readonly PBXProject Pbx;
            public readonly string DefaultTargetGuid;

            public XcodeEditorInternal(XcodeProject project, PBXProject pbx)
            {
                Project = project;
                Pbx = pbx;
                DefaultTargetGuid = Pbx.TargetGuidByName(DefaultTargetName);
            }

            public string ReplaceProjectVariables(string originalPath)
            {
                return originalPath; // TODO
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
                GetEditor<FilesModEditor>().Add(Path.GetFullPath(path), path);
            }

            public override void AddFolder(string path)
            {
                GetEditor<FolderModEditor>().Add(Path.GetFullPath(path), path);
            }

            public override void AddLibrary(string path)
            {
                GetEditor<LibraryModEditor>().Add(Path.GetFullPath(path), path);
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
                File.WriteAllText(Project.ProjectPath, Pbx.WriteToString());
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
                        editor.Pbx.SetBuildProperty(editor.DefaultTargetGuid, "HEADER_SEARCH_PATHS", path);
                    }
                }
            }

            /// <summary>
            /// Library Search Paths Editor
            /// </summary>
            class LibraryPathsModEditor : IModEditor
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
                    foreach(var mod in _mods)
                    {
                        var path = editor.ReplaceProjectVariables(mod.Path);
                        editor.Pbx.SetBuildProperty(editor.DefaultTargetGuid, "LIBRARY_SEARCH_PATHS", path);
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
                        CopyFile(editor.ReplaceProjectVariables(mod.Src), editor.ReplaceProjectVariables(mod.Dst));
                    }
                }

                void CopyFile(string source, string destination)
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
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Path;
                }

                public void Add(string fullPath, string path)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Path = path });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddFile(mod.FullPath, mod.Path);
                        /*if(mod.Path.EndsWith(".framework"))
                        {
                            editor.Pbx.AddFile(mod.Path, frameworkGroup, "GROUP", true, false);
                        }
                        else
                        {
                            string[] compilerFlags = null;
                            string[] filename = filePath.Split(':');
                            if( filename.Length > 1 )
                            {
                                compilerFlags = filename[1].Split(',');
                            }
                            this.AddFile(filename[0], modGroup, "SOURCE_ROOT", true, false, compilerFlags);
                        }*/
                    }
                }
            }

            /// <summary>
            /// Folder references Editor
            /// </summary>
            class FolderModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Path;
                }

                public void Add(string fullPath, string path)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Path = path });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddFolderReference(mod.FullPath, mod.Path);
                    }
                }
            }

            /// <summary>
            /// Linked Library Editor
            /// </summary>
            class LibraryModEditor : IModEditor
            {
                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string FullPath;
                    public string Path;
                }

                public void Add(string fullPath, string path)
                {
                    _mods.Add(new ModData{ FullPath = fullPath, Path = path });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddExternalLibraryDependency(editor.DefaultTargetGuid, mod.FullPath, "GUID", mod.Path, "Remoteinfo");
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
                        editor.Pbx.SetBuildProperty(editor.DefaultTargetGuid, mod.Symbol, mod.Value);
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

                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Script;
                    public string Shell;
                }

                public void Add(string path)
                {
                    Add(path, DefaultShell);
                }

                public void Add(string script, string shell)
                {
                    _mods.Add(new ModData{ Script = script, Shell = shell });
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
                    // TODO
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

