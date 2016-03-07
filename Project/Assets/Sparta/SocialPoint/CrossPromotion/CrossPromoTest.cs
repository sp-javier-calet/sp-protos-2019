using UnityEngine;
using System.Collections;
using Zenject;
using SocialPoint.CrossPromotion;
using SocialPoint.ServerEvents;
using SocialPoint.Attributes;
using SocialPoint.AppEvents;

public class CrossPromoTest : MonoBehaviour
{
    [Inject]
    IEventTracker eventTracker;
    [Inject]
    IAppEvents appEvents;
    CrossPromotionManager xpromo;

    void Start()
    {
        xpromo = new CrossPromotionManager(this);
        xpromo.TrackSystemEvent = eventTracker.TrackSystemEvent;
        xpromo.TrackUrgentSystemEvent = eventTracker.TrackUrgentSystemEvent;
        xpromo.AppEvents = appEvents;
        xpromo.CreateIcon = () => {};
        xpromo.CreatePopup = () => {};

        TextAsset data = Resources.Load("xpromo") as TextAsset;
        AttrDic attr = new JsonAttrParser().Parse(data.bytes).AssertDic;
        xpromo.Init(attr.Get("xpromo").AsDic);
        xpromo.Start();
    }
}
