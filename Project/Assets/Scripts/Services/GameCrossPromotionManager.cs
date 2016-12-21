using UnityEngine;
using SocialPoint.CrossPromotion;
using SocialPoint.Utils;

public class GameCrossPromotionManager : CrossPromotionManager
{
    PopupsController _popupsController;

    public GameCrossPromotionManager(ICoroutineRunner coroutineRunner, PopupsController popupsController) :
        base(coroutineRunner)
    {
        _popupsController = popupsController;

        CreateIcon = CreateButtonCrossPromo;
        CreatePopup = CreatePopupCrossPromo;
    }

    private void CreateButtonCrossPromo()
    {
        BaseCrossPromoButtonController.Create(this, "uGUI/CrossPromoButton", GameObject.Find("HUD").transform);
    }

    private void CreatePopupCrossPromo()
    {
        GameObject prefab = Resources.Load("uGUI/PopupCrossPromo") as GameObject;
        GameObject obj = GameObject.Instantiate(prefab) as GameObject;
        BaseCrossPromoPopupController crossPromoPopupController = obj.GetComponent<BaseCrossPromoPopupController>();
        _popupsController.Push(crossPromoPopupController);
        crossPromoPopupController.Init(this, () => { 
            _popupsController.Pop();
        });
    }
}
