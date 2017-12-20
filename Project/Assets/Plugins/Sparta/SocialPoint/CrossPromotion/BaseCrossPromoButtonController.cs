using UnityEngine;

namespace SocialPoint.CrossPromotion
{
    public class BaseCrossPromoButtonController : MonoBehaviour
    {
        protected CrossPromotionManager _cpm;

        public static BaseCrossPromoButtonController Create(CrossPromotionManager crossManager, GameObject prefab, Transform parent, bool rescale = false)
        {
            GameObject cpObj = Object.Instantiate(prefab);
            cpObj.transform.SetParent(parent, false);
            cpObj.transform.localPosition = Vector3.zero;
            cpObj.transform.localScale = Vector3.one;
            cpObj.GetComponent<BaseCrossPromoButtonController>().Init(crossManager, rescale);
            return cpObj.GetComponent<BaseCrossPromoButtonController>();
        }

        public virtual void Init(CrossPromotionManager crossManager, bool rescale)
        {
            _cpm = crossManager;
            _cpm.SendIconImpressedEvent();
        }

        public void OnPressButton()
        {
            _cpm.SendIconClickedEvent();
            _cpm.TryOpenPopup();
        }
    }
}
