using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using SocialPoint.Pooling;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPTooltip Trigger")]
    public class SPTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
    {
        [SerializeField]
        GameObject prefab;

        [SerializeField]
        SPTooltipView.ArrowPosition _arrowPosition = SPTooltipView.ArrowPosition.Default;

        [SerializeField]
        bool _useHover;

        [SerializeField]
        bool _useOffsetHover;

        [SerializeField]
        Vector3 _offset;

        [SerializeField]
        float _timeToClose = 0;

        UITooltipController _tooltipController;

        void Start()
        {
            _tooltipController = Services.Instance.Resolve<UITooltipController>();
            if(_tooltipController == null)
            {
                throw new InvalidOperationException("Could not find UI Tooltip Controller");
            }

            if(_tooltipController.UsePooling)
            {
                UnityObjectPool.CreatePool(prefab, 1);
            }
        }
            
        #region IPointerEnterHandler implementation

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(_useHover)
            {
                ShowTooltip();
            }
        }

        #endregion

        #region IPointerExitHandler implementation

        public void OnPointerExit(PointerEventData eventData)
        {
            if(_useHover)
            {
                HideTooltip();
            }
        }

        #endregion

        #region IPointerDownHandler implementation

        public void OnPointerDown(PointerEventData eventData)
        {
            if(!_useHover)
            {
                ShowTooltip();
            }
        }

        #endregion

        void ShowTooltip()
        {
            _tooltipController.ShowTooltip(prefab, transform, _arrowPosition, _offset, _timeToClose);
        }
            
        void HideTooltip()
        {
            _tooltipController.HideTooltip(false);
        }
    }
}
