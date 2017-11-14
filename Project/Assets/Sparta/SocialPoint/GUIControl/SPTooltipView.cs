using UnityEngine;
using SocialPoint.GUIControl;
using UnityEngine.EventSystems;
using SocialPoint.Base;
using System;

namespace SocialPoint.GUIControl
{
    [AddComponentMenu("UI/Extensions/SPTooltip Item")]
    public abstract class SPTooltipView : UIViewController
    {
        public enum ArrowPosition
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
        RectTransform _baseTransform;
        Vector2 _boundDelta;
        Vector3 _offset;
        Transform _parentTransform;
        ArrowPosition _spikePosition;
//        bool _isShown;
        bool _closeTimed;
        float _timeToClose;
        float _time;
//        IEnumerator _appearCoroutine;
//        IEnumerator _disappearCoroutine;

//        EventSystem _eventSystem;

        public void Init(Vector2 boundDelta, Transform parentTransform, ArrowPosition spikePosition, Vector3 offset, float timeToClose, Action finishHideCallback, Action hideTimedCallback)
        {
//            _eventSystem = Services.Instance.Resolve<EventSystem>();

            _baseCanvas = GetComponent<Canvas>();
            if(_baseCanvas == null)
            {
                throw new MissingComponentException("Base transform need to have a canvas setted up correctly in inspector");
            }

            _baseTransform = GetComponent<RectTransform>();


            _boundDelta = boundDelta;
            _parentTransform = parentTransform;
            _spikePosition = spikePosition;
            _offset = offset;
            _timeToClose = timeToClose;
            _finishHideCallback = finishHideCallback;
            _hideTimedCallback = hideTimedCallback;
//            _isShown = false;

            SetTooltipInfo();
            ShowTooltip();
        }

        public abstract void SetTooltipInfo();
            
        public void ShowTooltip()
        {
            SetSpikeAndPivots();

            transform.SetParent(_parentTransform);
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;

            SetTooltipWorldPosition(_parentTransform.position + _offset);

            Show();
        }

        protected override void OnAppeared()
        {
            base.OnAppeared();

            _closeTimed = _timeToClose > 0f;
            _time = 0f;
        }

        public void HideTooltip(bool immediate)
        {
            if(IsStable)
            {
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
            }

            base.OnDisappeared();
        }
 
        void SetSpikeAndPivots()
        {
            switch(_spikePosition)
            {
            case ArrowPosition.BestFit:
                // TODO
                break;

            case ArrowPosition.Left:
                _baseTransform.pivot = new Vector2(0f, 0.5f);
                _contentTransform.pivot = new Vector2(0f, 0.5f);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 270f);
                break;

            case ArrowPosition.Top:
                _baseTransform.pivot = new Vector2(0.5f, 0f);
                _contentTransform.pivot = new Vector2(0.5f, 0f);
                _spikeTransform.localRotation = Quaternion.Euler(Vector3.zero);
                break;

            case ArrowPosition.Right:
                _baseTransform.pivot = new Vector2(1f, 0.5f);
                _contentTransform.pivot = new Vector2(1f, 0.5f);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                break;

            case ArrowPosition.Bottom:
                _baseTransform.pivot = new Vector2(0.5f, 1f);
                _contentTransform.pivot = new Vector2(0.5f, 1f);
                _spikeTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
                break;
            }

            _spikeTransform.position = _baseTransform.position;
        }

        void SetTooltipScreenPosition(Vector2 pos)
        {
            var renderingCamera = _contentTransform.GetRectScreenPointCamera(_baseCanvas);
            if(renderingCamera != null)
            {
                SetTooltipFinalPosition(renderingCamera, pos);
            }
            else
            {
                _contentTransform.position = (Vector3)pos;

                // TODO check if is visible without a camera, then check which corners and size to adjust
            }
        }

        void SetTooltipWorldPosition(Vector3 pos)
        {
            var renderingCamera = _contentTransform.GetRectScreenPointCamera(_baseCanvas);
            if(renderingCamera != null)
            {
                Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(renderingCamera, pos);
                SetTooltipFinalPosition(renderingCamera, screenPos);
            }
            else
            {
                _contentTransform.position = pos;

                // TODO check if is visible without a camera, then check which corners and size to adjust
            }
        }

        void SetTooltipFinalPosition(Camera renderingCamera, Vector2 pos)
        {
            var screenBounds = new Rect(0f + _boundDelta.x, 0f + _boundDelta.y, Screen.width - _boundDelta.x, Screen.height - _boundDelta.y);

            Vector3 worldPos;
            var finalPos = pos;
            if(_contentTransform.IsOutOfBounds(renderingCamera, screenBounds))
            {
                var tooltipBounds = _contentTransform.GetScreenRect(renderingCamera);
                var pivot = _contentTransform.pivot;

                // Check x axis
                if((int)tooltipBounds.x < (int)screenBounds.x)
                {
                    finalPos.x = screenBounds.x + (tooltipBounds.width * pivot.x);
                }
                else if((int)tooltipBounds.x + tooltipBounds.width > (int)screenBounds.width)
                {
                    finalPos.x = screenBounds.width - (tooltipBounds.width * pivot.x);
                }

                // Check y axis
                if((int)tooltipBounds.y <= (int)screenBounds.y)
                {
                    finalPos.y = screenBounds.y + (tooltipBounds.height * pivot.y);
                }
                else if((int)(tooltipBounds.y + tooltipBounds.height) > (int)screenBounds.height)
                {
                    finalPos.y = screenBounds.height - (tooltipBounds.height * pivot.y);
                }
            }

            RectTransformUtility.ScreenPointToWorldPointInRectangle(_contentTransform, finalPos, renderingCamera, out worldPos);
            _contentTransform.position = worldPos;
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
