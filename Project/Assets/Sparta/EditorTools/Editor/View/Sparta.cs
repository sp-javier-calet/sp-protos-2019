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


        static string _targetPath = EditorPrefs.GetString(TargetProjectKey);

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
                if(_selectedModuleSync != null)
                {
                    SelectedModule = _selectedModuleSync.ReferenceModule;
                }
                else
                {
                    SelectedModule = null;
                }
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
        static RepositoryInfo _repoInfo = Target.GetRepositoryInfo();

        public static RepositoryInfo RepoInfo
        {
            get
            {
                return _repoInfo;
            }
        }

        public static void FetchInfo()
        {
            _repoInfo = Target.GetRepositoryInfo();
        }

        /*
         * Sparta editor window icon
         */
        static Texture2D _icon;

        public static Texture Icon
        {
            get
            {
                if(_icon == null)
                {
                    _icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Sparta/EditorTools/Editor/EditorResources/sparta.icon.png");
                }
                return _icon;
            }
        }
    }
}
