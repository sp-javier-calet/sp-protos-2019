using UnityEngine;
using UnityEditor;
using System;
using SpartaTools.Editor.Sync;
using SpartaTools.Editor.SpartaProject;

namespace SpartaTools.Editor.View
{
    public static class Sparta
    {
        public static event Action OnChanged;
        public static event Action OnModuleSelected;

        /*
         * Target Project
         */
        const string TargetProjectKey = "SpartaSyncTargetProject";


        static string _targetPath;

        static Project _current;

        public static Project Current
        {
            get
            {
                if(_current == null)
                {
                    _current = new Project(Project.BasePath);
                }
                return _current;
            }
        }

        static Project _target;

        public static Project Target
        {
            get
            {
                if(_target == null)
                {
                    _targetPath = EditorPrefs.GetString(TargetProjectKey);
                    _target = new Project(_targetPath);
                }
                return _target;
            }
            set
            {
                _target = value;

                SelectedModuleSync = null;

                FetchInfo();

                EditorPrefs.SetString(TargetProjectKey, _target.ProjectPath);

                if(OnChanged != null)
                {
                    OnChanged();
                }
            }
        }

        /*
         * Module selection
         */
        static ModuleSync _selectedModuleSync;

        public static ModuleSync SelectedModuleSync
        {
            get
            {
                return _selectedModuleSync;
            }
            set
            {
                _selectedModuleSync = value;
                SelectedModule = _selectedModuleSync != null ? _selectedModuleSync.ReferenceModule : null;
            }
        }

        static Module _selectedModule;

        public static Module SelectedModule
        {
            get
            {
                return _selectedModule;
            }
            set
            {
                if(_selectedModuleSync != null && _selectedModuleSync.ReferenceModule != value)
                {
                    _selectedModuleSync = null;
                }
                _selectedModule = value;
                if(OnModuleSelected != null)
                {
                    OnModuleSelected();
                }
            }
        }

        /*
         * Sparta repository info
         */
        static RepositoryInfo _repoInfo;

        public static RepositoryInfo RepoInfo
        {
            get
            {
                if(_repoInfo == null)
                {
                    _repoInfo = Current.GetRepositoryInfo();
                }
                return _repoInfo;
            }
        }

        public static void FetchInfo()
        {
            _repoInfo = Current.GetRepositoryInfo();
        }

        public static void SetIcon(EditorWindow window, string title, string tooltip, bool advancedMode = false)
        {
            string paddingTitle = " " + title;
            window.titleContent = new GUIContent(paddingTitle, advancedMode ? AdvancedIcon : Icon, tooltip);
        }

        /*
         * Sparta editor window icon
         */

        static Texture _icon;

        static Texture Icon
        {
            get
            {
                if(_icon == null)
                {
                    const string iconName = "sparta.icon";
                    _icon = LoadTexture2D(iconName);
                }
                return _icon;
            }
        }

        static Texture _advancedIcon;

        static Texture AdvancedIcon
        {
            get
            {
                if(_advancedIcon == null)
                {
                    const string iconName = "sparta.icon.advanced";
                    _advancedIcon = LoadTexture2D(iconName);
                }
                return _advancedIcon;
            }
        }

        static Texture LoadTexture2D(string textureName)
        {
            const string type = "t:texture2d";
            var guids = AssetDatabase.FindAssets(string.Format("{0} {1}", textureName, type));
            foreach(var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            return null;
        }
    }
}
