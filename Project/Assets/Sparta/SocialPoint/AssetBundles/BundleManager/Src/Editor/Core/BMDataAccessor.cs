using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using LitJson;
using SocialPointEditor.Assets.PlatformEx;

[InitializeOnLoad]
internal class BMDataAccessor
{
    static readonly string UNIQUE_PLATFORM_PTH_TMP = "Assets/BM_$(Platform)_$(TextureFmt)";
    static readonly string PREFERRED_DATA_PATH = "Assets/BundleManager";
    static readonly string[] MOST_LIKELY_SRC_PATHS = new string[] {"Assets/Sparta/AssetBundles/BundleManager", "Assets/Sparta", "Assets/Libraries/External/BundleManager", "Assets/Libraries"};

    public sealed class DataFilePaths
    {
        static readonly string DEFAULT_LOG = "<BM Config> Using default file location for {0}";
        static readonly string CUSTOM_LOG = "<BM Config> Using custom file location for {0}: {1}";

        string BasePath;
        public string BundleDataPath = null;
        public string BMConfigerPath = null;
        public string BundleBuildStatePath = null;
        public string UrlDataPath = null;

        public DataFilePaths()
        {
        }

        public DataFilePaths(string basePath)
        {
            BasePath = basePath;
            BundleDataPath = BasePath + "/BundleData.txt";
            BMConfigerPath = BasePath + "/BMConfiger.txt";
            BundleBuildStatePath = BasePath + "/BuildStates.txt";
            UrlDataPath = BasePath + "/Urls.txt";
        }

        public void CopyOver(DataFilePaths other)
        {
            BasePath = other.BasePath;
            if (other.BundleDataPath != null && File.Exists (other.BundleDataPath))
            {
                BundleDataPath = other.BundleDataPath;
            }
            if (other.BMConfigerPath != null && File.Exists (other.BMConfigerPath))
            {
                BMConfigerPath = other.BMConfigerPath;
            }
            if (other.BundleBuildStatePath != null && File.Exists (other.BundleBuildStatePath))
            {
                BundleBuildStatePath = other.BundleBuildStatePath;
            }
            if (other.UrlDataPath != null && File.Exists (other.UrlDataPath))
            {
                UrlDataPath = other.UrlDataPath;
            }
        }

        public void Log()
        {
            if(BundleDataPath.StartsWith(BasePath))
            {
                Debug.Log (string.Format(DEFAULT_LOG, "BundleDataPath"));
            }
            else
            {
                Debug.LogWarning (string.Format(CUSTOM_LOG, "BundleDataPath", BundleDataPath));
            }
            if(BMConfigerPath.StartsWith(BasePath))
            {
                Debug.Log (string.Format(DEFAULT_LOG, "BMConfigerPath"));
            }
            else
            {
                Debug.LogWarning (string.Format(CUSTOM_LOG, "BMConfigerPath", BMConfigerPath));
            }
            if(BundleBuildStatePath.StartsWith(BasePath))
            {
                Debug.Log (string.Format(DEFAULT_LOG, "BundleBuildStatePath"));
            }
            else
            {
                Debug.LogWarning (string.Format(CUSTOM_LOG, "BundleBuildStatePath", BundleBuildStatePath));
            }
            if(UrlDataPath.StartsWith(BasePath))
            {
                Debug.Log (string.Format(DEFAULT_LOG, "UrlDataPath"));
            }
            else
            {
                Debug.LogWarning (string.Format(CUSTOM_LOG, "UrlDataPath", UrlDataPath));
            }
        }
    }

    static BMDataAccessor()
    {
        SrcPath = FindSrcPath();
        BasePath = FindBasePath();
        bool freshInit = BasePath == null ? true : false;
        if (freshInit)
        {
            BasePath = FreshInitData();
        }
        Paths = new DataFilePaths(BasePath);
        if (freshInit)
        {
            SaveBMConfiger();
            SaveBundleData();
            SaveBundleBuildeStates();
            SaveUrls();
        }
    }

    static public List<BundleData> Bundles
    {
        get
        {
            if(m_Bundles == null)
            {
                m_Bundles = loadObjectFromJsonFile< List<BundleData> >(Paths.BundleDataPath);
            }
            
            if(m_Bundles == null)
            {
                m_Bundles = new List<BundleData>();
            }
            
            return m_Bundles;
        }
    }
    
    static public List<BundleBuildState> BuildStates
    {
        get
        {
            if(m_BuildStates == null)
            {
                m_BuildStates = loadObjectFromJsonFile< List<BundleBuildState> >(Paths.BundleBuildStatePath);
            }
            
            if(m_BuildStates == null)
            {
                m_BuildStates = new List<BundleBuildState>();
            }
            
            return m_BuildStates;
        }
    }
    
    static public BMConfiger BMConfiger
    {
        get
        {
            if(m_BMConfier == null)
            {
                m_BMConfier = loadObjectFromJsonFile<BMConfiger>(Paths.BMConfigerPath);
            }
            
            if(m_BMConfier == null)
            {
                m_BMConfier = new BMConfiger();
            }
            
            return m_BMConfier;
        }
    }
    
    static public BMUrls Urls
    {
        get
        {
            if(m_Urls == null)
            {
                m_Urls = loadObjectFromJsonFile<BMUrls>(Paths.UrlDataPath);
            }
            
            if(m_Urls == null)
            {
                m_Urls = new BMUrls();
            }
            
            return m_Urls;
        }
    }

    static public void Refresh()
    {
        m_Bundles = null;
        m_BuildStates = null;
        m_BMConfier = null;
        m_Urls = null;
    }
    
    static public void SaveBMConfiger()
    {
        saveObjectToJsonFile(BMConfiger, Paths.BMConfigerPath);
    }
    
    static public void SaveBundleData()
    {
        foreach(BundleData bundle in Bundles)
        {
            bundle.includeGUIDs.Sort(guidComp);
            bundle.includs = BundleManager.GUIDsToPaths(bundle.includeGUIDs);

            bundle.dependGUIDs.Sort(guidComp);
            bundle.dependAssets = BundleManager.GUIDsToPaths(bundle.dependGUIDs);
        }
        saveObjectToJsonFile(Bundles, Paths.BundleDataPath);
    }
    
    static public void SaveBundleBuildeStates()
    {
        saveObjectToJsonFile(BuildStates, Paths.BundleBuildStatePath);
    }

    /*
     * This will dump the buildState dict that BundleManager holds into
     * the BuildStates attribute and update the file
     */
    static public void DumpAndSaveBundleBuildeStates()
    {
        var newBundleStates = new List<BundleBuildState> ();
        foreach(var bundle in BundleManager.bundles)
        {
            var bundleState = BundleManager.GetBuildStateOfBundle(bundle.name);
            if (bundleState != null)
            {
                newBundleStates.Add (bundleState);
            }
        }
        m_BuildStates = newBundleStates;
        SaveBundleBuildeStates();
    }
    
    static public void SaveUrls()
    {
        saveObjectToJsonFile(Urls, Paths.UrlDataPath);
    }
        
    static public T loadObjectFromJsonFile<T>(string path)
    {  
        TextReader reader;
        try
        {
            reader = new StreamReader(path);
        }
        catch (FileNotFoundException)
        {
            reader = null;
        }

        if(reader == null)
        {
            Debug.LogError("Cannot find " + path);
            return default(T);
        }
        
        T data = JsonMapper.ToObject<T>(reader.ReadToEnd());
        if(data == null)
        {
            Debug.LogError("Cannot read data from " + path);
        }
        
        reader.Close();
        return data;
    }
    
    static private void saveObjectToJsonFile<T>(T data, string path)
    {
        TextWriter tw = new StreamWriter(path);
        if(tw == null)
        {
            Debug.LogError("Cannot write to " + path);
            return;
        }
        
        string jsonStr = JsonFormatter.PrettyPrint(JsonMapper.ToJson(data));
        
        tw.Write(jsonStr);
        tw.Flush();
        tw.Close();

        BMDataWatcher.MarkChangeDate(path);
    }

    static private int guidComp(string guid1, string guid2)
    {
        string fileName1 = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid1));
        string fileName2 = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid2));
        int ret = fileName1.CompareTo(fileName2);

        if(ret == 0)
        {
            return guid1.CompareTo(guid2);
        }
        else
        {
            return ret;
        }
    }

    /*
     * Should be called prior to any interaction with Bundle Manager
     */
    public static void SetNewPaths(DataFilePaths newPaths)
    {
        Paths.CopyOver(newPaths);
        Paths.Log();
        BundleManager.RefreshAll();
    }

    /*
     * Ensures a unique direcroty name in the Assets root for current platform(and textue compression if Android)
     */
    public static string GetUniquePlatformAssetDir(string suffix="")
    {
        var tmpPath = BMUtility.InterpretPath(UNIQUE_PLATFORM_PTH_TMP, Urls.bundleTarget, Urls.androidSubTarget);
        if(tmpPath.EndsWith("_"))
        {
            if(suffix != "")
            {
                return tmpPath + suffix;
            }
            else
            {
                return tmpPath.Substring(0, tmpPath.Length - 1);
            }
        }
        else
        {
            if(suffix != "")
            {
                return tmpPath + "_" + suffix;
            }
        }
        return tmpPath;
    }

    /// <summary>
    /// Will try to find the base path for the data.
    /// </summary>
    /// <returns>The base path or null.</returns>
    static private string FindBasePath()
    {
        string fullSysPath = Path.Combine(Application.dataPath, PREFERRED_DATA_PATH.Substring("Assets/".Length)).ToSysPath();
        string possiblePath;
        if (Directory.Exists(fullSysPath))
        {
            possiblePath = Path.Combine(fullSysPath, "BundleData.txt");
            if (File.Exists(possiblePath))
            {
                return PREFERRED_DATA_PATH;
            }
        }

        string fullSrcSysPath = Path.Combine(Application.dataPath, SrcPath.Substring("Assets/".Length)).ToSysPath();
        possiblePath = Path.Combine(fullSrcSysPath, "BundleData.txt");
        if (File.Exists(possiblePath))
        {
            Debug.LogWarning("Path for BundleManager data is the same as for source code. This is not recomended.");
            return SrcPath;
        }

        Debug.LogWarning(String.Format("Could not find a base path for BundleManager data. It should be either in {0} or in the source path {1}.", PREFERRED_DATA_PATH, SrcPath));
        return null;
    }

    /// <summary>
    /// It will try to generate the preferred folder with empty/default data structures.
    /// </summary>
    static private string FreshInitData()
    {
        //Create and save data files
        var fullSysPath = Path.Combine(Application.dataPath, PREFERRED_DATA_PATH.Substring("Assets/".Length)).ToSysPath();
        if (!Directory.Exists(fullSysPath))
        {
            AssetDatabase.CreateFolder(Path.GetDirectoryName(PREFERRED_DATA_PATH), 
                Path.GetFileNameWithoutExtension(PREFERRED_DATA_PATH));
        }
        Debug.LogWarning("BundleManager fresh start");
        return PREFERRED_DATA_PATH;
    }

    static private string FindSrcPath()
    {
        foreach(string mostLikelyPath in MOST_LIKELY_SRC_PATHS)
        {
            string fullSysPath = Path.Combine(Application.dataPath, mostLikelyPath.Substring("Assets/".Length)).ToSysPath();
            if (Directory.Exists(fullSysPath))
            {
                var matchedFiles = Directory.GetFiles(fullSysPath, "customStyles.asset", SearchOption.AllDirectories);
                if(matchedFiles.Length > 0)
                {
                    if(matchedFiles.Length != 1)
                    {
                        throw new Exception("Could not find a src path for BundleManager. More than one 'customStyles.asset' files found in the project.");
                    }

                    return Path.GetDirectoryName(matchedFiles[0]).NormalizedReplace(Application.dataPath, "Assets");
                }
            }
        }

        throw new Exception("Could not find a src path for BundleManager. No suitable folder found.");
    }
    
    static private List<BundleData> m_Bundles = null;
    static private List<BundleBuildState> m_BuildStates = null;
    static private BMConfiger m_BMConfier = null;
    static private BMUrls m_Urls = null;
    public static string BasePath;
    public static string SrcPath;
    public static DataFilePaths Paths;
}
