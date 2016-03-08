using UnityEngine;
using Zenject;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

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

    public CrossPromotionManager(ICoroutineRunner coroutineRunner) :
    base(coroutineRunner)
    {
        
    }
}
