using UnityEngine;
using System.Collections;
using SocialPoint.Marketing;

public sealed class EmptyAppsFlyer : IMarketingTracker
{
    #region IMarketingTracker implementation

    public void Init()
    {
    }

    public void SetUserID(string userID)
    {
    }

    public void TrackInstall(bool isNewInstall)
    {
    }

    public void SetDebugMode(bool debugMode)
    {
    }

    public event System.Action<TrackerAttributionData> OnDataReceived;

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
    }

    #endregion
}
