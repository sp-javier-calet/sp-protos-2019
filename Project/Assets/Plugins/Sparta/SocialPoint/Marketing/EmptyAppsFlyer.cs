using SocialPoint.Marketing;

public sealed class EmptyAppsFlyer : IMarketingTracker
{
    #region IMarketingTracker implementation

    public string Name
    { 
        get
        {
            return SocialPointAppsFlyer.TrackerName;
        }
    }

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

    void DataReceived(TrackerAttributionData obj)
    {
        var handler = OnDataReceived;
        if(handler != null)
        {
            handler(obj);
        }
    }

    #endregion

    #region IDisposable implementation

    public void Dispose()
    {
    }

    #endregion
}
