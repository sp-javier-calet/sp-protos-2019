using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Marketing;
using System;
using UnityEngine;

public sealed class SocialPointAppsFlyer : IMarketingTracker
{
    public struct AFConversionData
    {
        public string AdId;
        public string Status;
        public string MediaSource;
        public string Campaign;
    }

    public const string TrackerName = "appsflyer";
    public const string NonOrganicInstall = "Non-organic";
    public const string OrganicInstall = "Organic";
    public const string AdvertisingIdentifierKey = "ad_id";

    AppsFlyerTrackerCallbacks _trackerDelegate;

    public string AppsFlyerKey;
    public string AppID;

    public AFConversionData ConversionData
    {
        get;
        private set;
    }

    #region IMarketingTracker implementation

    public string Name
    { 
        get
        {
            return TrackerName;
        }
    }

    public void Init()
    {
        SetupAppsFlyerDelegate();
        DebugUtils.Assert(!String.IsNullOrEmpty(AppsFlyerKey));
        AppsFlyer.setAppsFlyerKey(AppsFlyerKey);
        DebugUtils.Assert(!String.IsNullOrEmpty(AppID));
        AppsFlyer.setAppID(AppID);
    }

    public void SetUserID(string userID)
    {
        AppsFlyer.setCustomerUserID(userID);
    }

    public void TrackInstall(bool isNewInstall)
    {
        #if UNITY_IOS
        AppsFlyer.getConversionData();
        AppsFlyer.trackAppLaunch();
        #elif UNITY_ANDROID
        /* This AppsFlyer.init method calls "loadConversionData" and triggers a "trackAppLaunch" in Android
         * (v4.15.1 of the Unity Plugin)
         * */
        AppsFlyer.init(AppsFlyerKey, "AppsFlyerTrackerCallbacks");
        #endif
    }

    public void SetDebugMode(bool debugMode)
    {
        AppsFlyer.setIsDebug(debugMode);
    }

    public event Action<TrackerAttributionData> OnDataReceived;

    #endregion

    void SetupAppsFlyerDelegate()
    {
        // The gameObject needs to be named AppsFlyerTrackerCallbacks for native callback reasons
        var gameObject = new GameObject("AppsFlyerTrackerCallbacks");
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        _trackerDelegate = gameObject.AddComponent<AppsFlyerTrackerCallbacks>();
        _trackerDelegate.OnConversionDataReceived = ParseDataReceived;
    }

    void ParseDataReceived(string data)
    {
        if(!string.IsNullOrEmpty(data))
        {
            try
            {
                var parser = new JsonAttrParser();
                AttrDic conversionDictionary = parser.ParseString(data).AsDic;
                SetConversionData(conversionDictionary);

                if(ConversionData.Status == NonOrganicInstall)
                {
                    OnDataReceived(new TrackerAttributionData {
                        trackerName = TrackerName,
                        data = data
                    });
                }
            }
            catch(Exception e)
            {
                Log.x(e);
            }
        }
    }

    void SetConversionData(AttrDic conversionDictionary)
    {
        var afData = new AFConversionData();
        afData.Status = conversionDictionary.Get("af_status").ToString();
        if(ConversionData.Status == NonOrganicInstall)
        {
            afData.AdId = conversionDictionary.Get(AdvertisingIdentifierKey).ToString();
            afData.MediaSource = conversionDictionary.Get("media_source").ToString();
            afData.Campaign = conversionDictionary.Get("campaign").ToString();
        }
        ConversionData = afData;
    }

    #region IDisposable implementation

    public void Dispose()
    {
        UnityEngine.Object.Destroy(_trackerDelegate.gameObject);
    }

    #endregion
    
}
