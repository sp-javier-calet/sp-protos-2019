using UnityEngine;
using System.Collections;

namespace SocialPoint.CrossPromotion
{
    public class BaseCrossPromoButtonController : MonoBehaviour
    {
        protected CrossPromotionManager _cpm;

        public static BaseCrossPromoButtonController Create(CrossPromotionManager crossManager, string prefab, Transform parent, bool rescale = false)
        {
            GameObject cpObj = GameObject.Instantiate(Resources.Load(prefab) as GameObject);
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
