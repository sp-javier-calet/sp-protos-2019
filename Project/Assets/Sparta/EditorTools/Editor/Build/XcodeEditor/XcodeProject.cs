using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor.iOS.Xcode;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Public interface for the Editor class
    /// There is only a implentation, but we need to hide some details 
    /// and public accessors to other classes.
    /// </summary>
    public abstract class XCodeProjectEditor
    {
        public abstract void AddFile(string relativePath);

        public abstract void AddFramework(string path, bool weak = false);

        public abstract void Commit();
    }

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

            #region IXcodeEditor methods

            public override void AddFile(string relativePath)
            {
                Pbx.AddFile(Path.GetFullPath(relativePath), relativePath);
            }

            public override void AddFramework(string path, bool weak = false)
            {
                GetEditor<FrameworkModEditor>().Add(path, weak);
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
            /// Plist Editor
            /// </summary>
            class PListModEditor : IModEditor
            {
                const string DefaultPListFileName = "Info.plist";

                readonly List<ModData> _mods = new List<ModData>();

                struct ModData
                {
                    public string Key;
                    public string[] Values;
                }

                public void Add(string key, string[] values)
                {
                    _mods.Add(new ModData{ Key = key, Values = values });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    var plistPath = Path.Combine(editor.Project.ProjectPath, DefaultPListFileName);
                    var plist = new PlistDocument();
                    plist.ReadFromFile(plistPath);


                    /*
                     var urlTypes = plist.root["CFBundleURLTypes"].AsArray();
                     var dict = urlTypes.AddDict();
                     var array = dict.CreateArray("CFBundleURLSchemes");
                     array.AddString(urlSchemeString);
                    */
                    plist.WriteToFile(plistPath);
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
                    public string Src;
                    public string Dst;
                }

                public void Add(string src, string dst)
                {
                    _mods.Add(new ModData{ Src = src, Dst = dst });
                }

                public override void Apply(XcodeEditorInternal editor)
                {
                    
                }
            }


            #endregion

            /*
         * targetAttributes 
         * buildSettings
         * shellScripts
         * frameworks
         * infoPlist
         * copyFiles 
         * variantGroups 
         * headerpaths
         * provisioningProfile
         * keychainAccessGroups
         */
        }

        #endregion
    }
}

