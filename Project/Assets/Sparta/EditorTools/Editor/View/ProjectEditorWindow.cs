using UnityEngine;
using UnityEditor;
using System.IO;
using SpartaTools.Editor.SpartaProject;

namespace SpartaTools.Editor.View
{
    public class ProjectEditorWindow : EditorWindow
    {
        Vector2 _scrollPosition;
        string _inputPath;
        string _fileContent;
        bool _showRawFile;
        bool _showLog;
        bool _editEnabled;

        #region Editor options

        [MenuItem("Sparta/Project/Project Info", false, 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow(typeof(ProjectEditorWindow), false, "Project", true);
        }

        #endregion

        ProjectEditorWindow()
        {
            _inputPath = Sparta.Target.ProjectPath;
            Sparta.OnChanged += OnSpartaChanged;   
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

        #region Draw GUI

        void OnEnable()
        {
            titleContent = new GUIContent("Project", Sparta.Icon, "Sparta Target project editor");
        }

        void GUIToolbar()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            GUILayout.FlexibleSpace();
            _editEnabled = GUILayout.Toggle(_editEnabled, new GUIContent("Advanced Mode", "Enables edition mode for module files"), EditorStyles.toolbarButton);
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

                if(_editEnabled)
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
                foreach(var entry in Sparta.Target.Log)
                {
                    EditorGUILayout.SelectableLabel(string.Format("{0} - Updated by {1} on {2} - Local branch: {3}", 
                        entry.RepoInfo.Commit, entry.RepoInfo.User, entry.Time, entry.RepoInfo.Branch));
                }
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

