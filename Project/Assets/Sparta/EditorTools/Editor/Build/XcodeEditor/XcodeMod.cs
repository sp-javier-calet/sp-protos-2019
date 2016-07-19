using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace SpartaTools.Editor.Build.XcodeEditor
{
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

            var content = file.OpenText().ReadToEnd();
            _datastore = (Hashtable)XMiniJSON.jsonDecode(content);
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

            foreach(DictionaryEntry entry in copyFiles)
            {
                string fromPath = (string)entry.Key;

                if(entry.Value is string)
                {
                    editor.CopyFile(_filePath, fromPath, (string)entry.Value);
                }
                else
                {
                    var toPaths = (IList<string>)entry.Value;
                    foreach(var dst in toPaths)
                    {
                        editor.CopyFile(_basePath, fromPath, dst);
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
                    editor.AddFile(file);
                }
            }
        }

        void ApplyFolders(XCodeProjectEditor editor)
        {
            var folders = (ArrayList)_datastore["folder"];
            if(folders != null)
            {
                foreach(string folder in folders)
                {
                    editor.AddFolder(folder);
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
                    editor.AddLibrary(lib);
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
                    string[] filename = framework.Split(':');
                    bool isWeak = (filename.Length > 1 && filename[1].Contains("weak"));
                    editor.AddFramework(filename[0], isWeak);
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
                    editor.SetPlistField((string)field.Key, (Dictionary<string, object>)field.Value);
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
                    var valueTable = (Hashtable)capability.Value;
                    bool enabled = (bool)valueTable["enabled"];
                    editor.SetSystemCapability((string)capability.Key, enabled);
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

