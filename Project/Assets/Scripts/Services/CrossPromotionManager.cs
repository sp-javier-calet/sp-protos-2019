using UnityEngine;
using Zenject;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;

public class CrossPromotionManager : SocialPoint.CrossPromotion.CrossPromotionManager 
{
    [Inject]
    IEventTracker injectEventTracker
    {
        set
        {
            TrackSystemEvent = value.TrackSystemEvent;
            TrackUrgentSystemEvent = value.TrackUrgentSystemEvent;
        }
    }

    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    public CrossPromotionManager(MonoBehaviour behavior) :
    base(behavior)
    {
        
    }
}
