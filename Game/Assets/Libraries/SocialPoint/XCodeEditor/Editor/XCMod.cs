
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Json = SocialPoint.XCodeEditor.XMiniJSON;

namespace SocialPoint.XCodeEditor
{
    public struct XCModShellScript
    {
        public string Script;
        public string Shell;
        public string Target;
        public int Position;
    };

    public class XCMod
    {
        private Hashtable _datastore;
        private ArrayList _libs;
        
        public string name { get; private set; }

        public string path { get; private set; }
        
        public string group
        {
            get
            {
                return (string)_datastore["group"];
            }
        }
        
        public ArrayList patches
        {
            get
            {
                return (ArrayList)_datastore["patches"];
            }
        }
        
        public ArrayList libs
        {
            get
            {
                if(_libs == null && _datastore.ContainsKey("libs"))
                {
                    _libs = new ArrayList(((ArrayList)_datastore["libs"]).Count);
                    foreach(string fileRef in (ArrayList)_datastore["libs"])
                    {
                        _libs.Add(new XCModFile(fileRef));
                    }
                }
                return _libs;
            }
        }

        public string provisioningProfile
        {
            get
            {
                return (string)_datastore["provisioningProfile"];
            }
        }

        public Hashtable targetAttributes
        {
            get
            {
                return (Hashtable)_datastore["targetAttributes"];
            }
        }

        public Hashtable variantGroups
        {
            get
            {
                return (Hashtable)_datastore["variantGroups"];
            }
        }
        
        public ArrayList frameworks
        {
            get
            {
                return (ArrayList)_datastore["frameworks"];
            }
        }

        public ArrayList keychainAccessGroups
        {
            get
            {
                return (ArrayList)_datastore["keychainAccessGroups"];
            }
        }

        public Hashtable systemCapabilities
        {
            get
            {
                return (Hashtable)_datastore["systemCapabilities"];
            }
        }
        
        public ArrayList headerpaths
        {
            get
            {
                return (ArrayList)_datastore["headerpaths"];
            }
        }

        public ArrayList librarysearchpaths
        {
            get
            {
                return (ArrayList)_datastore["librarysearchpaths"];
            }
        }

        public Hashtable buildSettings
        {
            get
            {
                return (Hashtable)_datastore["buildSettings"];
            }
        }
        
        public ArrayList files
        {
            get
            {
                return (ArrayList)_datastore["files"];
            }
        }

        public Hashtable copyFiles
        {
            get
            {
                return (Hashtable)_datastore["copyFiles"];
            }
        }
        
        public ArrayList folders
        {
            get
            {
                return (ArrayList)_datastore["folders"];
            }
        }
        
        public ArrayList excludes
        {
            get
            {
                return (ArrayList)_datastore["excludes"];
            }
        }

        public Hashtable infoPlist
        {
            get
            {
                return (Hashtable)_datastore["infoPlist"];
            }
        }

        IList<XCModShellScript> _shellScripts = null;

        public IList<XCModShellScript> shellScripts
        {
            get
            {
                if(_shellScripts == null)
                {
                    _shellScripts = new List<XCModShellScript>();
                    var data = (ArrayList)_datastore["shellScripts"];
                    if(data != null)
                    {
                        foreach(var elm in data)
                        {
                            if(elm != null)
                            {
                                var table = (Hashtable)(elm);
                                _shellScripts.Add(new XCModShellScript{
                                    Script = table["script"] != null ? table["script"].ToString() : null,
                                    Shell = table["shell"] != null ? table["shell"].ToString() : null,
                                    Target = table["target"] != null ? table["target"].ToString() : null,
                                    Position = table["position"] != null ? int.Parse(table["position"].ToString()) : 0
                                });
                            }
                        }
                    }
                }
                return _shellScripts;
            }
        }
        
        public XCMod(string filename): this(System.IO.Path.GetDirectoryName( filename ), filename)
        {   
        }

        public XCMod(string projectPath, string filename)
        {   
            FileInfo projectFileInfo = new FileInfo(filename);
            if(!projectFileInfo.Exists)
            {
                XCDebug.LogWarning("File does not exist.");
                return;
            }
            
            name = System.IO.Path.GetFileNameWithoutExtension(filename);
            path = projectPath;
            load(projectFileInfo.OpenText().ReadToEnd());
        }

        public XCMod(string projectPath, string projectName, string contents)
        {   
            name = projectName;
            path = projectPath;
            load(contents);
        }

        private void load(string contents)
        {
            _datastore = (Hashtable)XMiniJSON.jsonDecode(contents);
            if(_datastore == null)
            {
                throw new InvalidDataException("Could not load json");
            }
        }

    }
    
    public class XCModFile
    {
        public string filePath { get; private set; }

        public bool isWeak { get; private set; }

        public string sourceTree { get; private set; }
        
        public XCModFile(string inputString)
        {
            isWeak = false;
            sourceTree = "SDKROOT";
            if(inputString.Contains(":"))
            {
                string[] parts = inputString.Split(':');
                filePath = parts[0];
                isWeak = System.Array.IndexOf(parts, "weak", 1) > 0;
                
                if(System.Array.IndexOf(parts, "<group>", 1) > 0)
                {
                    sourceTree = "GROUP";
                }
                else
                {
                    sourceTree = "SDKROOT";
                }
                
            }
            else
            {
                filePath = inputString;
            }
        }
    }
}