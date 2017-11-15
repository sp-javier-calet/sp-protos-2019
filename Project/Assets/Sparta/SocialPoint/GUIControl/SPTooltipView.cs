using UnityEngine;
using SocialPoint.GUIControl;
using UnityEngine.EventSystems;
using SocialPoint.Base;
using System;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPTooltip Item")]
    public class SPTooltipView : UIViewController
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

        Action _finishHideCallback;
        Action _hideTimedCallback;

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

        //        EventSystem _eventSystem;

        public SPTooltipView()
        {
            IsFullScreen = false;
        }

        public void Init(Rect screenBounds, RectTransform uiControllerTransform, RectTransform triggerTransform, SpikePosition spikePosition, Vector3 offset, float timeToClose, Action hideTimedCallback)
        {
//            _eventSystem = Services.Instance.Resolve<EventSystem>();

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
            _hideTimedCallback = hideTimedCallback;

            SetTooltipInfo();
        }

        public virtual void SetTooltipInfo()
        {
        }

        void MoveTooltipToTriggerPosition()
        {
            _baseTransform.position = _triggerTransform.position + _offset;
            _baseTransform.localScale = Vector3.one;
        }

        public void ShowTooltip()
        {
            _baseTransform.SetParent(_uiControllerTransform);

            _camera = _baseCanvas.GetCamera();
            SetSpikeAndPivots(_spikePosition);
            if(_contentTransform.IsOutOfBounds(_camera, _screenBounds))
            {
                if(!TryToRepositionAsBestFit())
                {
                    Log.w("Tooltip cannot be fit correctly in screen. Move the tooltip trigger " + _triggerTransform.name + " to a new desired position");
                }
            }
                
            Show();
        }
            
        protected override void OnAppeared()
        {
            base.OnAppeared();

            _closeTimed = _timeToClose > 0f;
            _time = 0f;
        }

        public void HideTooltip(bool immediate, Action finishHideCallback)
        {
            if(IsStable)
            {
                _finishHideCallback = finishHideCallback;

                if(immediate)
                {
                    Hide();
                }
                else
                {
                    HideImmediate();
                }
            }
        }

        protected override void OnDisappeared()
        {
            if(_finishHideCallback != null)
            {
                _finishHideCallback();
                _finishHideCallback = null;
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
                _baseTransform.SetPivotAndAnchors(new Vector2(1f, 0.5f));
                _contentTransform.SetPivotAndAnchors(new Vector2(1f, 0.5f));
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case SpikePosition.Top:
                _baseTransform.SetPivotAndAnchors(new Vector2(0.5f, 0f));
                _contentTransform.SetPivotAndAnchors(new Vector2(0.5f, 0f));
                _spikeTransform.localRotation = Quaternion.Euler(Vector3.zero);
                break;

            case SpikePosition.Right:
                _baseTransform.SetPivotAndAnchors(new Vector2(0f, 0.5f));
                _contentTransform.SetPivotAndAnchors(new Vector2(0f, 0.5f));
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;

            case SpikePosition.Bottom:
                _baseTransform.SetPivotAndAnchors(new Vector2(0.5f, 1f));
                _contentTransform.SetPivotAndAnchors(new Vector2(0.5f, 1f));
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

                    if(_hideTimedCallback != null)
                    {
                        _hideTimedCallback();
                        _hideTimedCallback = null;
                    }
                }
            }
        }
    }
        
    // TEST WHILE WE CANNOT PROCESS CLICKS IN FULL SCREEN
    //        void Update()
    //        {
    //            if(_isShown && Input.GetMouseButtonUp(0))
    //            {
    //                // Does the RectTransform contain the screen point as seen from the given camera?
    //                Camera renderingCamera = _transform.GetRectScreenPointCamera(_canvas);
    //                bool contains = RectTransformUtility.RectangleContainsScreenPoint(_transform, Input.mousePosition, renderingCamera);
    //                if(!contains)
    //                {
    //                    HideTooltip();
    //                }
    //            }
    //        }
    //    }
}
