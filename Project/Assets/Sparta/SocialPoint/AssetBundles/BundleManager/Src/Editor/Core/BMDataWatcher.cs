using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class BMDataWatcher : AssetPostprocessor
{
    public static bool Active = true;

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach(string asset in importedAssets)
        {
            if(asset == BMDataAccessor.Paths.BundleDataPath || asset == BMDataAccessor.Paths.BundleBuildStatePath)
            {
                if(isDateChangedManually(asset))
                {
                    BundleManager.RefreshAll();
                }
            }
            else if(asset == BMDataAccessor.Paths.BMConfigerPath || asset == BMDataAccessor.Paths.UrlDataPath)
            {
                if(isDateChangedManually(asset))
                {
                    BMDataAccessor.Refresh();
                }
            }
        }
    }

    public static void MarkChangeDate(string path)
    {
        int[] date = BMUtility.long2doubleInt(BMUtility.GetLastWriteTime(path).ToBinary());
        PlayerPrefs.SetInt("BMChangeDate0", date[0]);
        PlayerPrefs.SetInt("BMChangeDate1", date[1]);
    }

    static bool isDateChangedManually(string asset)
    {
        if(!PlayerPrefs.HasKey("BMChangeDate0") || !PlayerPrefs.HasKey("BMChangeDate1"))
        {
            return false;
        }

        long assetChangeTime = BMUtility.GetLastWriteTime(asset).ToBinary();
        long markedChangeTime = BMUtility.doubleInt2long(PlayerPrefs.GetInt("BMChangeDate0"), PlayerPrefs.GetInt("BMChangeDate1"));
        return assetChangeTime != markedChangeTime;
    }
}
