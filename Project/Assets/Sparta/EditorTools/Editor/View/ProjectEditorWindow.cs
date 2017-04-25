using UnityEngine;
using UnityEditor;
using System.IO;
using SpartaTools.Editor.SpartaProject;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.View
{
    public class ProjectEditorWindow : EditorWindow
    {
        Vector2 _scrollPosition;
        string _inputPath;
        string _fileContent;
        string _mergeLogContent;
        bool _showRawFile;
        bool _showLog;
        bool _showMergeLog;

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

        #region Editor options

        [MenuItem("Sparta/Project/Project Info", false, 102)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ProjectEditorWindow), false, "Project", true);
        }

        #endregion

        ProjectEditorWindow()
        {
            Sparta.OnChanged += OnSpartaChanged;
        }

        void OnEnable()
        {
            _inputPath = Sparta.Target.ProjectPath;
        }

        ~ProjectEditorWindow()
        {
            Sparta.OnChanged -= OnSpartaChanged;
        }

        void OnSpartaChanged()
        {
            _inputPath = Sparta.Target.ProjectPath;
            Repaint();
        }

        void ClearContent()
        {
            _fileContent = null;
            _mergeLogContent = null;
        }

        #region Draw GUI

        void OnFocus()
        {
            RefreshIcon();
        }

        void RefreshIcon()
        {
            Sparta.SetIcon(this, "Project", "Sparta Target project editor", EditEnabled);
        }

        void GUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            EditEnabled = GUILayout.Toggle(EditEnabled, new GUIContent("Advanced Mode", "Enables edition mode for module files"), EditorStyles.toolbarButton);
            GUILayout.EndHorizontal();
        }

        void OnGUI()
        {
            GUIToolbar();

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            GUIProjectPathInput();
            GUIRepositoryStatus();

            if(Sparta.Target.Valid)
            {
                GUIProjectLog();
                GUIMergeLog();

                if(EditEnabled)
                {
                    GUIFileEditor();
                }
            }
            else
            {
                GUILayout.Label("No project selected");
            }

            EditorGUILayout.EndScrollView();
        }

        void GUIProjectPathInput()
        {
            // Target Project Path input
            GUILayout.Label("Target Project", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            var path = _inputPath;
            path = EditorGUILayout.TextField("Project path", _inputPath);
            if(GUILayout.Button("Browse", GUILayout.MaxWidth(60)))
            {
                path = EditorUtility.OpenFolderPanel("Select Target Project", 
                    Sparta.Target.ProjectPath,
                    Sparta.Target.ProjectPath);

                // Check for cancelled popup
                if(string.IsNullOrEmpty(path))
                {
                    path = _inputPath;
                }
            }

            if(GUILayout.Button("Refresh", GUILayout.MaxWidth(60)) ||
               path != Sparta.Target.ProjectPath)
            {
                Sparta.Target = new Project(path);
                ClearContent();
            }

            EditorGUILayout.EndHorizontal();

            // Validate project path indicator
            if(Sparta.Target.Valid)
            {
                GUILayout.Label("Is a valid Unity Project path", Styles.ValidProjectLabel);
            }
            else
            {
                GUILayout.Label("Path is not a Unity Project", Styles.InvalidProjectLabel);
            }
        }

        void GUIRepositoryStatus()
        {
            var spartaInfo = Sparta.RepoInfo;
            RepositoryInfo targetInfo;

            if(Sparta.Target.Valid && Sparta.Target.LastEntry != null)
            {
                targetInfo = Sparta.Target.LastEntry.RepoInfo;
            }
            else
            {
                targetInfo = new RepositoryInfo();
            }


            EditorGUILayout.BeginHorizontal();
            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label("", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel("Commit", Styles.TableLabel, Styles.TableLabelOptions);
            EditorGUILayout.SelectableLabel("Branch", Styles.TableLabel, Styles.TableLabelOptions);
            EditorGUILayout.SelectableLabel("User", Styles.TableLabel, Styles.TableLabelOptions);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label("Sparta", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(spartaInfo.Commit, Styles.TableContent);
            EditorGUILayout.SelectableLabel(spartaInfo.Branch, Styles.TableContent);
            EditorGUILayout.SelectableLabel(spartaInfo.User, Styles.TableContent);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(Styles.Group);
            GUILayout.Label("Target project", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(targetInfo.Commit, Styles.TableContent);
            EditorGUILayout.SelectableLabel(targetInfo.Branch, Styles.TableContent);
            EditorGUILayout.SelectableLabel(targetInfo.User, Styles.TableContent);
            GUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        void GUIProjectLog()
        {
            _showLog = EditorGUILayout.Foldout(_showLog, "Project Log");
            if(_showLog)
            {
                GUILayout.BeginVertical(Styles.Group);
                GUILayout.BeginVertical(EditorStyles.textArea);
                foreach(var entry in Sparta.Target.Log)
                {
                    EditorGUILayout.SelectableLabel(string.Format("{0} - Updated by {1} on {2} - Local branch: {3}", 
                        entry.RepoInfo.Commit, entry.RepoInfo.User, entry.Time, entry.RepoInfo.Branch));
                }
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
        }

        void GUIMergeLog()
        {
            _showMergeLog = EditorGUILayout.Foldout(_showMergeLog, new GUIContent("Merge Log", "Last 20 merges on master branch since last update"));
            if(_showMergeLog)
            {
                if(string.IsNullOrEmpty(_mergeLogContent))
                {
                    var repository = new Repository(Sparta.Current.ProjectPath);
                    var logQuery = repository.CreateLogQuery();

                    var target = Sparta.Target;
                    if(target != null && target.LastEntry != null)
                    {
                        logQuery.Since(Sparta.Target.LastEntry.Time);
                    }

                    logQuery.WithOption("merges", "master")
                            .WithLimit(20);

                    _mergeLogContent = logQuery.Exec();
                }

                GUILayout.BeginVertical(Styles.Group);
                GUILayout.TextArea(_mergeLogContent);
                GUILayout.EndVertical();
            }
        }

        void GUIFileEditor()
        {
            _showRawFile = EditorGUILayout.Foldout(_showRawFile, "Raw Project file");
            if(_showRawFile)
            {
                GUILayout.BeginVertical(Styles.Group);
                if(string.IsNullOrEmpty(_fileContent))
                {
                    LoadProjectFileContent();
                }

                _fileContent = GUILayout.TextArea(_fileContent);

                GUILayout.BeginHorizontal(Styles.Group);
                if(GUILayout.Button("Reload", GUILayout.MaxWidth(60)))
                {
                    LoadProjectFileContent();
                }
				
                if(GUILayout.Button("Save", GUILayout.MaxWidth(60)))
                {
                    File.WriteAllText(Sparta.Target.ProjectFilePath, _fileContent);
                }
				
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
        }

        void LoadProjectFileContent()
        {
            if(Sparta.Target.Valid)
            {
                _fileContent = File.ReadAllText(Sparta.Target.ProjectFilePath);
            }
            else
            {
                _fileContent = string.Empty;
            }

            Repaint();
        }

        #endregion
    }
}

