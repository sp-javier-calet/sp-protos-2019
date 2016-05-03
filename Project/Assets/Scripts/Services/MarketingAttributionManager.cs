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
        var trackers = ServiceLocator.Instance.ResolveArray<IMarketingTracker>();
        for(int i = 0; i < trackers.Length; i++)
        {
            AddTracker(trackers[i]);
        }
        GetUserID = () => login.UserId.ToString();
    }
}


