using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityEngine;
using System.Text;

namespace SocialPoint.XCodeEditor
{
    public partial class XCProject : System.IDisposable
    {
        private PBXDictionary _datastore;
        public PBXDictionary _objects;
        private PBXGroup _rootGroup;
        private string _rootObjectKey;

        public string projectRootPath { get; private set; }

        private FileInfo projectFileInfo;

        public string filePath { get; private set; }

        private bool modified = false;

        #region Data

        // Objects
        private PBXDictionary<PBXBuildFile> _buildFiles;
        private PBXDictionary<PBXGroup> _groups;
        private PBXDictionary<PBXVariantGroup> _variantGroups;
        private PBXDictionary<PBXFileReference> _fileReferences;
        private PBXDictionary<PBXNativeTarget> _nativeTargets;
        private PBXDictionary<PBXFrameworksBuildPhase> _frameworkBuildPhases;
        private PBXDictionary<PBXResourcesBuildPhase> _resourcesBuildPhases;
        private PBXDictionary<PBXShellScriptBuildPhase> _shellScriptBuildPhases;
        private PBXDictionary<PBXSourcesBuildPhase> _sourcesBuildPhases;
        private PBXDictionary<PBXCopyFilesBuildPhase> _copyBuildPhases;
        private PBXDictionary<XCBuildConfiguration> _buildConfigurations;
        private PBXDictionary<XCConfigurationList> _configurationLists;
        private PBXProject _project;

        #endregion
        #region Constructor

        public XCProject()
        {
        }

        public XCProject(string filePath) : this()
        {
            if(!System.IO.Directory.Exists(filePath))
            {
                XCDebug.LogWarning("Path does not exists.");
                return;
            }

            if(filePath.EndsWith(".xcodeproj"))
            {
                XCDebug.Log("Opening project " + filePath);
                this.projectRootPath = Path.GetDirectoryName(filePath);
                this.filePath = filePath;
            }
            else
            {
                XCDebug.Log("Looking for xcodeproj files in " + filePath);
                string[] projects = System.IO.Directory.GetDirectories(filePath, "*.xcodeproj");
                if(projects.Length == 0)
                {
                    XCDebug.LogWarning("Error: missing xcodeproj file");
                    return;
                }

                this.projectRootPath = filePath;
                this.filePath = projects[0];  
            }

            // Convert to absolute
            this.projectRootPath = GetFullPath(this.projectRootPath);
            this.filePath = GetFullPath(this.filePath);
            XCDebug.Log("Project file path is: " + this.filePath);

            projectFileInfo = new FileInfo(PathCombine(this.filePath, "project.pbxproj"));
            StreamReader sr = projectFileInfo.OpenText();
            string contents = sr.ReadToEnd();
            sr.Close();

            PBXParser parser = new PBXParser();
            _datastore = parser.Decode(contents);
            if(_datastore == null)
            {
                throw new System.Exception("Could not parse pbx at file path " + this.filePath);
            }

            if(!_datastore.ContainsKey("objects"))
            {
                XCDebug.Log("Error " + _datastore.Count);
                return;
            }

            _objects = (PBXDictionary)_datastore["objects"];
            modified = false;

            _rootObjectKey = (string)_datastore["rootObject"];
            if(!string.IsNullOrEmpty(_rootObjectKey))
            {
                _project = new PBXProject(_rootObjectKey, (PBXDictionary)_objects[_rootObjectKey]);
                _rootGroup = new PBXGroup(_rootObjectKey, (PBXDictionary)_objects[_project.mainGroupID]);
            }
            else
            {
                XCDebug.LogWarning("error: project has no root object");
                _project = null;
                _rootGroup = null;
            }
        }

        private string GetFullPath(string path)
        {
            return PathFix(Path.GetFullPath(path));
        }

        private string PathCombine(string path1, string path2)
        {
            return PathFix(Path.Combine(path1, path2));
        }

        private string PathFix(string path)
        {
            return path.Replace("" + Path.DirectorySeparatorChar, "/");
        }

        private string PathRelative(string absPath, string relTo)
        {
            string[] absDirs = absPath.Split('\\');
            string[] relDirs = relTo.Split('\\');
            // Get the shortest of the two paths 
            int len = absDirs.Length < relDirs.Length ? absDirs.Length : relDirs.Length;
            // Use to determine where in the loop we exited 
            int lastCommonRoot = -1;
            int index;
            // Find common root 
            for(index = 0; index < len; index++)
            {
                if(absDirs[index] == relDirs[index])
                {
                    lastCommonRoot = index;
                }
                else
                {
                    break;
                }
            }
            // If we didn't find a common prefix then throw 
            if(lastCommonRoot == -1)
            {
                return relTo;
            }
            // Build up the relative path 
            StringBuilder relativePath = new StringBuilder();
            // Add on the .. 
            for(index = lastCommonRoot + 1; index < absDirs.Length; index++)
            {
                if(absDirs[index].Length > 0)
                {
                    relativePath.Append("..\\");
                }
            }
            // Add on the folders 
            for(index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
            {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);
            return relativePath.ToString();
        }

        #endregion
        #region Properties

        public PBXProject project
        {
            get
            {
                return _project;
            }
        }

        public PBXGroup rootGroup
        {
            get
            {
                return _rootGroup;
            }
        }

        public PBXDictionary<PBXBuildFile> buildFiles
        {
            get
            {
                if(_buildFiles == null)
                {
                    _buildFiles = new PBXDictionary<PBXBuildFile>(_objects);
                }
                return _buildFiles;
            }
        }

        public PBXDictionary<PBXGroup> groups
        {
            get
            {
                if(_groups == null)
                {
                    _groups = new PBXDictionary<PBXGroup>(_objects);
                }
                return _groups;
            }
        }

        public PBXDictionary<PBXVariantGroup> variantGroups
        {
            get
            {
                if(_variantGroups == null)
                {
                    _variantGroups = new PBXDictionary<PBXVariantGroup>(_objects);
                }
                return _variantGroups;
            }
        }

        public PBXDictionary<PBXFileReference> fileReferences
        {
            get
            {
                if(_fileReferences == null)
                {
                    _fileReferences = new PBXDictionary<PBXFileReference>(_objects);
                }
                return _fileReferences;
            }
        }

        public PBXDictionary<PBXNativeTarget> nativeTargets
        {
            get
            {
                if(_nativeTargets == null)
                {
                    _nativeTargets = new PBXDictionary<PBXNativeTarget>(_objects);
                }
                return _nativeTargets;
            }
        }

        public PBXDictionary<XCBuildConfiguration> buildConfigurations
        {
            get
            {
                if(_buildConfigurations == null)
                {
                    _buildConfigurations = new PBXDictionary<XCBuildConfiguration>(_objects);
                }
                return _buildConfigurations;
            }
        }

        public PBXDictionary<XCConfigurationList> configurationLists
        {
            get
            {
                if(_configurationLists == null)
                {
                    _configurationLists = new PBXDictionary<XCConfigurationList>(_objects);
                }
                return _configurationLists;
            }
        }

        public PBXDictionary<PBXFrameworksBuildPhase> frameworkBuildPhases
        {
            get
            {
                if(_frameworkBuildPhases == null)
                {
                    _frameworkBuildPhases = new PBXDictionary<PBXFrameworksBuildPhase>(_objects);
                }
                return _frameworkBuildPhases;
            }
        }

        public PBXDictionary<PBXResourcesBuildPhase> resourcesBuildPhases
        {
            get
            {
                if(_resourcesBuildPhases == null)
                {
                    _resourcesBuildPhases = new PBXDictionary<PBXResourcesBuildPhase>(_objects);
                }
                return _resourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXShellScriptBuildPhase> shellScriptBuildPhases
        {
            get
            {
                if(_shellScriptBuildPhases == null)
                {
                    _shellScriptBuildPhases = new PBXDictionary<PBXShellScriptBuildPhase>(_objects);
                }
                return _shellScriptBuildPhases;
            }
        }

        public PBXDictionary<PBXSourcesBuildPhase> sourcesBuildPhases
        {
            get
            {
                if(_sourcesBuildPhases == null)
                {
                    _sourcesBuildPhases = new PBXDictionary<PBXSourcesBuildPhase>(_objects);
                }
                return _sourcesBuildPhases;
            }
        }

        public PBXDictionary<PBXCopyFilesBuildPhase> copyBuildPhases
        {
            get
            {
                if(_copyBuildPhases == null)
                {
                    _copyBuildPhases = new PBXDictionary<PBXCopyFilesBuildPhase>(_objects);
                }
                return _copyBuildPhases;
            }
        }



        #endregion
        #region PBXMOD

        public bool AddOtherCFlags(string flag)
        {
            return AddOtherCFlags(new PBXList(flag)); 
        }

        public bool AddOtherCFlags(PBXList flags)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddOtherCFlags(flags);
            }
            modified = true;
            return modified;    
        }

        public bool AddOtherLDFlags(string flag)
        {
            return AddOtherLDFlags(new PBXList(flag)); 
        }

        public bool AddOtherLDFlags(PBXList flags)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddOtherLDFlags(flags);
            }
            modified = true;
            return modified;    
        }

        public bool GccEnableCppExceptions(string value)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.GccEnableCppExceptions(value);
            }
            modified = true;
            return modified;    
        }

        public bool GccEnableObjCExceptions(string value)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.GccEnableObjCExceptions(value);
            }
            modified = true;
            return modified;
        }

        public bool SetBuildSettings(Hashtable settings)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.SetBuildSettings(settings);
            }
            modified = true;
            return modified;
        }

        public bool AddHeaderSearchPaths(string path)
        {
            return AddHeaderSearchPaths(new PBXList(path));
        }

        public bool AddHeaderSearchPaths(PBXList paths)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddHeaderSearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public bool AddLibrarySearchPaths(string path)
        {
            return AddLibrarySearchPaths(new PBXList(path));
        }

        public bool AddLibrarySearchPaths(PBXList paths)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddLibrarySearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public bool AddFrameworkSearchPaths(string path)
        {
            return AddFrameworkSearchPaths(new PBXList(path));
        }

        public bool AddFrameworkSearchPaths(PBXList paths)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                buildConfig.Value.AddFrameworkSearchPaths(paths);
            }
            modified = true;
            return modified;
        }

        public void AddTargetAttributes(Hashtable targetAttrs)
        {
            PBXDictionary pbxAttrs = new PBXDictionary();
            foreach(KeyValuePair<string,PBXNativeTarget> pair in nativeTargets)
            {
                if(!pbxAttrs.ContainsKey(pair.Key))
                {
                    pbxAttrs.Add(pair.Key, new PBXDictionary(targetAttrs));
                }
            }
            project.AddTargetAttributes(pbxAttrs);
        }

        public PBXNativeTarget GetNativeTarget(string name)
        {
            foreach(KeyValuePair<string,PBXNativeTarget> pair in nativeTargets)
            {
                if(pair.Value.data["name"].ToString() == name)
                {
                    return pair.Value;
                }
            }
            return null;
        }

        public void SetSystemCapabilities(Hashtable capabilities)
        {
            Hashtable targetAttrs = new Hashtable();
            Hashtable capsAttrs = new Hashtable();
            targetAttrs.Add("SystemCapabilities", capsAttrs);
            foreach(DictionaryEntry cap in capabilities)
            {
                Hashtable capAttrs = new Hashtable();
                string name = (string)cap.Key;
                bool value = (bool)cap.Value;
                capsAttrs.Add(name, capAttrs);
                XCDebug.Log((value ? "Enabling" : "Disabling") + " system capability '" + name + "'");
                capAttrs.Add("enabled", value ? 1 : 0);
            }
            AddTargetAttributes(targetAttrs);
        }

        public void SetSystemCapability(string name, bool enabled)
        {
            Hashtable caps = new Hashtable();
            caps.Add(name, enabled);
            SetSystemCapabilities(caps);
        }

        public bool AddKeychainAccessGroups(ArrayList groups, PBXGroup parent = null)
        {
            foreach(KeyValuePair<string, XCBuildConfiguration> buildConfig in buildConfigurations)
            {
                var plist = buildConfig.Value.GetEntitlements(this);
                if(plist != null && plist.AddKeychainAccessGroups(groups))
                {
                    if(plist.SaveFile())
                    {
                        XCDebug.Log("Adding entitlements file '" + plist.filePath + "'");
                        AddFile(plist.filePath, parent);
                    }
                }
            }
            if(groups.Count > 0)
            {
                SetSystemCapability("com.apple.Keychain", true);
            }
            modified = true;
            return modified;
        }

        public bool SetProvisioningProfile(string path)
        {
            XCMobileProvision prov = new XCMobileProvision(path);
            XCDebug.Log("Found provisioning profile '" + prov.UUID + "'");
            return SetProvisioningProfile(prov);
        }

        public bool SetProvisioningProfile(XCMobileProvision prov)
        {
            bool modified = false;
            foreach(var config in buildConfigurations.Values)
            {
                if(config.SetProvisioningProfile(prov))
                {
                    XCDebug.Log("Provisioning profile set for build configuration '" + config.name + "'");

                    if(prov.AppIdPrefix != null)
                    {
                        XCPlist info = config.GetInfoPlist(this);
                        if(info != null)
                        {
                            info["BundleSeedId"] = prov.AppIdPrefix;
                            info.SaveFile();
                            XCDebug.Log("Info plist BundleSeedId=" + prov.AppIdPrefix + " set for build configuration '" + config.name + "'");
                        }
                    }

                    modified = true;
                }
            }

            if(modified)
            {
                string path = Environment.GetEnvironmentVariable("HOME");
                path = PathCombine(path, "Library/MobileDevice/Provisioning Profiles/" + prov.UUID + ".mobileprovision");
                File.Copy(prov.filePath, path, true);
                XCDebug.Log("Copied provisioning profile to '" + path + "'");
            }

            return modified;
        }

        public bool CombineInfoPlist(Hashtable table)
        {
            bool success = false;
            foreach(var config in buildConfigurations.Values)
            {
                XCPlist info = config.GetInfoPlist(this);
                if(info == null)
                {
                    XCDebug.LogWarning("Could not find info plist for build config " + config.name);
                }
                else
                {
                    XCDebug.Log("Applying changes to info plist " + config.name);
                    info.Combine(table);
                    info.SaveFile();
                    success = true;
                }
            }
            return success;
        }

        public object GetObject(string guid)
        {
            return _objects[guid];
        }

        private void CreatePathDirectories(string path)
        {
            var dir = Path.GetDirectoryName(GetFullPath(path));
            Directory.CreateDirectory(dir);
        }

        public bool CopyFile(string fromPath, string toPath)
        {
            fromPath = GetFullPath(fromPath);
            toPath = GetFullPath(toPath);
            if(Path.GetFileName(toPath).Length == 0)
            {
                toPath = PathCombine(toPath, Path.GetFileName(fromPath));
            }
            string fromDir = null;
            string fromFile = null;
            string toFile = null;
            string toDir = null;
            var i = fromPath.IndexOf('*');
            if(i == -1)
            {
                if(Directory.Exists(fromPath))
                {
                    fromDir = fromPath;
                    fromFile = "*";
                    if(Directory.Exists(toPath))
                    {
                        toPath = Path.Combine(toPath, Path.GetFileName(fromPath));
                    }
                }
                else
                {
                    fromDir = Path.GetDirectoryName(fromPath);
                    fromFile = Path.GetFileName(fromPath);
                }
                toDir = Path.GetDirectoryName(toPath);
                toFile = Path.GetFileName(toPath);
            }
            else
            {
                i = fromPath.LastIndexOf(Path.DirectorySeparatorChar, i);
                fromDir = fromPath.Substring(0, i);
                i++;
                fromFile = fromPath.Substring(i, fromPath.Length - i);
                toDir = toPath;
            }

            XCDebug.Log("copy files origin " + fromDir + " -> " + fromFile);
            foreach(string fromFilepath in Directory.GetFiles(fromDir, fromFile, SearchOption.AllDirectories))
            {
                var toFilePath = fromFilepath;
                toFilePath = toFilePath.Replace(fromDir, toDir);
                if(toFile != null)
                {
                    i = toFilePath.LastIndexOf(fromFile);
                    if(i >= 0)
                    {
                        toFilePath = toFilePath.Substring(0, i)+toFile;
                    }
                }
                var toFileDir = Path.GetDirectoryName(toFilePath);
                XCDebug.Log("copy file from " + fromFilepath + " to " + toFilePath);
                if(!Directory.Exists(toFileDir))
                {
                    Directory.CreateDirectory(toFileDir);
                }
                File.Copy(fromFilepath, toFilePath, true);
            }
            return true;
        }

        public PBXDictionary AddFile(string filePath, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false, string[] compilerFlags = null)
        {
            return AddFile(filePath, null, parent, tree, createBuildFiles, weak, compilerFlags);
        }

        private string RootFilePath
        {
            get
            {
                try
                {
                    return Application.dataPath.Replace("Assets", "").TrimEnd(Path.DirectorySeparatorChar);
                }
                catch(System.MissingMethodException)
                {
                    return filePath;
                }
            }   
        }

        public PBXDictionary AddFile(string filePath, string name, PBXGroup parent = null, string tree = "SOURCE_ROOT", bool createBuildFiles = true, bool weak = false, string[] compilerFlags = null)
        {
            PBXDictionary results = new PBXDictionary();
            string absPath = string.Empty;

            if(Path.IsPathRooted(filePath))
            {
                absPath = filePath;
            }
            else
                if(tree.CompareTo("SDKROOT") != 0)
                {
                    absPath = PathCombine(RootFilePath, filePath);
                }

            if(!(File.Exists(absPath) || Directory.Exists(absPath)) && tree.CompareTo("SDKROOT") != 0)
            {
                XCDebug.LogError("Missing file: " + absPath + " > " + filePath);
                return results;
            }
            else
                if(tree.CompareTo("SOURCE_ROOT") == 0 || tree.CompareTo("GROUP") == 0)
                {
                    filePath = PathRelative(projectRootPath, absPath);
                }

            if(parent == null)
            {
                parent = _rootGroup;
            }

            PBXFileReference fileReference = GetFile(System.IO.Path.GetFileName(filePath)); 
            if(fileReference != null)
            {
                fileReferences.Remove(fileReference.guid);
            }

            fileReference = new PBXFileReference(filePath, name, (TreeEnum)System.Enum.Parse(typeof(TreeEnum), tree));
            parent.AddChild(fileReference);
            fileReferences.Add(fileReference);
            results.Add(fileReference.guid, fileReference);

            //Create a build file for reference
            if(!string.IsNullOrEmpty(fileReference.buildPhase) && createBuildFiles)
            {
                PBXBuildFile buildFile;
                switch(fileReference.buildPhase)
                {
                    case "PBXFrameworksBuildPhase":
                        foreach(KeyValuePair<string, PBXFrameworksBuildPhase> currentObject in frameworkBuildPhases)
                        {
                            buildFile = new PBXBuildFile(fileReference, weak, compilerFlags);
                            buildFiles.Add(buildFile);
                            currentObject.Value.AddBuildFile(buildFile);
                        }

                        if(!string.IsNullOrEmpty(absPath) && File.Exists(absPath) && tree.CompareTo("SOURCE_ROOT") == 0)
                        {
                            //XCDebug.LogError(absPath);
                            string libraryPath = PathCombine("$(SRCROOT)", Path.GetDirectoryName(filePath));
                            this.AddLibrarySearchPaths(new PBXList(libraryPath));
                        }
                        else
                            if(!string.IsNullOrEmpty(absPath) && Directory.Exists(absPath) && absPath.EndsWith(".framework") && tree.CompareTo("GROUP") == 0)
                            { // Annt: Add framework search path for FacebookSDK
                                string frameworkPath = PathCombine("$(SRCROOT)", Path.GetDirectoryName(filePath));
                                this.AddFrameworkSearchPaths(new PBXList(frameworkPath));
                            }
                        break;
                    case "PBXResourcesBuildPhase":
                        foreach(KeyValuePair<string, PBXResourcesBuildPhase> currentObject in resourcesBuildPhases)
                        {
                            buildFile = new PBXBuildFile(fileReference, weak, compilerFlags);
                            buildFiles.Add(buildFile);
                            currentObject.Value.AddBuildFile(buildFile);
                        }
                        break;
                    case "PBXShellScriptBuildPhase":
                        foreach(KeyValuePair<string, PBXShellScriptBuildPhase> currentObject in shellScriptBuildPhases)
                        {
                            buildFile = new PBXBuildFile(fileReference, weak, compilerFlags);
                            buildFiles.Add(buildFile);
                            currentObject.Value.AddBuildFile(buildFile);
                        }
                        break;
                    case "PBXSourcesBuildPhase":
                        foreach(KeyValuePair<string, PBXSourcesBuildPhase> currentObject in sourcesBuildPhases)
                        {
                            buildFile = new PBXBuildFile(fileReference, weak, compilerFlags);
                            buildFiles.Add(buildFile);
                            currentObject.Value.AddBuildFile(buildFile);
                        }
                        break;
                    case "PBXCopyFilesBuildPhase":
                        foreach(KeyValuePair<string, PBXCopyFilesBuildPhase> currentObject in copyBuildPhases)
                        {
                            buildFile = new PBXBuildFile(fileReference, weak, compilerFlags);
                            buildFiles.Add(buildFile);
                            currentObject.Value.AddBuildFile(buildFile);
                        }
                        break;
                    default:
                        XCDebug.LogWarning("Phase '" + fileReference.buildPhase + "' not supported.");
                        return null;
                }
            }

            return results;
        }

        public bool AddFolder(string folderPath, PBXGroup parent = null, string[] exclude = null, bool recursive = true, bool createBuildFile = true)
        {
            if(!Directory.Exists(folderPath))
            {
                return false;
            }
            DirectoryInfo sourceDirectoryInfo = new DirectoryInfo(folderPath);

            if(exclude == null)
            {
                exclude = new string[] {};
            }
            string regexExclude = string.Format(@"{0}", string.Join("|", exclude));

            if(parent == null)
            {
                parent = rootGroup;
            }

            // Create group
            PBXGroup newGroup = GetGroup(sourceDirectoryInfo.Name, null, parent);

            foreach(string directory in Directory.GetDirectories( folderPath ))
            {
                if(Regex.IsMatch(directory, regexExclude))
                {
                    continue;
                }

                XCDebug.Log("DIR: " + directory);
                if(directory.EndsWith(".bundle"))
                {
                    // Treath it like a file and copy even if not recursive
                    XCDebug.LogWarning("This is a special folder: " + directory);
                    AddFile(directory, newGroup, "SOURCE_ROOT", createBuildFile);
                    XCDebug.Log("fatto");
                    continue;
                }

                if(recursive)
                {
                    XCDebug.Log("recursive");
                    AddFolder(directory, newGroup, exclude, recursive, createBuildFile);
                }
            }
            // Adding files.
            foreach(string file in Directory.GetFiles( folderPath ))
            {
                if(Regex.IsMatch(file, regexExclude))
                {
                    continue;
                }
                AddFile(file, newGroup, "SOURCE_ROOT", createBuildFile);
            }

            modified = true;
            return modified;
        }

        public bool AddVariantGroup(string name, IDictionary<string,string> files, PBXGroup parent = null, bool createBuildFile = true)
        {   
            if(parent == null)
            {
                parent = rootGroup;
            }

            PBXVariantGroup newGroup = GetVariantGroup(name, null /*relative path*/, parent);

            // Adding files.
            foreach(var file in files)
            {
                AddFile(file.Value, file.Key, newGroup, "GROUP", createBuildFile);
            }

            return true;
        }

        #endregion
        #region Getters
        public PBXFileReference GetFile(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach(KeyValuePair<string, PBXFileReference> current in fileReferences)
            {
                if(!string.IsNullOrEmpty(current.Value.name) && current.Value.name.CompareTo(name) == 0)
                {
                    return current.Value;
                }
            }

            return null;
        }

        public PBXVariantGroup GetVariantGroup(string name, string path = null, PBXGroup parent = null)
        {
            XCDebug.Log("GetVariantGroup: " + name + ", " + path + ", " + parent);
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            if(parent == null)
            {
                parent = rootGroup;
            }

            foreach(KeyValuePair<string, PBXVariantGroup> current in variantGroups)
            {

                if(string.IsNullOrEmpty(current.Value.name))
                { 
                    if(!string.IsNullOrEmpty(current.Value.path) && current.Value.path.CompareTo(name) == 0 && parent.HasChild(current.Key))
                    {
                        return current.Value;
                    }
                }
                else
                    if(current.Value.name.CompareTo(name) == 0 && parent.HasChild(current.Key))
                    {
                        return current.Value;
                    }
            }

            PBXVariantGroup result = new PBXVariantGroup(name, path);
            variantGroups.Add(result);
            parent.AddChild(result);

            modified = true;
            return result;
        }

        public PBXGroup GetGroup(string name, string path = null, PBXGroup parent = null)
        {
            if(string.IsNullOrEmpty(name))
            {
                return null;
            }

            XCDebug.Log("GetGroup: " + name + ", " + path + ", " + parent);

            if(parent == null)
            {
                parent = rootGroup;
            }

            foreach(KeyValuePair<string, PBXGroup> current in groups)
            {

                if(string.IsNullOrEmpty(current.Value.name))
                { 
                    if(!string.IsNullOrEmpty(current.Value.path) && current.Value.path.CompareTo(name) == 0 && parent.HasChild(current.Key))
                    {
                        return current.Value;
                    }
                }
                else
                    if(current.Value.name.CompareTo(name) == 0 && parent.HasChild(current.Key))
                    {
                        return current.Value;
                    }
            }

            PBXGroup result = new PBXGroup(name, path);
            groups.Add(result);
            parent.AddChild(result);

            modified = true;
            return result;
        }

        #endregion

        #region Mods

        public void ApplyMod(string pbxmod)
        {
            XCMod mod = new XCMod(pbxmod);
            ApplyMod(mod);
        }

        public void ApplyMod(string rootPath, string pbxmod)
        {
            XCMod mod = new XCMod(rootPath, pbxmod);
            ApplyMod(mod);
        }

        public void ApplyMod(string rootPath, string name, string contents)
        {
            XCMod mod = new XCMod(rootPath, name, contents);
            ApplyMod(mod);
        }

        private string ReplaceProjectVariables(string str)
        {
            IDictionary<string, string> vars = new Dictionary<string, string>();
            vars.Add("XCODE_PROJECT_PATH", filePath);
            vars.Add("XCODE_ROOT_PATH", projectRootPath);
            vars.Add("ROOT_PATH", RootFilePath);

            foreach(var entry in vars)
            {
                str = str.Replace("{" + entry.Key + "}", entry.Value);
            }
            return str;
        }

        private string CombineModPath(XCMod mod, string path)
        {
            path = ReplaceProjectVariables(path);
            return PathCombine(mod.path, path);
        }

        public void ApplyMod(XCMod mod)
        {   
            if(mod == null)
            {
                XCDebug.Log("Could not read mod file.");
                return;
            }

            PBXGroup modGroup = this.GetGroup(mod.group);

            if(mod.libs != null)
            {
                XCDebug.Log("Adding libraries...");
                foreach(XCModFile libRef in mod.libs)
                {
                    string completeLibPath;
                    if(libRef.sourceTree.Equals("SDKROOT"))
                    {
                        completeLibPath = PathCombine("usr/lib", libRef.filePath);
                    }
                    else
                    {
                        completeLibPath = PathCombine(mod.path, libRef.filePath);
                    }

                    this.AddFile(completeLibPath, modGroup, libRef.sourceTree, true, libRef.isWeak);
                }
            }

            PBXGroup frameworkGroup = this.GetGroup("Frameworks");

            if(mod.frameworks != null)
            {
                XCDebug.Log("Adding frameworks...");
                foreach(string framework in mod.frameworks)
                {
                    string[] filename = framework.Split(':');
                    bool isWeak = (filename.Length > 1) ? true : false;
                    string completePath = PathCombine("System/Library/Frameworks", filename[0]);
                    this.AddFile(completePath, frameworkGroup, "SDKROOT", true, isWeak);
                }
            }

            if(mod.copyFiles != null)
            {
                XCDebug.Log("Copying files...");
                foreach(DictionaryEntry entry in mod.copyFiles)
                {
                    string fromPath = CombineModPath(mod, (string)entry.Key);
                    ArrayList toPaths;
                    if(entry.Value is string)
                    {
                        toPaths = new ArrayList();
                        toPaths.Add(entry.Value);
                    }
                    else
                    {
                        toPaths = (ArrayList)entry.Value;
                    }
                    foreach(string toPath in toPaths)
                    {
                        CopyFile(fromPath, CombineModPath(mod, toPath));
                    }
                }
            }

            if(mod.files != null)
            {
                XCDebug.Log("Adding files...");
                foreach(string filePath in mod.files)
                {
                    string absoluteFilePath = CombineModPath(mod, filePath);


                    if(filePath.EndsWith(".framework"))
                    {
                        this.AddFile(absoluteFilePath, frameworkGroup, "GROUP", true, false);
                    }
                    else
                    {
                        string[] compilerFlags = null;
                        string[] filename = filePath.Split(':');
                        if( filename.Length > 1 )
                        {
                            compilerFlags = filename[1].Split(',');
                        }
                        this.AddFile(filename[0], modGroup, "SOURCE_ROOT", true, false, compilerFlags);
                    }
                }
            }

            if(mod.variantGroups != null)
            {
                XCDebug.Log("Adding variant groups...");
                foreach(DictionaryEntry group in mod.variantGroups)
                {
                    IDictionary<string,string> files = new Dictionary<string,string>();
                    foreach(DictionaryEntry file in (Hashtable)group.Value)
                    {
                        files[(string)file.Key] = CombineModPath(mod, (string)file.Value);
                    }
                    this.AddVariantGroup((string)group.Key, files, modGroup);
                }
            }

            if(mod.folders != null)
            {
                XCDebug.Log("Adding folders...");
                foreach(string folderPath in mod.folders)
                {
                    string absoluteFolderPath = CombineModPath(mod, folderPath);
                    string[] excludes = null;
                    if(mod.excludes != null)
                    {
                        excludes = (string[])mod.excludes.ToArray(typeof(string));
                    }
                    this.AddFolder(absoluteFolderPath, modGroup, excludes);
                }
            }

            if(mod.headerpaths != null)
            {
                XCDebug.Log("Adding headerpaths...");
                foreach(string headerpath in mod.headerpaths)
                {
                    string absoluteHeaderPath = CombineModPath(mod, headerpath);
                    this.AddHeaderSearchPaths(absoluteHeaderPath);
                }
            }

            if(mod.librarysearchpaths != null)
            {
                XCDebug.Log("Adding librarysearchpaths...");
                foreach(string librarypath in mod.librarysearchpaths)
                {
                    string absolutePath = CombineModPath(mod, librarypath);
                    this.AddLibrarySearchPaths(absolutePath);
                }
            }

            if(mod.targetAttributes != null)
            {
                XCDebug.Log("Adding target attributes...");
                this.AddTargetAttributes(mod.targetAttributes);
            }

            if(mod.provisioningProfile != null)
            {
                string path = CombineModPath(mod, mod.provisioningProfile);
                XCDebug.Log("Setting provisioning profile '" + path + "'...");
                this.SetProvisioningProfile(path);
            }

            if(mod.keychainAccessGroups != null)
            {
                XCDebug.Log("Adding keychain access groups...");
                this.AddKeychainAccessGroups(mod.keychainAccessGroups, modGroup);
            }

            if(mod.systemCapabilities != null)
            {
                XCDebug.Log("Setting system capabilities...");
                this.SetSystemCapabilities(mod.systemCapabilities);
            }

            if(mod.infoPlist != null)
            {
                XCDebug.Log("Modifying info plist files...");
                CombineInfoPlist(mod.infoPlist);
            }

            Hashtable buildSettings = mod.buildSettings;
            if(buildSettings != null)
            {
                XCDebug.Log("Configure build settings...");
                if(buildSettings.ContainsKey("OTHER_LDFLAGS"))
                {
                    XCDebug.Log("    Adding other linker flags...");
                    ArrayList otherLinkerFlags = (ArrayList)buildSettings["OTHER_LDFLAGS"];
                    foreach(string linker in otherLinkerFlags)
                    {
                        string _linker = linker;
                        if(!_linker.StartsWith("-"))
                        {
                            _linker = "-" + _linker;
                        }
                        this.AddOtherLDFlags(_linker);
                    }
                    buildSettings.Remove("OTHER_LDFLAGS");
                }

                if(buildSettings.ContainsKey("GCC_ENABLE_CPP_EXCEPTIONS"))
                {
                    XCDebug.Log("    GCC_ENABLE_CPP_EXCEPTIONS = " + buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"]);
                    this.GccEnableCppExceptions((string)buildSettings["GCC_ENABLE_CPP_EXCEPTIONS"]);
                    buildSettings.Remove("GCC_ENABLE_CPP_EXCEPTIONS");
                }

                if(buildSettings.ContainsKey("GCC_ENABLE_OBJC_EXCEPTIONS"))
                {
                    XCDebug.Log("    GCC_ENABLE_OBJC_EXCEPTIONS = " + buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"]);
                    this.GccEnableObjCExceptions((string)buildSettings["GCC_ENABLE_OBJC_EXCEPTIONS"]);
                    buildSettings.Remove("GCC_ENABLE_OBJC_EXCEPTIONS");
                }

                this.SetBuildSettings(buildSettings);
            }

            if(mod.shellScripts != null)
            {
                XCDebug.Log("Adding shell scripts...");
                AddShellScripts(mod.shellScripts);
            }

            this.Consolidate();
        }

        void AddShellScripts(IList<XCModShellScript> scripts)
        {
            foreach(var script in scripts)
            {
                var target = GetNativeTarget(script.Target);
                if(target != null)
                {
                    var phaseData = new PBXDictionary();
                    phaseData.Add("isa", "PBXShellScriptBuildPhase");
                    phaseData.Add("shellPath", script.Shell ?? "/bin/sh");
                    phaseData.Add("shellScript", script.Script.Replace("\"", "\\\""));
                    phaseData.Add("files", new ArrayList());
                    phaseData.Add("inputPaths", new ArrayList());
                    phaseData.Add("outputPaths", new ArrayList());
                    var phase = new PBXShellScriptBuildPhase(PBXObject.GenerateGuid(), phaseData);
                    this.shellScriptBuildPhases.Add(phase);
                    var phases = target.data["buildPhases"] as PBXList;
                    var pos = script.Position;
                    if(pos < 0)
                    {
                        pos += phases.Count;
                    }
                    phases.Insert(pos, phase.guid);
                }
            }
        }

        #endregion
        #region Savings

        public void Consolidate()
        {
            PBXDictionary consolidated = new PBXDictionary();
            consolidated.Append<PBXBuildFile>(this.buildFiles);
            consolidated.Append<PBXGroup>(this.groups);
            consolidated.Append<PBXVariantGroup>(this.variantGroups);
            consolidated.Append<PBXFileReference>(this.fileReferences);
            consolidated.Append<PBXNativeTarget>(this.nativeTargets);
            consolidated.Append<PBXFrameworksBuildPhase>(this.frameworkBuildPhases);
            consolidated.Append<PBXResourcesBuildPhase>(this.resourcesBuildPhases);
            consolidated.Append<PBXShellScriptBuildPhase>(this.shellScriptBuildPhases);
            consolidated.Append<PBXSourcesBuildPhase>(this.sourcesBuildPhases);
            consolidated.Append<PBXCopyFilesBuildPhase>(this.copyBuildPhases);
            consolidated.Append<XCBuildConfiguration>(this.buildConfigurations);
            consolidated.Append<XCConfigurationList>(this.configurationLists);
            consolidated.Add(project.guid, project.data);
            _objects = consolidated;
            consolidated = null;
        }

        public void Backup()
        {
            var timestamp = System.DateTime.Now.ToString("yyMMddHHmmss");
            string backupPath = PathCombine(this.filePath, "project.backup." + timestamp + ".pbxproj");
            XCDebug.Log("Writing backup file " + backupPath + "...");

            // Delete previous backup file
            if(File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }

            // Backup original pbxproj file first
            File.Copy(PathCombine(this.filePath, "project.pbxproj"), backupPath);
        }

        /// <summary>
        /// Saves a project after editing.
        /// </summary>
        public void Save()
        {
            PBXDictionary result = new PBXDictionary();
            result.Add("archiveVersion", 1);
            result.Add("classes", new PBXDictionary());
            result.Add("objectVersion", 45);

            Consolidate();
            result.Add("objects", _objects);

            result.Add("rootObject", _rootObjectKey);

            Backup();

            // Parse result object directly into file
            PBXParser parser = new PBXParser();
            StreamWriter saveFile = File.CreateText(PathCombine(this.filePath, "project.pbxproj"));
            saveFile.Write(parser.Encode(result, false));
            saveFile.Close();

        }

        /**
        * Raw project data.
        */
        public Dictionary<string, object> objects
        {
            get
            {
                return null;
            }
        }


        #endregion

        public void Dispose()
        {

        }
    }
}
