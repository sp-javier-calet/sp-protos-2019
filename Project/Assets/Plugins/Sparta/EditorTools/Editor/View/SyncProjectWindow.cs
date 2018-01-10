using UnityEngine;
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

        [MenuItem("Sparta/Project/Sync Tools", false, 103)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(SyncProjectWindow), false, "Sync", true);
        }

        [MenuItem("Sparta/Project/Create module...", false, 120)]
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
            if(_autoRefresh)
            {
                RefreshModules();
                Repaint();
            }
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
        readonly string[] InstalledModuleOptions = {
            "None",
            "Override",
            "Uninstall" 
        };

        readonly string[] NotInstalledModuleOptions = {
            "None",
            "Install"
        };

        IList<ModuleSync> _modules = new List<ModuleSync>();
        IList<ModuleSyncCategory> _categories = new List<ModuleSyncCategory>();
        Vector2 _scrollPosition = Vector2.right;
        ProgressHandler _progressHandler;
        bool _refreshFinished;

        bool _editEnabled;

        bool EditEnabled
        {
            set
            {
                bool changed = _editEnabled != value;
                _editEnabled = value;
                if(changed)
                {
                    RefreshIcon();
                }
            }
            get
            {
                return _editEnabled;
            }
        }

        //Windows doesn't work with autorefresh enabled when opening the editorwindow.
        #if UNITY_EDITOR_WIN
        bool _autoRefresh = false;
        #else
        bool _autoRefresh = true;
        #endif

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

                    if(modules != null)
                    {
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
                    }
                    _refreshFinished = true;
                });
            }
        }

        #region Draw GUI

        void OnFocus()
        {
            RefreshIcon();
        }

        void RefreshIcon()
        {
            Sparta.SetIcon(this, "Sync", "Sparta Project Sync", EditEnabled);
        }

        void OnInspectorUpdate()
        {
            if(_progressHandler != null)
            {
                Repaint();
            }
        }

        void GUIToolbar()
        {
            GUI.enabled = Sparta.Target != null && Sparta.Target.Valid && _progressHandler == null;

            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if(GUILayout.Button(new GUIContent("Synchronize", "Synchronize modules information between projects"), EditorStyles.toolbarButton))
            {
                Sparta.FetchInfo();
                RefreshModules(); 
            }

            EditorGUILayout.Space();
            // Enable actions after synchronize categories and modules
            GUI.enabled &= Synchronized;
            if(GUILayout.Button(new GUIContent("Backport", "Copy back target project changes"), EditorStyles.toolbarButton))
            {
                if(EditorUtility.DisplayDialog("Backport target changes", 
                       "Override Sparta with target project changes?", 
                       "Backport", 
                       "Cancel"))
                {
                    bool doBackport = Sparta.Target.LastEntry == null ||
                                      Sparta.Target.LastEntry.RepoInfo.Commit == Sparta.RepoInfo.Commit;

                    // If is not a clean backport, ask for actions
                    if(!doBackport)
                    {
                        if(EditorUtility.DisplayDialog("Different commit", 
                               "Target project was updated from a different library commit. Do you want to checkout this commit? This will remove any change in the library working copy.",
                               "Skip", 
                               "Checkout"))
                        {
                            doBackport |= EditorUtility.DisplayDialog("Different commit", "Target project was updated from a different library commit. Backport could be inconsistent.", "Backport", "Cancel");
                        }
                        else
                        {
                            var repository = new Repository(Sparta.Current.ProjectPath);
                            var commit = Sparta.Target.LastEntry.RepoInfo.Commit;
                            var result = repository.ResetToCommit(commit);
                            EditorUtility.DisplayDialog("Reset to target commit", result, "Ok");
                        }
                    }

                    // Backport
                    if(doBackport)
                    {
                        AsyncProcess.Start(progress => {
                            progress.Update("Fetching repository info", 0.1f);
                            Sparta.FetchInfo();
                            progress.Update("Backporting modules", 0.5f);
                            SyncTools.BackportModules(Sparta.Target.ProjectPath, _modules);
                            progress.Finish();
                        });

                        if(_autoRefresh)
                        {
                            RefreshModules();
                        }
                    }

                }
            }

            if(GUILayout.Button(new GUIContent("Update", "Override selected modules in target project"), EditorStyles.toolbarButton))
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
                        SyncTools.UpdateModules(Sparta.Target.ProjectPath, _modules, Sparta.Current.GetRepositoryInfo());
                        progress.Finish();
                    });

                    if(_autoRefresh)
                    {
                        RefreshModules();
                    }
                }
            }

            GUILayout.FlexibleSpace();
            _autoRefresh = GUILayout.Toggle(_autoRefresh, new GUIContent("Auto Refresh", "Synchronize automatically"), EditorStyles.toolbarButton);
            EditEnabled = GUILayout.Toggle(EditEnabled, new GUIContent("Advanced Mode", "Enable manual sync configuration"), EditorStyles.toolbarButton);

            GUILayout.EndHorizontal();
            GUI.enabled = true;
        }


        void OnGUI()
        {
            GUIToolbar();

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
                }
                else if(_progressHandler.Cancelled && _refreshFinished)
                {
                    // Disable autorefresh if needed, to avoid a new sync after cancel
                    _autoRefresh = Synchronized && _autoRefresh;

                    _progressHandler = null;
                }
                else
                {
                    var rect = GUILayoutUtility.GetRect(position.width - 6, 20);
                    EditorGUI.ProgressBar(rect, _progressHandler.Percent, _progressHandler.Message);
                    if(GUILayout.Button("Cancel"))
                    {
                        _progressHandler.Cancel();
                        Close();
                    }
                }
            }
            else if(!Synchronized && _autoRefresh)
            {
                RefreshModules();
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUILayout.Label("Registered Modules", EditorStyles.boldLabel);

            GUIRegisteredModules();

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

        void GUIModuleStatus(ModuleSync sync)
        {
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            if(GUILayout.Button(new GUIContent(sync.Name, string.Format("{0}.\n{1} module.\n{2}", sync.ReferenceModule.Description, sync.Type, sync.Path)), 
                   sync.ReferenceModule.Valid ? EditorStyles.label : Styles.Warning))
            {
                Sparta.SelectedModuleSync = sync;
            }

            GUILayout.Label(sync.Status.ToString(), Styles.ModuleStatus);

            GUI.enabled = !sync.ReferenceModule.IsMandatory || EditEnabled;
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
