using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.EventSystems;
using SocialPoint.Hardware;
using SocialPoint.Pooling;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SocialPoint.GUIControl
{
    public class UITooltipController : UIViewController, IPointerDownHandler
    {
        const float DefaultAnimationTime = 1.0f;
        const string ToolTipAnimationTime = "tooltip_animation_time";

        float AnimationTime = DefaultAnimationTime;
        public bool UsePooling = true;
        public Vector2 ScreenBoundsDelta = Vector2.zero;

        [SerializeField]
        LayerMask _ignoreDispatcherMask;

        [HideInInspector]
        public IDeviceInfo DeviceInfo;

        GameObject _currentTooltipGO;
        RectTransform _rectTransform;
        Rect _screenBounds;
        SPStandaloneInputModule _eventSystem;

        public bool TooltipIsShown
        {
            get { return _currentTooltipGO != null; }
        }

        #region IPointerDownHandler implementation

        void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
        {
            HideTooltip(false);
        }

        #endregion

        protected override void OnAwake()
        {
            base.OnAwake();

            _rectTransform = transform.GetComponent<RectTransform>();
        }

        protected override void OnStart()
        {
            base.OnStart();

            AnimationTime = Services.Instance.Resolve(ToolTipAnimationTime, DefaultAnimationTime);

            if(AppearAnimationFactory == null)
            {
                AppearAnimation = new FadeAnimation(AnimationTime, 0f, 1f);
            }

            if(DisappearAnimationFactory == null)
            {
                DisappearAnimation = new FadeAnimation(AnimationTime, 1f, 0f);
            }

            var screenWidth = DeviceInfo == null ? Screen.width : DeviceInfo.ScreenSize.x;
            var screenHeight = DeviceInfo == null ? Screen.height : DeviceInfo.ScreenSize.y;

            _screenBounds = new Rect(0f + ScreenBoundsDelta.x, 0f + ScreenBoundsDelta.y, screenWidth - ScreenBoundsDelta.x, screenHeight - ScreenBoundsDelta.y);

            _eventSystem = Services.Instance.Resolve<SPStandaloneInputModule>();
            if(_eventSystem != null)
            {
                _eventSystem.RegisterEventReceiver(EventTriggerType.PointerDown, gameObject, _ignoreDispatcherMask);
            }
        }
            
        protected override void OnDestroy()
        {
            if(_eventSystem != null)
            {
                _eventSystem.UnRegisterEventReceiver(EventTriggerType.PointerDown, gameObject);
            }

            base.OnDestroy();
        }

        public void ShowTooltip(GameObject prefab, RectTransform triggerTransform, SPTooltipViewController.SpikePosition spikePosition, Vector3 offset, float timeToclose)
        {
            HideTooltip(true);

            if(prefab != null && _currentTooltipGO == null)
            {
                _currentTooltipGO = CreateTooltip(prefab);
                if(_currentTooltipGO != null)
                {
                    var spTooltipItem = _currentTooltipGO.GetComponent<SPTooltipViewController>();
                    if(spTooltipItem != null)
                    {
                        spTooltipItem.HideTimedCallback = HideTooltipTimed;
                        spTooltipItem.Init(_screenBounds, _rectTransform, triggerTransform, spikePosition, offset, timeToclose);
                        spTooltipItem.SetupAppearAnimation(AppearAnimationFactory, AppearAnimation);
                        spTooltipItem.Show();
                    }
                }
            }
        }

        public void HideTooltip(bool immediate)
        {
            if(_currentTooltipGO != null)
            {
                var tempTooltipGO = _currentTooltipGO;
                _currentTooltipGO = null;

                var spTooltipItem = tempTooltipGO.GetComponent<SPTooltipViewController>();
                if(spTooltipItem != null)
                {
                    if(spTooltipItem.IsStable)
                    {
                        spTooltipItem.SetupDisappearAnimation(DisappearAnimationFactory, DisappearAnimation);
                    }

                    spTooltipItem.FinishHideCallback = (() => DestroyTooltip(tempTooltipGO));
                    if(!spTooltipItem.IsStable || immediate)
                    {
                        spTooltipItem.HideImmediate();
                    }
                    else
                    {
                        spTooltipItem.Hide();
                    }
                }
            }
        }

        GameObject CreateTooltip(GameObject prefab)
        {
            return UsePooling ? UnityObjectPool.Spawn(prefab) : Object.Instantiate(prefab);
        }

        void HideTooltipTimed()
        {
            HideTooltip(false);
        }

        void DestroyTooltip(GameObject tempTooltipGO)
        {
            if(tempTooltipGO != null)
            {
                if(UsePooling)
                {
                    UnityObjectPool.Recycle(tempTooltipGO);
                }
                else
                {
                    tempTooltipGO.DestroyAnyway();
                }
            }
        }
    }
}
