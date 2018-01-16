using System;
using SocialPoint.Base;
using SocialPoint.GUIControl;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPTooltip Item")]
    public class SPTooltipViewController : UIViewController
    {
        public enum SpikePosition
        {
            Default,
            BestFit,
            Top,
            Bottom,
            Left,
            Right
        }

        [SerializeField]
        RectTransform _contentTransform;

        [SerializeField]
        RectTransform _spikeTransform;

        public Action FinishHideCallback;
        public Action HideTimedCallback;

        Canvas _baseCanvas;
        RectTransform _uiControllerTransform;
        RectTransform _baseTransform;
        RectTransform _triggerTransform;
        Rect _screenBounds;
        Vector3 _offset;
        SpikePosition _spikePosition;
        Camera _camera;
        bool _closeTimed;
        float _timeToClose;
        float _time;

        SpikePosition[] BestFitSpikeDirection = { SpikePosition.Top, SpikePosition.Bottom, SpikePosition.Left, SpikePosition.Right };

        public SPTooltipViewController()
        {
            IsFullScreen = false;
        }

        public void Init(Rect screenBounds, RectTransform uiControllerTransform, RectTransform triggerTransform, SpikePosition spikePosition, Vector3 offset, float timeToClose)
        {
            _baseCanvas = GetComponent<Canvas>();
            if(_baseCanvas == null)
            {
                throw new MissingComponentException("Base transform need to have a canvas setted up correctly in inspector");
            }

            _baseTransform = GetComponent<RectTransform>();
            _screenBounds = screenBounds;
            _uiControllerTransform = uiControllerTransform;
            _triggerTransform = triggerTransform;
            _spikePosition = spikePosition;
            _offset = offset;
            _timeToClose = timeToClose;

            SetTooltipInfo();
        }

        public virtual void SetTooltipInfo()
        {
        }

        void MoveTooltipToTriggerPosition()
        {
            // Regarding where we setup the transform pivot, we will force the initial position for the spawned Tooltip always to be in the Transform center
            var centerPoint = _triggerTransform.GetWorldCenterPoint();

            _baseTransform.position = centerPoint + _offset;
            _baseTransform.localScale = Vector3.one;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            _baseTransform.SetParent(_uiControllerTransform);

            _camera = _baseCanvas.GetCamera();
            SetSpikeAndPivots(_spikePosition);
            if(_contentTransform.IsOutOfBounds(_camera, _screenBounds))
            {
                if(!TryToRepositionAsBestFit())
                {
                    DebugLog("Tooltip cannot be fit correctly in screen. Move the tooltip trigger " + _triggerTransform.name + " to a new desired position");
                }
            }
        }
            
        protected override void OnAppeared()
        {
            base.OnAppeared();

            _closeTimed = _timeToClose > 0f;
            _time = 0f;
        }
            
        protected override void OnDisappeared()
        {
            if(FinishHideCallback != null)
            {
                FinishHideCallback();
                FinishHideCallback = null;
            }

            base.OnDisappeared();
        }

        void SetSpikeAndPivots(SpikePosition spikePosition)
        {
            switch(spikePosition)
            {
            case SpikePosition.BestFit:
                TryToRepositionAsBestFit();
                break;

            case SpikePosition.Left:
                _baseTransform.SetPivotAndAnchors(RectTransformExtensions.PivotMiddleRight);
                _contentTransform.SetPivotAndAnchors(RectTransformExtensions.PivotMiddleRight);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case SpikePosition.Top:
                _baseTransform.SetPivotAndAnchors(RectTransformExtensions.PivotBottomCenter);
                _contentTransform.SetPivotAndAnchors(RectTransformExtensions.PivotBottomCenter);
                _spikeTransform.localRotation = Quaternion.Euler(Vector3.zero);
                break;

            case SpikePosition.Right:
                _baseTransform.SetPivotAndAnchors(RectTransformExtensions.PivotMiddleLeft);
                _contentTransform.SetPivotAndAnchors(RectTransformExtensions.PivotMiddleLeft);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;

            case SpikePosition.Bottom:
                _baseTransform.SetPivotAndAnchors(RectTransformExtensions.PivotTopCenter);
                _contentTransform.SetPivotAndAnchors(RectTransformExtensions.PivotTopCenter);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                break;
            }

            _contentTransform.localPosition = Vector3.zero;
            _spikeTransform.localPosition = Vector3.zero;

            MoveTooltipToTriggerPosition();
        }

        bool TryToRepositionAsBestFit()
        {
            for(int i = 0; i < BestFitSpikeDirection.Length; ++i)
            {
                var spike = BestFitSpikeDirection[i];
                if(spike != _spikePosition)
                {
                    SetSpikeAndPivots(spike);
                    if(!_contentTransform.IsOutOfBounds(_camera, _screenBounds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        void LateUpdate()
        {
            if(_closeTimed)
            {
                _time += Time.deltaTime;
                if(_time >= _timeToClose)
                {
                    _closeTimed = false;

                    if(HideTimedCallback != null)
                    {
                        HideTimedCallback();
                        HideTimedCallback = null;
                    }
                }
            }
        }
    }
}
