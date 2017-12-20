using UnityEngine;
using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.SpartaProject
{
    public class Project
    {
        static Project()
        {
            _dataPath = Application.dataPath;
        }
        /*
         * Sparta Project Base Path
         */
        static string _dataPath;
        static string _basePath;

        public static string BasePath
        {
            get
            {
                if(string.IsNullOrEmpty(_basePath))
                {
                    _basePath = Path.GetFullPath(Path.Combine(_dataPath, ".."));
                }
                return _basePath;
            }
        }

        const string ProjectFileName = ".sparta_project";

        const string DefaultCultureName = "en-US";

        public bool Valid { get; private set; }

        public string ProjectPath { get; private set; }

        public string ProjectFilePath
        {
            get
            {
                return Path.Combine(ProjectPath, ProjectFileName);
            }
        }

        public class LogEntry
        {
            public DateTime Time;
            public RepositoryInfo RepoInfo;

            public LogEntry(DateTime time, RepositoryInfo repoInfo)
            {
                Time = time;
                RepoInfo = repoInfo;
            }

            public LogEntry(string serializedEntry)
            {
                var parts = serializedEntry.Split(new char[]{ ';' }, 4);
                Time = DateTime.Parse(parts[0], new CultureInfo(DefaultCultureName));
                var commit = parts[1];
                var branch = parts[2];
                var user = parts[3];

                RepoInfo = new RepositoryInfo(commit, branch, user);
            }

            public override string ToString()
            {
                return string.Format("{0};{1};{2};{3}", Time.ToString(new CultureInfo(DefaultCultureName)), RepoInfo.Commit, RepoInfo.Branch, RepoInfo.User);
            }
        }

        public IEnumerable<LogEntry> Log
        {
            get
            {
                return _log;
            }
        }

        readonly List<LogEntry> _log;

        public LogEntry LastEntry { get; private set; }

        public Project(string path)
        {
            ProjectPath = path;
            _log = new List<LogEntry>();
            Valid = IsUnityProjectFolder;
            if(Exists)
            {
                Load();
            }
        }

        public void Initialize()
        {
            File.Create(Path.Combine(ProjectPath, ProjectFileName)).Close();
            Valid = true;
        }

        public bool Exists
        {
            get
            {
                return File.Exists(Path.Combine(ProjectPath, ProjectFileName));
            }
        }

        public bool IsUnityProjectFolder
        {
            get
            {
                return 	Directory.Exists(ProjectPath) &&
                Directory.Exists(Path.Combine(ProjectPath, "Assets")) &&
                Directory.Exists(Path.Combine(ProjectPath, "Library"));
            }
        }

        public void AddLog(DateTime time, RepositoryInfo repoInfo)
        {
            _log.Add(new LogEntry(time, repoInfo));
        }

        void Sort()
        {
            _log.Sort((l, r) => r.Time.CompareTo(l.Time));
        }

        void Load()
        {
            var file = new StreamReader(Path.Combine(ProjectPath, ProjectFileName));
            var line = file.ReadLine();
            while(line != null)
            {
                if(!string.IsNullOrEmpty(line))
                {
                    _log.Add(new LogEntry(line));
                }
                line = file.ReadLine();
            }
            file.Close();

            Sort();
            if(_log.Count > 0)
            {
                LastEntry = _log[0];
            }
        }

        public void Save()
        {
            var file = File.CreateText(Path.Combine(ProjectPath, ProjectFileName));
            Sort();
            foreach(var entry in _log)
            {
                file.WriteLine(entry);
            }
            file.Flush();
            file.Close();
        }

        public Dictionary<string, Module> GetModules()
        {
            return GetModules(ProjectPath);
        }

        public RepositoryInfo GetRepositoryInfo()
        {
            Repository repository = new Repository(ProjectPath);
            return new RepositoryInfo(repository.GetCommit(), repository.GetBranch(), repository.GetUser());
        }

        /// <summary>
        /// Gets the project modules.
        /// </summary>
        /// <returns>A dictionary containing the project modules, indexing by module name.</returns>
        /// <param name="projectPath">Project path.</param>
        public static Dictionary<string, Module> GetModules(string projectPath)
        {
            var dic = new Dictionary<string, Module>();

            // TODO Implement own recursive search, stopping when a module is found to avoid conflicts.
            string[] files = Directory.GetFiles(projectPath, Module.DefinitionFileName, SearchOption.AllDirectories);
            foreach(var moduleFile in files)
            {
                var module = new Module(projectPath, moduleFile);
                try
                {
                    dic.Add(module.Name, module);
                }
                catch(Exception e)
                {
                    throw new Exception(string.Format("Duplicated module with name {0} in {1} and {2}. Error: {3}", 
                        module.Name, module.RelativePath, dic[module.Name].RelativePath, e.Message));
                }
            }

            return dic;
        }
    }
}
