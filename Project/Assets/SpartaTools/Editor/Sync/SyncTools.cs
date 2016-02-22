﻿using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using SpartaTools.Editor.Utils;

namespace SpartaTools.Editor.Sync
{
    public static class SyncTools
    {

        static readonly FileInfo[] EmptyFileList = new FileInfo[0];

        enum ProjectType
        {
            Source,
            Target
        }

        enum CopyAction
        {
            SourceToTarget,
            TargetToSource
        }

        static ProgressHandler CurrentProgress;

        /// <summary>
        /// Synchronize the specified projectPath.
        /// </summary>
        /// <returns>a List of ModuleSync, which contains the status of every module defined in both
        /// the Sparta and Target projects, and their differences.</returns>
        /// <param name="projectPath">Target Project path.</param>
        /// <param name="progressHandler">Progress handler, to communicate progress.</param>
        public static IList<ModuleSync> Synchronize(string projectPath, ProgressHandler progressHandler)
        {
            CurrentProgress = progressHandler;

            SyncReport.Start("Synchronize");
            SyncReport.Log("Synchronizing " + projectPath);

            var list = new List<ModuleSync>();

            CurrentProgress.Update("Retrieving Sparta modules", 0.05f);
            var spartaModules = GetProjectModules(Sparta.BasePath);

            CurrentProgress.Update("Retrieving Target modules", 0.05f);
            var targetModules = GetProjectModules(projectPath);

            float modulePercent = 0.9f / (spartaModules.Values.Count + targetModules.Values.Count + 1);
            /*
             * Add diffs for every module defined in sparta 
             */
            foreach(var spartaMod in spartaModules.Values)
            {
                CurrentProgress.Update(string.Format("Comparing {0}", spartaMod.Name), modulePercent);

                // Search for module in target
                Module targetModule = null;
                targetModules.TryGetValue(spartaMod.Name, out targetModule);

                // Compare modules between sparta and target project
                ModuleSync.SyncStatus status;
                var diffFiles = Compare(projectPath, spartaMod, out status);

                // Create a module sync
                var sync = new ModuleSync(spartaMod, targetModule, diffFiles, status);
                list.Add(sync);

                SyncReport.Log(string.Format("Found module {0}. Status {1}", spartaMod.Name, status));

                // If installed, remove from target modules list, which will be iterated later to find new modules
                if(targetModule != null)
                {
                    targetModules.Remove(spartaMod.Name);
                }
            }

            modulePercent = (1.0f - CurrentProgress.Percent) / (targetModules.Values.Count + 1);

            /*
             * Iterates over all remaining module in target. 
             * Since there are not in sparta, all of them will be marked as New.
             */
            foreach(var targetMod in targetModules.Values)
            {
                CurrentProgress.Update(string.Format("Comparing {0}", targetMod.Name), modulePercent);

                ModuleSync.SyncStatus status;
                var diffFiles = Compare(projectPath, targetMod, out status);
                var sync = new ModuleSync(null, targetMod, diffFiles, status);
                list.Add(sync);

                SyncReport.Log(string.Format("Found module {0}. Status {1}", targetMod.Name, status));
            }

            SyncReport.Dump();
            CurrentProgress.Finish();
            return list;
        }

        /// <summary>
        /// Creates a new module in the specified folder
        /// </summary>
        /// <returns><c>true</c>, if module was created, <c>false</c> otherwise.</returns>
        /// <param name="path">New module path. An sparta_module file will be created here, if possible.</param>
        public static bool CreateModule(string path)
        {
            var defPath = Path.Combine(Path.GetFullPath(path), Module.DefinitionFileName); 
            if(!File.Exists(defPath))
            {
                File.Create(defPath).Close();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates and Override the modules, copying from Sparta to the Target Project.
        /// </summary>
        /// <returns><c>true</c>, if modules was updated successfully, <c>false</c> otherwise.</returns>
        /// <param name="targetPath">Target project path.</param>
        /// <param name="modules">List of ModuleSync to proccess. Actions will be differents depending on the sync status.</param>
        public static bool UpdateModules(string targetPath, IList<ModuleSync> modules)
        {
            var spartaProject = new Project(targetPath);
            if(!spartaProject.Valid)
            {
                return false;
            }

            if(!spartaProject.Exists)
            {
                spartaProject.Initialize();
            }

            spartaProject.AddLog(DateTime.UtcNow, Sparta.RepoInfo);
            spartaProject.Save();

            foreach(var module in modules)
            {
                switch(module.Action)
                {
                case ModuleSync.SyncAction.Override:
                    SyncModule(module, targetPath, CopyAction.SourceToTarget);
                    break;

                case ModuleSync.SyncAction.Uninstall:
                    DeleteModule(Path.Combine(targetPath, module.Path));
                    foreach(var dep in module.ReferenceModule.Dependencies)
                    {
                        DeleteModule(Path.Combine(targetPath, dep));
                    }
                    break;
                }
            }

            return true;
        }

        /// <summary>
        /// Backports the modules. Override module files copying from Target Project to Sparta.
        /// </summary>
        /// <returns><c>true</c>, if modules was backported, <c>false</c> otherwise.</returns>
        /// <param name="targetPath">Target Project path.</param>
        /// <param name="modules">List of ModuleSync to proccess. Actions will be differents depending on the sync status.</param>
        public static bool BackportModules(string targetPath, IList<ModuleSync> modules)
        {
            foreach(var module in modules)
            {
                switch(module.Status)
                {
                case ModuleSync.SyncStatus.New:
                case ModuleSync.SyncStatus.HasChanges:
                    SyncModule(module, targetPath, CopyAction.TargetToSource);
                    break;
                }
            }
			
            return true;
        }

        /// <summary>
        /// Deletes the module definition.
        /// </summary>
        /// <returns><c>true</c>, if module was deleted, <c>false</c> otherwise.</returns>
        /// <param name="path">Module Path.</param>
        public static bool DeleteModule(string path)
        {
            if(!File.Exists(path) && !Directory.Exists(path))
            {
                return false;
            }

            FileAttributes attributes = File.GetAttributes(path);
            if((attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                Directory.Delete(path, true);
                return true;
            }
            else
            {
                File.Delete(path);
                return true;
            }
        }

        /// <summary>
        /// Gets the project modules.
        /// </summary>
        /// <returns>A dictionary containing the project modules, indexing by module name.</returns>
        /// <param name="projectPath">Project path.</param>
        static Dictionary<string, Module> GetProjectModules(string projectPath)
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

        /// <summary>
        /// Synchronize module folder, copying or removing files.
        /// </summary>
        /// <param name="moduleSync">Module sync with status and actions to perform.</param>
        /// <param name="targetPath">Target Project path.</param>
        /// <param name="action">Ssync direction.</param>
        static void SyncModule(ModuleSync moduleSync, string targetPath, CopyAction action)
        {
            var srcModulePath = action == CopyAction.TargetToSource ? targetPath : Sparta.BasePath;
            var dstModulePath = action == CopyAction.TargetToSource ? Sparta.BasePath : targetPath;

            foreach(var file in moduleSync.Files)
            {
                bool deleteFile = action == CopyAction.TargetToSource ? 
					file.FileStatus == ModuleSync.FileStatus.MissingFileInTarget :
					file.FileStatus == ModuleSync.FileStatus.MissingFileInSource;

                if(deleteFile)
                {
                    File.Delete(Path.Combine(dstModulePath, file.File));
                }
                else
                {
                    var dstPath = Path.Combine(dstModulePath, file.File);
                    var dstDir = Path.GetDirectoryName(dstPath);
                    if(!Directory.Exists(dstDir))
                    {
                        Directory.CreateDirectory(dstDir);
                    }

                    File.Copy(Path.Combine(srcModulePath, file.File),
                        dstPath,
                        true);
                }
            }
        }

        /// <summary>
        /// Compare the reference module between the Sparta and Target Project
        /// </summary>
        /// <param name="projectPath">Target Project path.</param>
        /// <param name="module">Reference Module.</param>
        /// <param name="status">Out parameter. Return the module status after comparison.</param>
        static IList<ModuleSync.FileSync> Compare(string projectPath, Module module, out ModuleSync.SyncStatus status)
        {
            var fileSync = new Dictionary<string, ModuleSync.FileSync>();

            CompareModuleFolder(fileSync, module, projectPath);
            CompareModuleDependencies(fileSync, module, projectPath);

            // Update status
            status = GetSyncStatus(fileSync, module, projectPath);
		
            return fileSync.Values.ToList();
        }

        /// <summary>
        /// Gets the sync status, depending on the modules folders and the current fileSync diferences.
        /// </summary>
        /// <returns>The sync status.</returns>
        /// <param name="fileSync">File sync.</param>
        /// <param name="module">Module.</param>
        /// <param name="projectPath">Project path.</param>
        static ModuleSync.SyncStatus GetSyncStatus(Dictionary<string, ModuleSync.FileSync> fileSync, Module module, string projectPath)
        {
            var spartaModulePath = Path.Combine(Sparta.BasePath, module.RelativePath);
            var targetModulePath = Path.Combine(projectPath, module.RelativePath);

            var spartaModuleDir = new DirectoryInfo(spartaModulePath);
            var targetModuleDir = new DirectoryInfo(targetModulePath);

            // Check folders status
            var status = ModuleSync.SyncStatus.UpToDate;
            if(spartaModuleDir.Exists && !targetModuleDir.Exists)
            {
                status = ModuleSync.SyncStatus.NotInstalled;
            }
            else if(!spartaModuleDir.Exists && targetModuleDir.Exists)
            {
                status = ModuleSync.SyncStatus.New;
            }

            // Check actual differences between module files
            foreach(var fs in fileSync.Values)
            {
                if(fs.SizeInSource != fs.SizeInTarget)
                {
                    if(status == ModuleSync.SyncStatus.UpToDate)
                    {
                        status = ModuleSync.SyncStatus.HasChanges;
                    }

                    SyncReport.Log(string.Format("{0} {1} : {2}", fs.SizeInSource != 0 ? "<" : " ", fs.SizeInTarget != 0 ? ">" : " ", fs.File));
                }
            }

            return status;
        }

        /// <summary>
        /// Compares the module folder.
        /// </summary>
        /// <param name="fileSync">File sync.</param>
        /// <param name="module">Module.</param>
        /// <param name="projectPath">Project path.</param>
        static void CompareModuleFolder(Dictionary<string, ModuleSync.FileSync> fileSync, Module module, string projectPath)
        {
            var spartaModulePath = Path.Combine(Sparta.BasePath, module.RelativePath);
            var targetModulePath = Path.Combine(projectPath, module.RelativePath);

            SyncReport.Log(string.Format("Comparing {0} with {1}", spartaModulePath, targetModulePath));

            UpdateFolderSync(fileSync, projectPath, spartaModulePath, targetModulePath);
        }

        /// <summary>
        /// Compares the module dependencies.
        /// </summary>
        /// <param name="fileSync">File sync.</param>
        /// <param name="module">Module.</param>
        /// <param name="projectPath">Project path.</param>
        static void CompareModuleDependencies(Dictionary<string, ModuleSync.FileSync> fileSync, Module module, string projectPath)
        {
            foreach(var dependencyPath in module.Dependencies)
            {
                var spartaDependencyPath = Path.Combine(Sparta.BasePath, dependencyPath);
                var targetDependencyPath = Path.Combine(projectPath, dependencyPath);

                bool isDirectory = IsDirectory(spartaDependencyPath) || IsDirectory(targetDependencyPath);

                if(isDirectory)
                {
                    UpdateFolderSync(fileSync, projectPath, spartaDependencyPath, targetDependencyPath);
                }
                else
                {
                    // File dependency. Include .meta files for each file.
                    var depFile1 = new FileInfo(spartaDependencyPath);
                    var depFile2 = new FileInfo(targetDependencyPath);

                    IEnumerable<FileInfo> depList1 = depFile1.Exists ? new FileInfo[] {
                        depFile1,
                        new FileInfo(spartaDependencyPath + ".meta")
                    } : EmptyFileList;
                    IEnumerable<FileInfo> depList2 = depFile2.Exists ? new FileInfo[] {
                        depFile2,
                        new FileInfo(targetDependencyPath + ".meta")
                    } : EmptyFileList;

                    // Here the BasePath is the project root since the dependencies are from there, instead of the module root
                    UpdateFileSync(fileSync, depList1, Sparta.BasePath, ProjectType.Source);
                    UpdateFileSync(fileSync, depList2, projectPath, ProjectType.Target);
                }
            }
        }

        /// <summary>
        /// Determines if is directory the specified path.
        /// </summary>
        /// <returns><c>true</c> if is directory the specified path; otherwise, <c>false</c>.</returns>
        /// <param name="path">Path.</param>
        static bool IsDirectory(string path)
        {
            bool isDirectory = false;
            try
            {
                var attrs = File.GetAttributes(path);
                isDirectory = (attrs & FileAttributes.Directory) == FileAttributes.Directory;
            }
            catch(IOException)
            {
            }

            return isDirectory;
        }

        /// <summary>
        /// Updates folder synchronization status.
        /// </summary>
        /// <param name="fileSync">File sync.</param>
        /// <param name="projectPath">Project path.</param>
        /// <param name="spartaFolderPath">Sparta folder path to sync.</param>
        /// <param name="targetFolderPath">Target folder path to sync.</param>
        static void UpdateFolderSync(IDictionary<string, ModuleSync.FileSync> fileSync, string projectPath, string spartaFolderPath, string targetFolderPath)
        {
            var spartaModuleDir = new DirectoryInfo(spartaFolderPath);
            var targetModuleDir = new DirectoryInfo(targetFolderPath);

            // Take a snapshot of the file system.
            var spartaFiles = spartaModuleDir.Exists ? spartaModuleDir.GetFiles("*.*", SearchOption.AllDirectories) : EmptyFileList;
            var targetfiles = targetModuleDir.Exists ? targetModuleDir.GetFiles("*.*", SearchOption.AllDirectories) : EmptyFileList;

            UpdateFileSync(fileSync, spartaFiles, Sparta.BasePath, ProjectType.Source);
            UpdateFileSync(fileSync, targetfiles, projectPath, ProjectType.Target);
        }

        /// <summary>
        /// Update file synchronization status
        /// </summary>
        /// <param name="fileSync">File sync.</param>
        /// <param name="files">Files.</param>
        /// <param name="basePath">Project base path.</param>
        /// <param name="projectType">Referenced project type.</param>
        static void UpdateFileSync(IDictionary<string, ModuleSync.FileSync> fileSync, IEnumerable<FileInfo> files, string basePath, ProjectType projectType)
        {
            foreach(var file in files)
            {
                // TODO Check required filters
                if(file.Extension.Equals(".DS_Store"))
                {
                    continue;
                }

                var relativePath = file.FullName.Substring(basePath.Length + 1);

                ModuleSync.FileSync sync;
                if(!fileSync.TryGetValue(relativePath, out sync))
                {
                    sync = new ModuleSync.FileSync();
                    sync.File = relativePath;
                    fileSync[relativePath] = sync;
                }

                // TODO Byte-Byte comparison.
                // Update size . Uses min size as 1 for zero-bytes files.
                if(projectType == ProjectType.Source)
                {
                    sync.SizeInSource = Math.Max(file.Length, 1);
                }
                else
                {
                    sync.SizeInTarget = Math.Max(file.Length, 1);
                }
            }
        }
    }
}