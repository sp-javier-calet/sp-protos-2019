using UnityEngine;
using SocialPoint.CrossPromotion;
using SocialPoint.Utils;

public class GameCrossPromotionManager : CrossPromotionManager
{
    public GameObject ButtonPrefab;
    public GameObject PopupPrefab;

    readonly PopupsController _popupsController;

    public GameCrossPromotionManager(ICoroutineRunner coroutineRunner, INativeUtils nativeUtils, PopupsController popupsController) :
    base(coroutineRunner, nativeUtils)
    {
        _popupsController = popupsController;

        CreateIcon = CreateButtonCrossPromo;
        CreatePopup = CreatePopupCrossPromo;
    }

    void CreateButtonCrossPromo()
    {
        if(ButtonPrefab != null)
        {
            BaseCrossPromoButtonController.Create(this, ButtonPrefab, GameObject.Find("HUD").transform);
        }
    }

    void CreatePopupCrossPromo()
    {
        if(PopupPrefab != null)
        {
            var obj = GameObject.Instantiate(PopupPrefab);
            var crossPromoPopupController = obj.GetComponent<BaseCrossPromoPopupController>();
            _popupsController.Push(crossPromoPopupController);
            crossPromoPopupController.Init(this, _popupsController.Pop);
        }
    }
}
