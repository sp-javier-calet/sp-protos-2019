using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Diagnostics;

namespace SpartaTools.Editor.Sync
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
        static RepositoryInfo _repoInfo = GetRepositoryInfo();

        public static RepositoryInfo RepoInfo
        {
            get
            {
                return _repoInfo;
            }
        }

        /*
         * Sparta Project Base Path
         */
        static string _basePath;

        public static string BasePath
        {
            get
            {
                if(string.IsNullOrEmpty(_basePath))
                {
                    _basePath = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
                }
                return _basePath;
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
            _repoInfo = GetRepositoryInfo();
        }


        #region Repository Info

        public class RepositoryInfo
        {
            public string Commit { get; private set; }

            public string User { get; private set; }

            public string Branch { get; private set; }

            public RepositoryInfo() : this("No commit", "No branch", "No user")
            {
            }

            public RepositoryInfo(string commit, string branch, string user)
            {
                Commit = commit;
                Branch = branch;
                User = user;
            }
        }

        static RepositoryInfo GetRepositoryInfo()
        {
            string commit = null;
            RunProcess("git", "log --pretty=format:'%H' -n 1", BasePath, line => {
                commit = line.Trim();
            });

            string branch = null;
            RunProcess("git", "rev-parse --abbrev-ref HEAD", BasePath, line => {
                branch = line.Trim();
            });

            string user = null;
            RunProcess("git", "config user.email", BasePath, line => {
                user = line.Trim();
            });

            return new RepositoryInfo(commit, branch, user);
        }

        static int RunProcess(string exe, string args, string path, Action<string> output)
        {
            var proc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = false,
                    CreateNoWindow = true,
                    WorkingDirectory = path
                }
            };
            proc.Start();
            while(!proc.StandardOutput.EndOfStream)
            {
                string line = proc.StandardOutput.ReadLine();
                if(output != null)
                {
                    output(line);
                }
            }
            proc.WaitForExit();
            int code = proc.ExitCode;
            proc.Close();
            return code;
        }

        #endregion
    }
}
