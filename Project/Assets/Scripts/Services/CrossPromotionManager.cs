using UnityEngine;
using Zenject;
using SocialPoint.CrossPromotion;
using SocialPoint.AppEvents;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;

public class CrossPromotionManager : SocialPoint.CrossPromotion.CrossPromotionManager
{
    PopupsController _popupsController;

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

    public CrossPromotionManager(ICoroutineRunner coroutineRunner, PopupsController popupsController) :
        base(coroutineRunner)
    {
        _popupsController = popupsController;

        CreateIcon = CreateButtonCrossPromo;
        CreatePopup = CreatePopupCrossPromo;
    }

    private void CreateButtonCrossPromo()
    {
        //TODO: create the side bar icon panel and inject here to be used instead of HUD
        BaseCrossPromoButtonController.Create(this, "CrossPromotion/CrossPromoButton", GameObject.Find("HUD").transform);
    }

    private void CreatePopupCrossPromo()
    {
        GameObject prefab = Resources.Load("CrossPromotion/PopupCrossPromo") as GameObject;
        GameObject obj = GameObject.Instantiate(prefab) as GameObject;
        BaseCrossPromoPopupController crossPromoPopupController = obj.GetComponent<BaseCrossPromoPopupController>();
        _popupsController.Push(crossPromoPopupController);
        crossPromoPopupController.Init(this, () => { 
            _popupsController.Pop();
        });
    }
}
