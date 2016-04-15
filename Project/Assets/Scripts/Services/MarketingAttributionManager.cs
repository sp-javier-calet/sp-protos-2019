using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Marketing;
using Zenject;

public class MarketingAttributionManager : SocialPointMarketingAttributionManager
{
    [InjectOptional]
    List<IMarketingTracker> injectTrackers
    {
        set
        {
            for(int i = 0; i < value.Count; i++)
            {
                AddTracker(value[i]);
            }
        }
    }

    public MarketingAttributionManager(IAppEvents appEvents, IAttrStorage storage) : base(appEvents, storage)
    {
    }

    [PostInject]
    void postInject()
    {
        GetUserID = () => "userID";
    }
}


