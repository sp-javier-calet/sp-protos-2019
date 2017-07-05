using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace SpartaTools.Editor.Build
{
    public static class AutoBuilder
    {
        static BuildOptions _options = BuildOptions.None;
        const BuildOptions _appendFlag = BuildOptions.AcceptExternalModificationsToPlayer;
        static StringBuilder _detailedOutput;

        public static bool IsRunning;

        static string ProjectName
        {
            get
            {
                // Returns main project folder as ProjectName
                string[] s = Application.dataPath.Split(Path.DirectorySeparatorChar);
                return s[s.Length - 2];
            }
        }

        static string[] ActiveScenes
        {
            get
            {
                var scenes = new List<string>();
                foreach(var scene in EditorBuildSettings.scenes)
                {
                    if(scene.enabled)
                    {
                        scenes.Add(scene.path);
                    }
                }
                return scenes.ToArray();
            }
        }

        static string GetCommandLineArg(string name, string defaultValue)
        {
            string value = defaultValue;
            string[] arguments = Environment.GetCommandLineArgs();
            foreach(var arg in arguments)
            {
                string argName = "+" + name + "=";
                if(arg.StartsWith(argName))
                {
                    value = arg.Substring(argName.Length);
                    break;
                }
            }

            return value;
        }

        static bool IsUsingCommandLineArg(string name)
        {
            string[] arguments = Environment.GetCommandLineArgs();
            foreach(var arg in arguments)
            {
                string argName = "+" + name;
                if(arg.Equals(argName))
                {
                    return true;
                }
            }

            return false;
        }

        static string GetDefaultLocationForTarget(BuildTarget target)
        {
            switch(target)
            {
            case BuildTarget.Android:
                return "Builds/Android";
            case BuildTarget.iOS:
                return "Builds/iOS";
            case BuildTarget.tvOS:
                return "Builds/tvOS";
            case BuildTarget.StandaloneOSXIntel:
                return "Builds/OSX-Intel";
            default:
                throw new NotSupportedException("Unsupported platform " + target);
            }
        }

        static string GetLocationForTarget(BuildTarget target, string outputPath, string projectName)
        {
            if(string.IsNullOrEmpty(outputPath))
            {
                outputPath = GetDefaultLocationForTarget(target);
            }

            string path;

            switch(target)
            {
            case BuildTarget.Android:
                path = Path.Combine(outputPath, projectName + ".apk");
                break;
            case BuildTarget.iOS:
            case BuildTarget.tvOS:
                path = outputPath;
                break;
            case BuildTarget.StandaloneOSXIntel:
                path = Path.Combine(outputPath, projectName + ".app");
                break;
            default:
                throw new NotSupportedException("Unsupported platform " + target);
            }

            var directory = Path.GetDirectoryName(path);
            if(!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return path;
        }

        static void SetTarget(BuildTarget target)
        {
            // Select target platform
            EditorUserBuildSettings.SwitchActiveBuildTarget(target);

            if(target == BuildTarget.Android)
            {
                EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ETC;
                PlayerSettings.Android.targetDevice = AndroidTargetDevice.ARMv7;

                Log("Defined Android compilation for ARMv7 with ETC1 compression");
            }

            Log(string.Format("Defined Build Target: '{0}'", EditorUserBuildSettings.activeBuildTarget));
        }

        static void OverrideVersion(string version)
        {
            if(!string.IsNullOrEmpty(version))
            {
                PlayerSettings.bundleVersion = version;
            }

            Log(string.Format("Defined bundle version: '{0}'", PlayerSettings.bundleVersion));
        }

        static void OverrideBuiltSetOptions(BuildTarget target, string location, BuildSet buildSet, bool appendBuild)
        {
            _options = buildSet.Options;

            if(appendBuild)
            {
                _options |= _appendFlag;
            }

            var canAppendBuild = UnityEditorInternal.InternalEditorUtility.BuildCanBeAppended(target, location) == UnityEditorInternal.CanAppendBuild.Yes;
            var optionsHasAppendFlag = (_options & _appendFlag) == _appendFlag;
            if(optionsHasAppendFlag && (target == BuildTarget.Android || buildSet.IsShippingConfig || !canAppendBuild))
            {
                _options = _options & ~_appendFlag;
                Log("Append flag is not allowed, it will not be used");
            }

            Log(string.Format("Building player with options: '{0}'", _options));
        }

        static void OverrideBuildNumber(int buildNumber)
        {
            // Set build number. May be overriden by Build Set.
            if(buildNumber == 0)
            {
                buildNumber = Int32.Parse(DateTime.Today.ToString("yyMMddhhmm"));
            }

            if(buildNumber > 0)
            {
                PlayerSettings.Android.bundleVersionCode = buildNumber;
                PlayerSettings.iOS.buildNumber = buildNumber.ToString();
            }

            Log(string.Format("Defined Build Number (Bundle Version Code): '{0}'", buildNumber));
        }

        static BuildSet LoadBuildSetByName(string buildSetName)
        {
            var buildSet = BuildSet.Load(buildSetName);
            if(buildSet == null)
            {
                throw new FileNotFoundException("No Build Set found for name: " + buildSetName); 
            }

            return buildSet;
        }

        static void Log(string message)
        {
            Debug.Log(string.Format("Sparta-Autobuilder: {0}", message));
        }

        #region Public builder interface

        public static bool IsAppendingBuild
        {
            get
            {
                bool enabled = (_options & _appendFlag) == _appendFlag;
                return enabled;
            }
        }

        public static void BuildWithArgs()
        {
            // Parse Build number argument
            int versionNumber;
            try
            {
                versionNumber = Int32.Parse(GetCommandLineArg("build", null));
            }
            catch(Exception e)
            {
                throw new ArgumentException("A valid build number must be provided for the 'build' argument", e);
            }

            // Parse optional arguments
            var versionName = GetCommandLineArg("version", string.Empty);
            var builSetName = GetCommandLineArg("config", BuildSet.DebugConfigName);
            var outputPath = GetCommandLineArg("output", string.Empty);
            var appendBuild = IsUsingCommandLineArg("append");

            // Launch build
            Build(EditorUserBuildSettings.activeBuildTarget, builSetName, appendBuild, versionNumber, versionName, outputPath);
        }

        /// <summary>
        /// Build project for the specified target and config.
        /// </summary>
        /// <param name="target">Target Platform</param>
        /// <param name="buildSetName">Build set name to apply before compiling</param>
        /// <param name = "appendBuild">Append Build Option. If true adds the append option to the builder.</param>
        /// <param name="versionNumber">Version number. If zero, local current timestamp will be used. If negative, it will use the one defined in Unity Player Settings. </param>
        /// <param name="versionName">Short Version Name. If it is null or empty, it will use the one defined in Unity Player Settings. </param>
        /// <param name="outputPath">Output location. </param>
        public static void Build(BuildTarget target, string buildSetName, bool appendBuild = false, int versionNumber = -1, string versionName = null, string outputPath = null)
        {
            IsRunning = true;

            _detailedOutput = new StringBuilder();
            Application.logMessageReceived += LogMessageReceivedCallback;

            Log(string.Format("Starting Build <{0}> for target <{1}> with config set <{2}>", 
                versionNumber, target, buildSetName));

            if(BuildPipeline.isBuildingPlayer)
            {
                throw new InvalidOperationException("Already building a player");
            }

            SetTarget(target);

            var buildSet = LoadBuildSetByName(buildSetName);
            Log(string.Format("Applying '{0}' Build Set with extended features...", buildSet.Name));
            buildSet.ApplyExtended();

            /*
             * Settings Override
             */ 
            if(!buildSet.App.OverrideBuild)
            {
                OverrideBuildNumber(versionNumber);
            }

            OverrideVersion(versionName);

            /*
             * Start build
             */ 
            string[] activeScenes = ActiveScenes;
            Log(string.Format("Building player with active scenes: '{0}'", string.Join(", ", activeScenes)));

            var location = GetLocationForTarget(target, outputPath, ProjectName);
            Log(string.Format("Building player in path '{0}", location));

            OverrideBuiltSetOptions(target, location, buildSet, appendBuild);

            Log(string.Format("Unity version: {0}", Application.unityVersion));

            //Dump config report after apply config
            new BuildReport()
                .CollectBaseSettings()
                .AddBuildSetInfo(buildSet)
                .CollectPlayerSettings()
                .Dump();
                
            Log("Starting Player Build");

            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = activeScenes;
            buildPlayerOptions.locationPathName = location;
            buildPlayerOptions.target = target;
            buildPlayerOptions.options = _options;
            string result = BuildPipeline.BuildPlayer(buildPlayerOptions);

            BackupTempFolder();

            _options = BuildOptions.None;
            IsRunning = false;

            Log(string.Format("Detailed log:\n '{0}' \nEnd of detailed log\n", _detailedOutput));
            Application.logMessageReceived -= LogMessageReceivedCallback;

            if(!string.IsNullOrEmpty(result))
            {
                Log(string.Format("Player Build finished with error result: '{0}'", result));
                throw new CompilerErrorException(result);
            }
            Log("Player Build finished successfully");
        }

        static void LogMessageReceivedCallback(string msg, string stack, LogType type)
        {
            if(type == LogType.Error || type == LogType.Exception || type == LogType.Assert)
            {
                _detailedOutput.AppendFormat("{0} - {1}\n{2}\n\n", type, msg, stack);
            }
        }

        #endregion

        #region BackupTempFolder and Directory Utils methods.

        static void BackupTempFolder()
        {
            //Save TempFolder in order to backup debug symbols
            string tempFolder = Path.Combine(Application.dataPath, "../Temp");
            string tempFolderBackup = Path.Combine(Application.dataPath, "../TempBackup");

            // We first delete the previous backup folder to allow copying or moving files without exceptions.
            if(Directory.Exists(tempFolderBackup))
            {
                DeleteDirectory(tempFolderBackup);
            }

            if(Directory.Exists(tempFolder))
            {
                CopyDirectory(tempFolder, tempFolderBackup, true);
            }
        }

        // Directory.Delete(targetDir, true) is not enough in .NET 3.5
        static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            for(int i = 0, filesLength = files.Length; i < filesLength; i++)
            {
                string file = files[i];
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            for(int i = 0, dirsLength = dirs.Length; i < dirsLength; i++)
            {
                string dir = dirs[i];
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }

        // We perform a full depth copy instead of only rename (move) the folder with Directory.Move.
        static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);

            if(!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if(!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            for(int i = 0, filesLength = files.Length; i < filesLength; i++)
            {
                FileInfo file = files[i];
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, false);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if(copySubDirs)
            {
                for(int i = 0, dirsLength = dirs.Length; i < dirsLength; i++)
                {
                    DirectoryInfo subdir = dirs[i];
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        #endregion
    }
}

