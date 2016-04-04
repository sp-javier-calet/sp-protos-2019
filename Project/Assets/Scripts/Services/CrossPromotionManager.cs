using UnityEngine;
using SocialPoint.Dependency;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

public class CrossPromotionManager : SocialPoint.CrossPromotion.CrossPromotionManager 
{
    public CrossPromotionManager(ICoroutineRunner coroutineRunner) :
    base(coroutineRunner)
    {
        var eventTracker = ServiceLocator.Instance.Resolve<IEventTracker>();
        TrackSystemEvent = eventTracker.TrackSystemEvent;
        TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
    }
}
