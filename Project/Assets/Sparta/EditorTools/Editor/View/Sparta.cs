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

                SelectedModule = null;

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
        static ModuleSync _selectedModule;

        public static ModuleSync SelectedModule
        {
            get
            {
                return _selectedModule;
            }
            set
            {
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

        /*
         * Advanced Mode configuration
         */
        static bool _advancedMode;

        public static bool AdvancedMode
        { 
            get
            { 
                return _advancedMode; 
            }
            set
            {
                _advancedMode = value;
                if(OnChanged != null)
                {
                    OnChanged(); 
                }
            }
        }

        public static void FetchInfo()
        {
            _repoInfo = Target.GetRepositoryInfo();
        }
    }
}
