using System.Collections.Generic;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Marketing;
using SocialPoint.Login;
using SocialPoint.Dependency;

public class MarketingAttributionManager : SocialPointMarketingAttributionManager
{
    public MarketingAttributionManager(IAppEvents appEvents, IAttrStorage storage) : base(appEvents, storage)
    {
        var login = ServiceLocator.Instance.Resolve<ILogin>();
        var trackers = ServiceLocator.Instance.ResolveList<IMarketingTracker>();
        for(int i = 0; i < trackers.Count; i++)
        {
            AddTracker(trackers[i]);
        }
        GetUserID = () => login.UserId.ToString();
    }
}


