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
                { typeof(FrameworkModEditor), new FrameworkModEditor() },
                { typeof(CopyFileModEditor), new CopyFileModEditor() },
                { typeof(PListModEditor), new PListModEditor() }
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

            #region IXcodeEditor methods

            public override void AddFile(string relativePath)
            {
                Pbx.AddFile(Path.GetFullPath(relativePath), relativePath);
            }

            public override void AddFramework(string path, bool weak = false)
            {
                GetEditor<FrameworkModEditor>().Add(path, weak);
            }

            public override void CopyFile(string basePath, string src, string dst)
            {
                GetEditor<CopyFileModEditor>().Add(basePath, src, dst);
            }

            public override void Commit()
            {
                var modEditors = ModEditors.Values;
                foreach(var editor in modEditors)
                {
                    editor.Validate();
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
                /// </summary>
                public virtual void Validate()
                {
                }

                /// <summary>
                /// Apply mods to the enclosing Project
                /// </summary>
                public virtual void Apply(XcodeEditorInternal editor)
                {
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
                    public string Path;
                    public bool Weak;
                }

                public void Add(string path, bool weak)
                {
                    _mods.Add(new ModData{ Path = path, Weak = weak });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    foreach(var mod in _mods)
                    {
                        editor.Pbx.AddFrameworkToProject(editor.DefaultTargetGuid, mod.Path, mod.Weak);
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
                    public string Base; // TODO Same than project?
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
                                toFilePath = toFilePath.Substring(0, i)+toFile;
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
            /// Header Paths Editor
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
                    var plistPath = Path.Combine(editor.Project.ProjectPath, DefaultPListFileName);
                    var plist = new PlistDocument();
                    plist.ReadFromFile(plistPath);


                    foreach(var mod in _mods)
                    {
                        var v = plist.root[mod.Key];
                        if(v == null)
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

            #endregion

            /*
         * frameworks
         * headerpaths
         * copyFiles 

         * targetAttributes 
         * buildSettings
         * shellScripts
         
         * variantGroups 
         * provisioningProfile
         * keychainAccessGroups
         * infoPlist
         */
        }

        #endregion
    }
}

