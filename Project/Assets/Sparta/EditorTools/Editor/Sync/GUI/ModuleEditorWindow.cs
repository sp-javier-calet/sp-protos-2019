﻿using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.Sync.View
{
    public class ModuleEditorWindow : EditorWindow
    {
        #region Editor options

        [MenuItem("Sparta/Sync/Module Info", false, 5)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ModuleEditorWindow), false, "Sparta Module", true);
        }

        #endregion

        ModuleEditorWindow()
        {
            Sparta.OnChanged += OnSpartaChanged;
            Sparta.OnModuleSelected += OnSelect;
        }

        ~ModuleEditorWindow()
        {
            Sparta.OnChanged -= OnSpartaChanged;
            Sparta.OnModuleSelected -= OnSelect;
        }

        void OnSpartaChanged()
        {
            Repaint();
        }

        void OnSelect()
        {
            ModuleSync = Sparta.SelectedModule;
            Repaint();
        }


        Vector2 _scrollPosition;
        string _fileContent;
        bool _showRawFile;
        bool _showDiff;

        Module _module;

        Module Module
        {
            get
            {
                return _module;
            }

            set
            {
                _module = value;
                _fileContent = string.Empty;
                if(_module != null && _module.Valid)
                {
                    _fileContent = File.ReadAllText(_module.ModuleFile);
                }

            }
        }

        ModuleSync _moduleSync;

        ModuleSync ModuleSync
        {
            get
            {
                return _moduleSync;
            }

            set
            {
                _moduleSync = value;
                Module = _moduleSync != null ? _moduleSync.ReferenceModule : null;
            }
        }

        #region Draw GUI

        void OnGUI()
        {
            if(!Sparta.Target.Valid || Module == null)
            {
                GUILayout.Label("No module selected", EditorStyles.boldLabel);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUIModuleInfo();
            GUIModuleDiff();

            if(Sparta.AdvancedMode)
            {
                GUIAdvancedMode();
            }

            EditorGUILayout.EndScrollView();
        }

        void GUIModuleInfo()
        {
            GUILayout.Label(Module.Name, EditorStyles.boldLabel);

            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label(string.Format("Module type: {0}", Module.Type));
            GUILayout.Label(string.Format("Relative Location: {0}", Module.RelativePath));

            // Show dependencies
            if(Module.Dependencies.Count > 0)
            {
                GUILayout.BeginVertical();
                GUILayout.Label("Dependencies", EditorStyles.boldLabel);
                bool first = true;
                var builder = new StringBuilder();
                foreach(var dependency in Module.Dependencies)
                {
                    if(first)
                    {
                        builder.Append(dependency);
                        first = false;
                    }
                    else
                    {
                        builder.AppendFormat("\n{0}", dependency);
                    }
                }
                EditorGUILayout.HelpBox(builder.ToString(), MessageType.None);
                GUILayout.EndVertical();
            }

            GUILayout.EndVertical();
        }

        void GUIModuleDiff()
        {
            _showDiff = EditorGUILayout.Foldout(_showDiff, "Module diff");
            if(_showDiff)
            {
                bool hasChanges = false;
                GUILayout.BeginVertical(Styles.Group);
                foreach(var file in _moduleSync.Files)
                {
                    if(file.FileStatus != ModuleSync.FileStatus.Equals)
                    {
                        GUILayout.BeginHorizontal(Styles.Group);
                        GUILayout.Label(file.File);
                        GUILayout.Label(file.FileStatus.ToString(), Styles.ModuleStatus);
                        GUILayout.EndHorizontal();
                        hasChanges = true;
                    }
                }

                if(!hasChanges)
                {
                    GUILayout.Label("No differences");
                }
                GUILayout.EndVertical();
            }
        }

        void GUIAdvancedMode()
        {
            // Raw Module file editor
            _showRawFile = EditorGUILayout.Foldout(_showRawFile, "Raw Module file");
            if(_showRawFile)
            {
                GUILayout.BeginVertical(Styles.Group);
                _fileContent = GUILayout.TextArea(_fileContent);

                GUILayout.BeginHorizontal(Styles.Group);
                if(GUILayout.Button("Reload", GUILayout.MaxWidth(60)))
                {
                    _fileContent = File.ReadAllText(Module.ModuleFile);
                    Repaint();
                }

                if(GUILayout.Button("Save", GUILayout.MaxWidth(60)))
                {
                    File.WriteAllText(Module.ModuleFile, _fileContent);
                    Repaint();
                }

                if(GUILayout.Button("Remove", GUILayout.MaxWidth(60)))
                {
                    if(EditorUtility.DisplayDialog("Delete sparta module", 
                           string.Format("Remove module definition from '{0}'? This won't delete code.", Module.ModuleFile), 
                           "Remove", 
                           "Cancel"))
                    {
                        File.Delete(Module.ModuleFile);
                        Module.Valid = false;
                        Module = null;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        #endregion
    }
}
