﻿using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using SpartaTools.Editor.Sync;
using SpartaTools.Editor.SpartaProject;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.View
{
    public class SyncProjectWindow : EditorWindow
    {
        #region Editor options

        [MenuItem("Sparta/Sync/Sync Tools", false, 9)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SyncProjectWindow), false, "Sparta Sync", true);
        }

        [MenuItem("Sparta/Sync/Create module...", false, 30)]
        public static void CreateModule()
        {
            var path = EditorUtility.OpenFolderPanel("Select module root", 
                Project.BasePath,
                Module.DefinitionFileName);

            if(!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                SyncTools.CreateModule(path);
                EditorUtility.DisplayDialog("Create Module", string.Format("Module created successfully in {0}", path), "Accept");
            }
            else
            {
                EditorUtility.DisplayDialog("Create Module", string.Format("Error creating module in {0}", path), "Accept");
            }
        }

        [MenuItem("Sparta/Sync/Advanced Mode", false, 50)]
        public static void ToggleAdvanced()
        {
            Sparta.AdvancedMode = !Sparta.AdvancedMode;
        }

        #endregion

        SyncProjectWindow()
        {
            Sparta.OnChanged += OnSpartaChanged;
        }

        ~SyncProjectWindow()
        {
            Sparta.OnChanged -= OnSpartaChanged;
        }

        void OnSpartaChanged()
        {
            RefreshModules();
            Repaint();
        }

        class ModuleSyncCategory
        {
            public string Name;
            public bool Show;
            public IList<ModuleSync> Modules;

            public ModuleSyncCategory(string name)
            {
                Name = name;
                Show = true;
                Modules = new List<ModuleSync>();
            }
        }

        // Options are tied to ModuleSync.SyncAction definition.
        readonly string[] InstalledModuleOptions = new [] {
            "None",
            "Override",
            "Uninstall" 
        };

        readonly string[] NotInstalledModuleOptions = new [] {
            "None",
            "Install"
        };

        IList<ModuleSync> _modules = new List<ModuleSync>();
        IList<ModuleSyncCategory> _categories = new List<ModuleSyncCategory>();
        Vector2 _scrollPosition = Vector2.right;
        ProgressHandler _progressHandler;
        bool _refreshFinished;

        bool Synchronized
        {
            get
            {
                return _categories != null && _categories.Count > 0;
            }
        }

        void RefreshModules()
        {
            if(Sparta.Target.Valid && _progressHandler == null)
            {
                _refreshFinished = false;

                _progressHandler = AsyncProcess.Start(progress => {
                    var dic = new Dictionary<string, ModuleSyncCategory>();
                    var categories = new List<ModuleSyncCategory>();
                    var modules = SyncTools.Synchronize(Sparta.Target.ProjectPath, progress);
                    foreach(var mod in modules)
                    {
                        ModuleSyncCategory category;
                        var categoryName = mod.Type.ToString();
                        if(!dic.TryGetValue(categoryName, out category))
                        {
                            category = new ModuleSyncCategory(categoryName);
                            categories.Add(category);
                            dic.Add(categoryName, category);
                        }

                        category.Modules.Add(mod);
                    }

                    // Synchronize
                    _modules = modules;
                    _categories = categories;
                    _refreshFinished = true;
                });
            }
        }

        #region Draw GUI

        void OnInspectorUpdate()
        {
            if(_progressHandler != null)
            {
                Repaint();
            }
        }

        void OnGUI()
        {
            if(!Sparta.Target.Valid)
            {
                GUILayout.Label("No project selected", EditorStyles.boldLabel);
                return;
            }
            if(_progressHandler != null)
            {
                if(_progressHandler.Finished && _refreshFinished)
                {
                    _progressHandler = null;
                    EditorUtility.ClearProgressBar();
                }
                else
                {
                    if(EditorUtility.DisplayCancelableProgressBar("Synchronizing", _progressHandler.Message, _progressHandler.Percent))
                    {
                        // TODO Cancel process
                        _progressHandler.Cancel();
                        _refreshFinished = true;
                    }
                }
            }
            else if(!Synchronized)
            {
                RefreshModules();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Registered Modules", EditorStyles.boldLabel);

            GUIRegisteredModules();
            GUIActionButtons();

            EditorGUILayout.EndScrollView();
        }

        void GUIRegisteredModules()
        {
            if(Synchronized)
            {
                foreach(var category in _categories)
                {
                    category.Show = EditorGUILayout.Foldout(category.Show, string.Format("{0} Modules", category.Name));
                    if(category.Show)
                    {
                        GUILayout.BeginVertical(Styles.Group);

                        foreach(var m in category.Modules)
                        {
                            GUIModuleStatus(m);
                        }
                        GUILayout.EndVertical();
                    }
                    EditorGUILayout.Space();
                }
            }
        }

        void GUIActionButtons()
        {
            GUI.enabled = Sparta.Target != null && Sparta.Target.Valid && _progressHandler == null;

            if(GUILayout.Button(new GUIContent("Synchronize", "Synchronize modules information between projects")))
            {
                Sparta.FetchInfo();
                RefreshModules();
            }

            // Enable actions after synchronize categories and modules
            GUI.enabled &= Synchronized;

            GUILayout.BeginHorizontal();
            if(GUILayout.Button(new GUIContent("Backport", "Copy back target project changes")))
            {
                if(EditorUtility.DisplayDialog("Backport target changes", 
                       "Override Sparta with target project changes?", 
                       "Backport", 
                       "Cancel"))
                {
                    if(Sparta.Target.LastEntry == null ||
                       Sparta.Target.LastEntry.RepoInfo.Commit == Sparta.RepoInfo.Commit ||
                       EditorUtility.DisplayDialog("Different commit", 
                           "Target project was updated from a different library commit. Backport could be inconsistent.", 
                           "Backport", 
                           "Cancel"))
                    {
                        AsyncProcess.Start(progress => {
                            progress.Update("Fetching repository info", 0.1f);
                            Sparta.FetchInfo();
                            progress.Update("Backporting modules", 0.5f);
                            SyncTools.BackportModules(Sparta.Target.ProjectPath, _modules);
                            progress.Finish();
                        });
                        RefreshModules();
                    }
                }
            }
            if(GUILayout.Button(new GUIContent("Update", "Override selected modules in target project")))
            {
                if(EditorUtility.DisplayDialog("Update target project", 
                       "Override target project with Sparta code?", 
                       "Override", 
                       "Cancel"))
                {
                    AsyncProcess.Start(progress => {
                        progress.Update("Fetching repository info", 0.1f);
                        Sparta.FetchInfo();
                        progress.Update("Updating modules", 0.5f);
                        SyncTools.UpdateModules(Sparta.Target.ProjectPath, _modules);
                        progress.Finish();
                    });
                    RefreshModules();
                }
            }
            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }

        void GUIModuleStatus(ModuleSync sync)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(new GUIContent(sync.Name, string.Format("{0}.\n{1} module.\n{2}", sync.ReferenceModule.Description, sync.Type, sync.Path)), 
                   sync.ReferenceModule.Valid ? EditorStyles.label : Styles.Warning))
            {
                Sparta.SelectedModule = sync;
            }

            GUILayout.Label(sync.Status.ToString(), Styles.ModuleStatus);

            GUI.enabled = !sync.ReferenceModule.IsMandatory || Sparta.AdvancedMode;
            sync.Action = (ModuleSync.SyncAction)EditorGUILayout.Popup(string.Empty, (int)sync.Action, 
                sync.Status == ModuleSync.SyncStatus.NotInstalled ? NotInstalledModuleOptions : InstalledModuleOptions, 
                Styles.PopupLayoutOptions);
            GUI.enabled = true;

            GUILayout.EndHorizontal();

            // Show dependencies
            bool first = true;
            var builder = new StringBuilder();
            if(sync.ReferenceModule.Dependencies.Count > 0)
            {
                GUILayout.BeginVertical();
                foreach(var dependency in sync.ReferenceModule.Dependencies)
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

        #endregion
    }
}
