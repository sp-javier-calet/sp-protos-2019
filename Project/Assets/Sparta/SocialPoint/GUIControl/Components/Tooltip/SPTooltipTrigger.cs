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
        SPTooltipViewController.SpikePosition _spikePosition = SPTooltipViewController.SpikePosition.Default;

        [SerializeField]
        bool _useHover;

        [SerializeField]
        bool _useOffsetHover;

        [SerializeField]
        Vector3 _offset;

        [SerializeField]
        float _timeToClose = 0;

        UITooltipController _tooltipController;
        RectTransform _rectTransform;
        bool _hovering;

        void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

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
            if(_useHover && !_hovering)
            {
                _hovering = true;
                ShowTooltip();
            }
        }

        #endregion

        #region IPointerExitHandler implementation

        public void OnPointerExit(PointerEventData eventData)
        {
            if(_useHover && _hovering)
            {
                _hovering = false;
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
            _tooltipController.ShowTooltip(prefab, _rectTransform, _spikePosition, (_useHover && _useOffsetHover ? _offset : Vector3.zero), (_useHover ? 0f : _timeToClose));
        }
            
        void HideTooltip()
        {
            _tooltipController.HideTooltip(false);
        }
    }
}
