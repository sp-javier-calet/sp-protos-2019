using System.Collections;
using System.IO;

namespace SocialPoint.XCodeEditor
{
    public class XCBuildConfiguration : PBXObject
    {
        protected const string BUILDSETTINGS_KEY = "buildSettings";
        protected const string HEADER_SEARCH_PATHS_KEY = "HEADER_SEARCH_PATHS";
        protected const string LIBRARY_SEARCH_PATHS_KEY = "LIBRARY_SEARCH_PATHS";
        protected const string FRAMEWORK_SEARCH_PATHS_KEY = "FRAMEWORK_SEARCH_PATHS";
        protected const string OTHER_C_FLAGS_KEY = "OTHER_CFLAGS";
        protected const string OTHER_LD_FLAGS_KEY = "OTHER_LDFLAGS";
        protected const string GCC_ENABLE_CPP_EXCEPTIONS_KEY = "GCC_ENABLE_CPP_EXCEPTIONS";
        protected const string GCC_ENABLE_OBJC_EXCEPTIONS_KEY = "GCC_ENABLE_OBJC_EXCEPTIONS";
        protected const string PROVISIONING_PROFILE_KEY = "PROVISIONING_PROFILE";
        protected const string CODE_SIGN_IDENTITY_KEY = "CODE_SIGN_IDENTITY";
        protected const string CODE_SIGN_ENTITLEMENTS_KEY = "CODE_SIGN_ENTITLEMENTS";
        protected const string INFOPLIST_FILE_KEY = "INFOPLIST_FILE";
        protected const string PRODUCT_NAME_KEY = "PRODUCT_NAME";

        public XCBuildConfiguration(string guid, PBXDictionary dictionary) : base( guid, dictionary )
        {
        }
        
        public string infoPlistPath
        {
            get
            {
                if(buildSettings.ContainsKey(INFOPLIST_FILE_KEY))
                {
                    return (string)buildSettings[INFOPLIST_FILE_KEY];
                }

                return null;
            }
        }

        public string name
        {
            get
            {
                string name = productName;
                if(name == null)
                {
                    name = guid;
                }
                return name;
            }
        }

        public string productName
        {
            get
            {
                if(buildSettings.ContainsKey(PRODUCT_NAME_KEY))
                {
                    return (string)buildSettings[PRODUCT_NAME_KEY];
                }
                
                return null;
            }
        }
        
        public PBXDictionary buildSettings
        {
            get
            {
                if(ContainsKey(BUILDSETTINGS_KEY))
                {
                    return (PBXDictionary)_data[BUILDSETTINGS_KEY];
                }
            
                return null;
            }
        }

        public XCEntitlements GetEntitlements(XCProject project)
        {
            string path = entitlementsPath;
            if(path == null)
            {
                return null;
            }
            path = Path.Combine(project.projectRootPath, path);
            path = Path.GetFullPath(path);
            XCEntitlements plist = new XCEntitlements();
            plist.LoadFile(path);
            return plist;
        }

        public XCPlist GetInfoPlist(XCProject project)
        {
            string path = infoPlistPath;
            if(path == null)
            {
                return null;
            }
            path = Path.Combine(project.projectRootPath, path);
            path = Path.GetFullPath(path);
            XCPlist infoPlist = new XCPlist();
            infoPlist.LoadFile(path);
            return infoPlist;
        }
        
        protected bool AddSearchPaths(string path, string key, bool recursive = true)
        {
            PBXList paths = new PBXList();
            paths.Add(path);
            return AddSearchPaths(paths, key, recursive);
        }
        
        protected bool AddSearchPaths(PBXList paths, string key, bool recursive = true)
        {   
            bool modified = false;
            
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }
            
            foreach(string path in paths)
            {
                string currentPath = path;
                if(recursive && !path.EndsWith("/**"))
                {
                    currentPath += "/**";
                }
                
                if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(key))
                {
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add(key, new PBXList());
                }
                else
                if(((PBXDictionary)_data[BUILDSETTINGS_KEY])[key] is string)
                {
                    PBXList list = new PBXList();
                    list.Add(((PBXDictionary)_data[BUILDSETTINGS_KEY])[key]);
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY])[key] = list;
                }
                
                currentPath = "\\\"" + currentPath + "\\\"";
                
                if(!((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[key]).Contains(currentPath))
                {
                    ((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[key]).Add(currentPath);
                    modified = true;
                }
            }
        
            return modified;
        }

        public string entitlementsPath
        {
            get
            {
                if(!ContainsKey(BUILDSETTINGS_KEY))
                {
                    this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
                }
                
                string path = name + ".entitlements";
                if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(CODE_SIGN_ENTITLEMENTS_KEY))
                {
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add(CODE_SIGN_ENTITLEMENTS_KEY, path);
                }
                else
                {
                    path = (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[CODE_SIGN_ENTITLEMENTS_KEY];
                }
                return path.Replace("$(TARGET_NAME)", name);
            }
        }
        
        public bool AddHeaderSearchPaths(PBXList paths, bool recursive = true)
        {
            return this.AddSearchPaths(paths, HEADER_SEARCH_PATHS_KEY, recursive);
        }
        
        public bool AddLibrarySearchPaths(PBXList paths, bool recursive = true)
        {
            return this.AddSearchPaths(paths, LIBRARY_SEARCH_PATHS_KEY, recursive);
        }

        public bool AddFrameworkSearchPaths(PBXList paths, bool recursive = true)
        {
            return this.AddSearchPaths(paths, FRAMEWORK_SEARCH_PATHS_KEY, recursive);
        }
        
        public bool AddOtherCFlags(string flag)
        {
            PBXList flags = new PBXList();
            flags.Add(flag);
            return AddOtherCFlags(flags);
        }
        
        public bool AddOtherCFlags(PBXList flags)
        {
            bool modified = false;
            
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }
            
            foreach(string flag in flags)
            {
                
                if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(OTHER_C_FLAGS_KEY))
                {
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add(OTHER_C_FLAGS_KEY, new PBXList());
                }
                else
                if(((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY] is string)
                {
                    string tempString = (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY];
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY] = new PBXList();
                    ((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Add(tempString);
                }
                
                if(!((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Contains(flag))
                {
                    ((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_C_FLAGS_KEY]).Add(flag);
                    modified = true;
                }
            }
            
            return modified;
        }

        public bool AddOtherLDFlags(string flag)
        {
            //XCDebug.Log( "INIZIO A" );
            PBXList flags = new PBXList();
            flags.Add(flag);
            return AddOtherLDFlags(flags);
        }

        public bool AddOtherLDFlags(PBXList flags)
        {
            //XCDebug.Log( "INIZIO B" );
            
            bool modified = false;
            
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }
            
            foreach(string flag in flags)
            {
                
                if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(OTHER_LD_FLAGS_KEY))
                {
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY]).Add(OTHER_LD_FLAGS_KEY, new PBXList());
                }
                else
                if(((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY] is string)
                {
                    string tempString = (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY];
                    ((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY] = new PBXList();
                    ((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Add(tempString);
                }
                
                if(!((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Contains(flag))
                {
                    ((PBXList)((PBXDictionary)_data[BUILDSETTINGS_KEY])[OTHER_LD_FLAGS_KEY]).Add(flag);
                    modified = true;
                }
            }
            
            return modified;
        }

        public bool GccEnableCppExceptions(string value)
        {
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }

            ((PBXDictionary)_data[BUILDSETTINGS_KEY])[GCC_ENABLE_CPP_EXCEPTIONS_KEY] = value;
            return true;
        }

        public bool GccEnableObjCExceptions(string value)
        {
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }

            ((PBXDictionary)_data[BUILDSETTINGS_KEY])[GCC_ENABLE_OBJC_EXCEPTIONS_KEY] = value;
            return true;
        }

        public bool SetBuildSettings(Hashtable settings)
        {
            foreach(DictionaryEntry entry in settings)
            {
                ((PBXDictionary)_data[BUILDSETTINGS_KEY])[(string)entry.Key] = entry.Value;
            }
            return true;
        }

        public bool SetProvisioningProfile(XCMobileProvision prov)
        {
            if(!ContainsKey(BUILDSETTINGS_KEY))
            {
                this.Add(BUILDSETTINGS_KEY, new PBXDictionary());
            }

            bool modified = false;

            string uuid = prov.UUID;
            if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(PROVISIONING_PROFILE_KEY) ||
                (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[PROVISIONING_PROFILE_KEY] != uuid)
            {
                ((PBXDictionary)_data[BUILDSETTINGS_KEY])[PROVISIONING_PROFILE_KEY] = uuid;
                modified = true;
            }

            string teamname = prov.TeamName;
            if(!((PBXDictionary)_data[BUILDSETTINGS_KEY]).ContainsKey(CODE_SIGN_IDENTITY_KEY) ||
                (string)((PBXDictionary)_data[BUILDSETTINGS_KEY])[CODE_SIGN_IDENTITY_KEY] != teamname)
            {
                ((PBXDictionary)_data[BUILDSETTINGS_KEY])[CODE_SIGN_IDENTITY_KEY] = teamname;
                modified = true;
            }

            return modified;
        }
    }
}