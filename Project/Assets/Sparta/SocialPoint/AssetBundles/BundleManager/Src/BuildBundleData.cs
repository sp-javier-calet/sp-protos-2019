using System.Collections.Generic;
using UnityEngine;
using LitJson;

/**
 * All informations of a bundle
 * Use the BundleManager APIs to change the bundle content, don't change the members of this class directly.
 */ 
public class BundleData
{
    /**
     * Name of the bundle. The name should be uniqle in all bundles
     */ 
    public string       name = "";
    
    /**
     * List of paths included. The path can be directories.
     */ 
    public List<string> includs = new List<string>();

    /**
     * List of paths currently depended. One asset can be depended but not included in the Includes list
     */ 
    public List<string> dependAssets = new List<string>();
    public List<string> includeGUIDs = new List<string>();
    public List<string> dependGUIDs = new List<string>();
    public List<string> jsonDataAssetsGUIDS = new List<string>();
    
    /**
     * Is this bundle a scene bundle?
     */ 
    public bool         sceneBundle = false;

    /**
     * Default download priority of this bundle.
     */ 
    public int          priority = 0;
    
    /**
     * Parent name of this bundle.
     */ 
    public string       parent = "";
    
    /**
     * Childrens' name of this bundle
     */ 
    public List<string> children = new List<string>();
}

public class BundleBuildState
{
    public string       bundleName = "";
    public int          version = -1;
    public uint         crc = 0;
    public long         size = -1;
    public long         changeTime = -1;
    public string[]     lastBuildDependencies = null;
    public int          platform = -1;
    public int          androidSubTarget = -1;

    //CUSTOM: this property might be custom
    public bool         PackageContainsLocalFile{ get { return version.Equals(-1); } }
    //
}

public class BMConfiger
{
    public bool             compress = true;
    public bool             deterministicBundle = true;
    public string           bundleSuffix = "assetBundle";
    public string           buildOutputPath = "";
    public bool             useCache = true;
    public bool             useCRC = true;
    public int              downloadThreadsCount = 3;
    public int              downloadRetryTime = 2;
    public int              bmVersion = 1;
    //CUSTOM:
    public bool             useCustomBuildBundleProcedures = false;
    public string[]         buildBundleClasses = new string[] {};
    //
}

public class BMUrls
{
    static readonly string DEFAULT_OUTPUT = "$(DataPath)/../AssetBundle/$(Platform)";

    public Dictionary<string, string> downloadUrls;
    public Dictionary<string, string> outputs;
    public BuildPlatform bundleTarget = BuildPlatform.Standalones;
    public bool useEditorTarget = true;
    public bool downloadFromOutput = false;
    public bool offlineCache = false;
    public string androidSubTarget = null;

    static string _bestTextureFormatSupport = null;
    
    public BMUrls()
    {
        downloadUrls = new Dictionary<string, string>()
        {
            {"WebPlayer", ""},
            {"Standalones", ""},
            {"IOS", ""},
            {"Android", ""},
            {"WP8", ""}
        };
        outputs = new Dictionary<string, string>()
        {
            {"WebPlayer", DEFAULT_OUTPUT},
            {"Standalones", DEFAULT_OUTPUT},
            {"IOS", DEFAULT_OUTPUT},
            {"Android", "$(DataPath)/../AssetBundle/$(Platform)_$(TextureFmt)"},
            {"WP8", DEFAULT_OUTPUT}
        };
    }
    
    public string GetInterpretedDownloadUrl(BuildPlatform platform, string texfmt=null)
    {
        return BMUtility.InterpretPath(downloadUrls[platform.ToString()], platform, texfmt: texfmt);
    }
    
    public string GetInterpretedOutputPath(BuildPlatform platform, string texfmt=null)
    {
        return BMUtility.InterpretPath(outputs[platform.ToString()], platform, texfmt: texfmt);
    }

    public static string StandarizeTextureFormat(string txtfmt)
    {
        switch(txtfmt.ToLowerInvariant())
        {
        case "atitc":
        case "atc":
            {
                return "ATC";
            }
        case "dxt":
        case "s3tc":
        case "dxtc":
            {
                return "DXT";
            }
        case "pvr":
        case "pvrtc":
            {
                return "PVRTC";
            }
        case "etc":
        case "etc1":
            {
                return "ETC";
            }
        case "etc2":
            {
                return "ETC2";
            }
        default:
            return string.Empty;
        }
    }

    public static string GetBestTextureFormatSupport()
    {
		if (BMUrls._bestTextureFormatSupport == null)
        {
            #if UNITY_ANDROID
            #if MULTI_ANDROID_TEXTURE_ENABLED
            if(SystemInfo.SupportsTextureFormat(TextureFormat.ETC2_RGB))
            {
				BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("etc2");
            }
            else if(SystemInfo.SupportsTextureFormat(TextureFormat.DXT1))
            {
				BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("dxt");
            }
            else if(SystemInfo.SupportsTextureFormat(TextureFormat.PVRTC_RGB4))
            {
				BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("pvr");
            }
            else if(SystemInfo.SupportsTextureFormat(TextureFormat.ATC_RGB4))
            {
				BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("atc");
            }
            else
            {
				BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("etc");
            }
            #else
			BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("etc");
            #endif
            #elif UNITY_EDITOR
			BMUrls._bestTextureFormatSupport = StandarizeTextureFormat("etc");
			Debug.LogWarning(string.Format("Texture support while in Editor will always default to '{0}'", BMUrls._bestTextureFormatSupport));
            #else
			BMUrls._bestTextureFormatSupport = string.Empty;
            #endif
        }
		return BMUrls._bestTextureFormatSupport;
    }

    public static string SerializeToString(BMUrls urls)
    {
        return JsonMapper.ToJson(urls);
    }
}

public enum BuildPlatform
{
    WebPlayer,
    Standalones,
    IOS,
    Android,
    WP8,
}
