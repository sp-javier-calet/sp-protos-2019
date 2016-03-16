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
        CreateIcon = CreateButtonCrossPromo;
        CreatePopup = CreatePopupCrossPromo;
    }

    private void CreateButtonCrossPromo()
    {
        //TODO
    }

    private void CreatePopupCrossPromo()
    {
        //TODO: Use a UIController to add the object to GUI_Root??
        GameObject prefab = Resources.Load("CrossPromotion/PopupCrossPromo") as GameObject;
        GameObject obj = GameObject.Instantiate(prefab) as GameObject;
        BasePopupCrossPromoController popupController = obj.GetComponent<BasePopupCrossPromoController>();
        popupController.Init(this, null);
    }
}
