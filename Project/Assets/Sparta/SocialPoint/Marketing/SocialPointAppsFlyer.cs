using SocialPoint.Base;
using SocialPoint.Attributes;
using SocialPoint.Marketing;
using System;
using UnityEngine;

public sealed class SocialPointAppsFlyer : IMarketingTracker
{
    const string TrackerName = "appsflyer";
    AppsFlyerTrackerCallbacks _trackerDelegate;

    public string AppsFlyerKey;
    public string AppID;

    #region IMarketingTracker implementation

    public void Init()
    {
        SetupAppsFlyerDelegate();
    }

    public void SetUserID(string userID)
    {
        AppsFlyer.setCustomerUserID(userID);
    }

    public void TrackInstall(bool isNewInstall)
    {
        DebugUtils.Assert(!String.IsNullOrEmpty(AppID));
        AppsFlyer.setAppID(AppID);

        if(isNewInstall)
        {
            #if UNITY_IOS
            AppsFlyer.getConversionData();
            #elif UNITY_ANDROID
            AppsFlyer.loadConversionData("AppsFlyerTrackerCallbacks");
            #endif
        }

        /* We set AppsFlyer key here (and not on Init) because it triggers a trackAppLaunch on Android,
         * and we enclose our trackAppLaunch call inside the iOS ifdef because of that reason.
         * (It was happening with v4.14 of the Unity Plugin)
         * */
        DebugUtils.Assert(!String.IsNullOrEmpty(AppsFlyerKey));
        AppsFlyer.setAppsFlyerKey(AppsFlyerKey);
        #if UNITY_IOS
        AppsFlyer.trackAppLaunch();
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

                if(conversionDictionary != null && conversionDictionary.ContainsKey("af_status") && conversionDictionary.GetValue("af_status").ToString() == "Non-organic")
                {
                    OnDataReceived(new TrackerAttributionData{ trackerName = TrackerName, data = data });
                }
            }
            catch(Exception e)
            {
                Log.x(e);
            }
        }
    }

    #region IDisposable implementation

    public void Dispose()
    {
        UnityEngine.Object.Destroy(_trackerDelegate.gameObject);
    }

    #endregion
    
}
