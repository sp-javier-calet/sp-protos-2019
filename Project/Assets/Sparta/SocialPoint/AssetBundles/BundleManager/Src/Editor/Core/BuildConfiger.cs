using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

/**
 * Build settings
 */ 
public class BuildConfiger
{
    /**
     * Compress bundles
     */ 
    public static bool Compress
    {
        get{ return BMDataAccessor.BMConfiger.compress;}
        set
        {
            if(BMDataAccessor.BMConfiger.compress != value)
            {
                BMDataAccessor.BMConfiger.compress = value;
                BundleManager.UpdateAllBundleChangeTime();
            }
        }
    }
    
    /**
     * Build deterministic Bundles
     */ 
    public static bool DeterministicBundle
    {
        get{ return BMDataAccessor.BMConfiger.deterministicBundle;}
        set
        {
            if(BMDataAccessor.BMConfiger.deterministicBundle != value)
            {
                BMDataAccessor.BMConfiger.deterministicBundle = value;
                BundleManager.UpdateAllBundleChangeTime();
            }
        }
    }
    
    /** 
     * Target platform
     */ 
    public static BuildPlatform BundleBuildTarget
    {
        get
        {
            return BMDataAccessor.Urls.bundleTarget;
        }
        set
        {
            BMDataAccessor.Urls.bundleTarget = value;
        }
    }

    public static string BundleTextureFormat
    {
        get
        {
            if (BMDataAccessor.Urls.androidSubTarget != null)
            {
                return BMDataAccessor.Urls.androidSubTarget.ToString ();
            }
            else
            {
                return null;
            }
        }
        set
        {
            string val = BMUrls.StandarizeTextureFormat (value);
            if (val != BMDataAccessor.Urls.androidSubTarget)
            {
                if (val == string.Empty)
                {
                    BMDataAccessor.Urls.androidSubTarget = null;
                }
                else
                {
                    BMDataAccessor.Urls.androidSubTarget = val;
                }
            }
        }
    }

    /** 
     * Target platform
     */
    public static bool UseEditorTarget
    {
        get
        {
            return BMDataAccessor.Urls.useEditorTarget;
        }
        set
        {
            BMDataAccessor.Urls.useEditorTarget = value;
        }
    }
    
    /**
     * Bundle file's suffix
     */ 
    public static string BundleSuffix
    {
        get{ return BMDataAccessor.BMConfiger.bundleSuffix;}
        set{ BMDataAccessor.BMConfiger.bundleSuffix = value;}
    }
     
    /**
     * Current output string for target platform
     */
    public static string BuildOutputStr
    {
        get
        {
            return BMDataAccessor.Urls.outputs[BMDataAccessor.Urls.bundleTarget.ToString()];
        }
        set
        {
            var urls = BMDataAccessor.Urls.outputs;
            string platformStr = BMDataAccessor.Urls.bundleTarget.ToString();
            string origValue = urls[platformStr];
            urls[platformStr] = value;
            if(origValue != value)
            {
                BMDataAccessor.SaveUrls();
            }
        }
    }
     
    internal static string InterpretedOutputPath
    {
        get
        {
            return BMDataAccessor.Urls.GetInterpretedOutputPath(BMDataAccessor.Urls.bundleTarget, 
                                                                texfmt: BMDataAccessor.Urls.androidSubTarget);
        }
    }
    
    internal static BuildOptions BuildOptions
    {
        get
        {
            return BMDataAccessor.BMConfiger.compress ? 0 : BuildOptions.UncompressedAssetBundle;
        }
    }
    
    internal static BuildTarget UnityBuildTarget
    {
        get
        {
            if(BuildConfiger.UseEditorTarget)
            {
                BuildConfiger.UnityBuildTarget = EditorUserBuildSettings.activeBuildTarget;
            }

            switch(BundleBuildTarget)
            {
            case BuildPlatform.Standalones:
                if(Application.platform == RuntimePlatform.OSXEditor)
                {
                    return BuildTarget.StandaloneOSXIntel;
                }
                else
                {
                    return BuildTarget.StandaloneWindows;
                }
            case BuildPlatform.WebPlayer:
                return BuildTarget.WebPlayer;
            case BuildPlatform.IOS:
#if UNITY_5
                return BuildTarget.iOS;
#else
                return BuildTarget.iPhone;
#endif
            case BuildPlatform.Android:
                return BuildTarget.Android;
            default:
                Debug.LogError("Internal error. Cannot find BuildTarget for " + BundleBuildTarget);
                return BuildTarget.StandaloneWindows;
            }
        }
        set
        {
            switch(value)
            {
            case BuildTarget.StandaloneGLESEmu:
            case BuildTarget.StandaloneLinux:
            case BuildTarget.StandaloneLinux64:
            case BuildTarget.StandaloneLinuxUniversal:
            case BuildTarget.StandaloneOSXIntel:
            case BuildTarget.StandaloneWindows:
            case BuildTarget.StandaloneWindows64:
                BundleBuildTarget = BuildPlatform.Standalones;
                break;
            case BuildTarget.WebPlayer:
            case BuildTarget.WebPlayerStreamed:
                BundleBuildTarget = BuildPlatform.WebPlayer;
                break;
            case BuildTarget.iOS:
            case BuildTarget.tvOS:
                BundleBuildTarget = BuildPlatform.IOS;
                break;
            case BuildTarget.Android:
                BundleBuildTarget = BuildPlatform.Android;
                break;
            default:
                Debug.LogError("Internal error. Bundle Manager does not have support for platform " + value);
                BundleBuildTarget = BuildPlatform.Standalones;
                break;
            }
        }
    }

    //CUSTOM:
    public static bool UseCustomBuildBundleProcedures
    {
        get{ return BMDataAccessor.BMConfiger.useCustomBuildBundleProcedures;}
        set
        {
            if(BMDataAccessor.BMConfiger.useCustomBuildBundleProcedures != value)
            {
                BMDataAccessor.BMConfiger.useCustomBuildBundleProcedures = value;
            }
        }
    }
    public static string useCustomBuildBundleProcedure_tt = "Call to derived BuildBundleProcedure classes in the different bundle build steps(including serialization)." +
        "This is the correct way to add custom bundle-build functionality per project.";

    public static string BuildBundleProcedures
    {
        get
        {
            return string.Join(";", BMDataAccessor.BMConfiger.buildBundleClasses);
        }
        set
        {
            if(BuildBundleProcedures != value)
            {
                BMDataAccessor.BMConfiger.buildBundleClasses = value.Split(new char[] {';'});
            }
        }
    }

    public static string[] BuildBundleProceduresArray()
    {
        return BuildBundleProcedures.Split(new char[] {';'});
    }

    public static string buildBundleProcedures_tt = "Class names derived from BuildBundleProcedure's implementations. They must all reside in the Editor assembly." +
        "They will be called in different situations depending on their base class, in string order and must be separated by ';' characters. Also bear in mind possible namespaces " +
            "must appear in the class name.";
    //
}