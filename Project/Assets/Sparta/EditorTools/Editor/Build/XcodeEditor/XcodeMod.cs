using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SpartaTools.Editor.Build.XcodeEditor
{
    /// <summary>
    /// Xcode mod file class
    /// 
    /// Supported options:
    ///  - headerpaths : List of search paths
    ///  - librarysearchpaths : List of search paths
    ///  - copyFiles : Dictionary defining required copies. Each source file can have either a single or list of destiny files.
    ///  - files : List of file paths to add to the build.
    ///     * flags : Could have a list of arbitrary flags, separated by comma, as arguments.
    ///  - folders : List of folder paths to reference from the project
    ///  - libs : List of library file paths to add to the build.
    ///  - frameworks : List of system frameworks
    ///     * 'weak' : Mark the framework as optional.
    ///  - buildSettings : Dictionary of Settings name and value pairs.
    ///  - variantGroups :
    ///  - infoPlist : 
    ///  - shellScripts : 
    ///  - systemCapabilities : Dictionary of Capabilitiy names and boolean values pairs.
    ///  - provisioningProfile :
    ///  - keychainAccessGroups : 
    /// 
    /// Arguments can be defined as a part of the entry, using the following format:
    ///  "entry_content:arg1,arg2"
    /// </summary>
    public class XcodeMod
    {
        readonly Hashtable _datastore;
        readonly string _filePath;
        readonly string _basePath;

        public XcodeMod(string path)
        {
            _filePath = path;
            _basePath = Path.GetDirectoryName(path);

            var file = new FileInfo(path);
            if(!file.Exists)
            {
                throw new FileNotFoundException("Xcode mod file not found at " + path);
            }

            using(var filestream = file.OpenText())
            {
                var content = filestream.ReadToEnd();
                _datastore = (Hashtable)XMiniJSON.jsonDecode(content);
            }

            if(_datastore == null)
            {
                throw new InvalidDataException("Could not load json");
            }
        }

        public void Apply(XCodeProjectEditor editor)
        {
            ApplyHeaderpaths(editor);
            ApplyLibrarypaths(editor);
            ApplyCopyFiles(editor);
            ApplyFiles(editor);
            ApplyFolders(editor);
            ApplyLibs(editor);
            ApplyFrameworks(editor);
            ApplyBuildSettings(editor);
            ApplyVariantGroups(editor);
            ApplyInfoPlist(editor);
            ApplyShellScripts(editor);
            ApplySystemCapabilities(editor);
            ApplyProvisioningProfile(editor);
            ApplyKeychainAccessGroups(editor);
        }

        /// <summary>
        /// Splits content and optionals attributes of one entry, with format:
        /// "entry_content:arg1,arg2"
        /// </summary>
        string SplitAttributes(string entry, out string[] attributes)
        {
            string[] parts = entry.Split(':');
            if(parts.Length > 1)
            {
                attributes = parts[1].Split(',');
            }
            else
            {
                attributes = new string[0];
            }
            return parts[0];
        }

        /// <summary>
        /// Resolve path for the mod element
        /// If path contains XcodeMod or Xcode variables, keep it as is. 
        /// Otherwise, use the full path.
        /// </summary>
        string GetModPath(string path)
        {
            if(path.Contains("{") || path.Contains("$"))
            {
                return path;
            }
            return Path.GetFullPath(Path.Combine(_basePath, path));
        }

        void ApplyHeaderpaths(XCodeProjectEditor editor)
        {
            var headerpaths = (ArrayList)_datastore["headerpaths"];
            if(headerpaths != null)
            {
                foreach(string headerpath in headerpaths)
                {
                    editor.AddHeaderSearchPath(headerpath);
                }
            }
        }

        void ApplyLibrarypaths(XCodeProjectEditor editor)
        {
            var librarysearchpaths = (ArrayList)_datastore["librarysearchpaths"];
            if(librarysearchpaths != null)
            {
                foreach(string libraryPath in librarysearchpaths)
                {
                    editor.AddLibrarySearchPath(libraryPath);
                }
            }
        }

        void ApplyCopyFiles(XCodeProjectEditor editor)
        {
            var copyFiles = (Hashtable)_datastore["copyFiles"];
            if(copyFiles != null)
            {
                foreach(DictionaryEntry entry in copyFiles)
                {
                    string fromPath = (string)entry.Key;

                    if(entry.Value is string)
                    {
                        editor.CopyFile(_basePath, fromPath, (string)entry.Value);
                    }
                    else
                    {
                        var toPaths = (ArrayList)entry.Value;
                        foreach(string dst in toPaths)
                        {
                            editor.CopyFile(_basePath, fromPath, dst);
                        }
                    }
                }
            }
        }

        void ApplyFiles(XCodeProjectEditor editor)
        {
            var files = (ArrayList)_datastore["files"];
            if(files != null)
            {
                foreach(string file in files)
                {
                    string[] attrs;
                    var filePath = SplitAttributes(file, out attrs);
                    string fullPath = GetModPath(filePath);
                    editor.AddFile(fullPath, attrs);
                }
            }
        }

        void ApplyFolders(XCodeProjectEditor editor)
        {
            var folders = (ArrayList)_datastore["folders"];
            if(folders != null)
            {
                foreach(string folder in folders)
                {
                    string fullPath = GetModPath(folder);
                    editor.AddFolder(fullPath);
                }
            }
        }

        void ApplyLibs(XCodeProjectEditor editor)
        {
            var libs = (ArrayList)_datastore["libs"];
            if(libs != null)
            {
                foreach(string lib in libs)
                {
                    string fullPath = GetModPath(lib);
                    editor.AddLibrary(fullPath);
                }
            }
        }

        void ApplyFrameworks(XCodeProjectEditor editor)
        {
            var frameworks = (ArrayList)_datastore["frameworks"];
            if(frameworks != null)
            {
                foreach(string framework in frameworks)
                {
                    string[] attrs;
                    var filename = SplitAttributes(framework, out attrs);
                    bool isWeak = (attrs.Length > 0 && attrs[0].Equals("weak"));
                    editor.AddFramework(filename, isWeak);
                }
            }
        }

        void ApplyBuildSettings(XCodeProjectEditor editor)
        {
            var settings = (Hashtable)_datastore["buildSettings"];
            if(settings != null)
            {
                foreach(DictionaryEntry entry in settings)
                {
                    editor.SetBuildSetting((string)entry.Key, (string)entry.Value);
                }
            }
        }

        void ApplyVariantGroups(XCodeProjectEditor editor)
        {
            var variantGroups = (Hashtable)_datastore["variantGroups"];
            if(variantGroups != null)
            {
                foreach(DictionaryEntry group in variantGroups)
                {
                    foreach(DictionaryEntry file in (Hashtable)group.Value)
                    {
                        editor.AddVariantGroup((string)group.Key, (string)file.Key, (string)file.Value);
                    }
                }
            }
        }

        void ApplyInfoPlist(XCodeProjectEditor editor)
        {
            var infoPlist = (Hashtable)_datastore["infoPlist"];
            if(infoPlist != null)
            {
                foreach(DictionaryEntry field in infoPlist)
                {
                    editor.SetPlistField((string)field.Key, new Dictionary<string, object>()); // FIXME use field.Value);
                }
            }
        }

        void ApplyShellScripts(XCodeProjectEditor editor)
        {
            var shellScripts = (ArrayList)_datastore["shellScripts"];
            if(shellScripts != null)
            {
                foreach(Hashtable scriptEntry in shellScripts)
                {
                    editor.AddShellScript((string)scriptEntry["script"]);
                }
            }
        }

        void ApplySystemCapabilities(XCodeProjectEditor editor)
        {
            var systemCapabilities = (Hashtable)_datastore["systemCapabilities"];
            if(systemCapabilities != null)
            {
                foreach(DictionaryEntry capability in systemCapabilities)
                {
                    string name = (string)capability.Key;
                    bool enabled = (bool)capability.Value;
                    editor.SetSystemCapability(name, enabled);
                }
            }
        }

        void ApplyProvisioningProfile(XCodeProjectEditor editor)
        {
            var provisioningProfile = (string)_datastore["provisioningProfile"];
            if(provisioningProfile != null)
            {
                editor.SetProvisioningProfile(provisioningProfile);
            }
        }

        void ApplyKeychainAccessGroups(XCodeProjectEditor editor)
        {
            var keychainAccessGroups = (ArrayList)_datastore["keychainAccessGroups"];
            if(keychainAccessGroups != null)
            {
                foreach(string accessGroup in keychainAccessGroups)
                {
                    editor.AddKeychainAccessGroup(accessGroup);
                }
            }
        }
    }
}

