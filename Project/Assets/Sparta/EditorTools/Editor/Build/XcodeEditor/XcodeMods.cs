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
            ApplyFrameworks(editor);
            ApplyCopyFiles(editor);
        }

        void ApplyFrameworks(XCodeProjectEditor editor)
        {
            var frameworks = (ArrayList)_datastore["frameworks"];
            if(frameworks != null)
            {
                foreach(string framework in frameworks)
                {
                    string[] filename = framework.Split(':');
                    bool isWeak = (filename.Length > 1) ? true : false;
                    string completePath = Path.Combine("System/Library/Frameworks", filename[0]);

                    editor.AddFramework(completePath, isWeak);
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
    }
}

