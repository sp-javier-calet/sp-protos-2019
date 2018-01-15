using System;
using SocialPoint.Dependency;
using SocialPoint.GUIControl;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPTooltip Trigger")]
    public class SPTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public enum TriggerType
        {
            Press,
            Hold,
            Hover
        }
            
        public GameObject Prefab;
        public TriggerType PressType;
        public SPTooltipViewController.SpikePosition SpikePosition = SPTooltipViewController.SpikePosition.Default;
        public float HoldTime;
        public Vector3 Offset;
        public float TimeToClose;

        UITooltipController _tooltipController;
        RectTransform _rectTransform;
        bool _hovering;
        bool _holding;
        float _time;

        #region Unity methods

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
                UnityObjectPool.CreatePool(Prefab, 1);
            }
        }

        void LateUpdate()
        {
            if(_holding)
            {
                _time += Time.deltaTime;
                if(_time >= HoldTime)
                {
                    _holding = false;
                    ShowTooltip();
                }
            }
        }

        #endregion
           
        #region Event Handler implementation

        public void OnPointerEnter(PointerEventData eventData)
        {
            if(PressType == TriggerType.Hover && !_hovering)
            {
                _hovering = true;
                ShowTooltip();
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if(PressType == TriggerType.Hover && _hovering)
            {
                _hovering = false;
                HideTooltip();
            }
        }
            
        public void OnPointerDown(PointerEventData eventData)
        {
            if(PressType == TriggerType.Press)
            {
                ShowTooltip();
            }
            else if(PressType == TriggerType.Hold)
            {
                _holding = true;
                _time = 0f;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _holding = false;
        }

        #endregion
            
        void ShowTooltip()
        {
            _tooltipController.ShowTooltip(Prefab, _rectTransform, SpikePosition, Offset, (PressType == TriggerType.Hover ? 0f : TimeToClose));
        }
            
        void HideTooltip()
        {
            _tooltipController.HideTooltip(false);
        }
    }
}
