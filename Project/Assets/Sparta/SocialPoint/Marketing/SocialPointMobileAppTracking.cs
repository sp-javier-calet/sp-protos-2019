using SocialPoint.Marketing;
using SocialPoint.Base;
using System;
using UnityEngine;
using MATSDK;

public sealed class SocialPointMobileAppTracking : IMarketingTracker
{
    const string TrackerName = "tune";
    MATDelegate _trackerDelegate;
    
    public string AdvertiserID;
    public string ConversionKey;

    #region IMarketingTracker implementation

    public void Init()
    {
        DebugUtils.Assert(!String.IsNullOrEmpty(AdvertiserID));
        DebugUtils.Assert(!String.IsNullOrEmpty(ConversionKey));
        MATBinding.Init(AdvertiserID, ConversionKey);
    }

    public void TrackInstall(bool isNewInstall)
    {
        MATBinding.SetExistingUser(!isNewInstall);
        MATBinding.MeasureSession();
    }

    public void SetUserID(string userID)
    {
        MATBinding.SetUserId(userID);
    }

    public void SetDebugMode(bool debugMode)
    {
        MATBinding.SetDebugMode(debugMode);
    }

    public event Action<TrackerAttributionData> OnDataReceived;

    #endregion

    void SetupMobileAppTrackingDelegate()
    {
        // The gameObject needs to be named AppsFlyerTrackerCallbacks for native callback reasons
        GameObject gameObject = new GameObject("MobileAppTracker");
        UnityEngine.Object.DontDestroyOnLoad(gameObject);
        _trackerDelegate = gameObject.AddComponent<MATDelegate>();
        _trackerDelegate.OnTrackerDidSucced = ParseDataReceived;
    }

    void ParseDataReceived(string data)
    {
        OnDataReceived(new TrackerAttributionData { trackerName = TrackerName, data = data });
    }

    #region IDisposable implementation

    public void Dispose()
    {
        if(_trackerDelegate != null)
            UnityEngine.Object.Destroy(_trackerDelegate.gameObject);
    }

    #endregion
}
