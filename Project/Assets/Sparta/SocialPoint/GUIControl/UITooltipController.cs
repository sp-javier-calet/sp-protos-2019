using UnityEngine;
using SocialPoint.Pooling;
using SocialPoint.Base;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public class UITooltipController : UIViewController
    {
        public const float DefaultAnimationTime = 1.0f;
        float AnimationTime = DefaultAnimationTime;

        public bool UsePooling = true;
        public Vector2 ScreenBoundsDelta = Vector2.zero;

        GameObject _currentTooltipGO;
        GameObject _tempTooltipGO;
        RectTransform _rectTransform;
        Rect _screenBounds;

        public bool TooltipIsShown
        {
            get { return _currentTooltipGO != null; }
        }

        protected override void OnAwake()
        {
            base.OnAwake();

            _rectTransform = transform.GetComponent<RectTransform>();
        }

        protected override void OnStart()
        {
            base.OnStart();

            AnimationTime = Services.Instance.Resolve("tooltip_animation_time", DefaultAnimationTime);
            AppearAnimation = new FadeAnimation(AnimationTime, 0f, 1f);
            DisappearAnimation = new FadeAnimation(AnimationTime, 1f, 0f);

            _screenBounds = new Rect(0f + ScreenBoundsDelta.x, 0f + ScreenBoundsDelta.y, Screen.width - ScreenBoundsDelta.x, Screen.height - ScreenBoundsDelta.y);
        }

        public void ShowTooltip(GameObject prefab, RectTransform triggerTransform, SPTooltipView.SpikePosition spikePosition, Vector3 offset, float timeToclose)
        {
            HideTooltip(true);

            if(prefab != null)
            {
                _currentTooltipGO = CreateTooltip(prefab);
                if(_currentTooltipGO != null)
                {
                    var spTooltipItem = _currentTooltipGO.GetComponent<SPTooltipView>();
                    if(spTooltipItem != null)
                    {
                        
                        spTooltipItem.Init(_screenBounds, _rectTransform, triggerTransform, spikePosition, offset, timeToclose, DestroyTooltip, HideTooltipTimed);
                        spTooltipItem.LoadAppearAnimation(AppearAnimationFactory, AppearAnimation);
                        spTooltipItem.ShowTooltip();
                    }
                }
            }
        }

        public void HideTooltip(bool immediate)
        {
            if(_currentTooltipGO != null)
            {
                _tempTooltipGO = _currentTooltipGO;
                _currentTooltipGO = null;

                var spTooltipItem = _tempTooltipGO.GetComponent<SPTooltipView>();
                if(spTooltipItem != null)
                {
                    spTooltipItem.LoadDisappearAnimation(DisappearAnimationFactory, DisappearAnimation);
                    spTooltipItem.HideTooltip(immediate);
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

        void DestroyTooltip()
        {
            if(_tempTooltipGO != null)
            {
                if(UsePooling)
                {
                    UnityObjectPool.Recycle(_tempTooltipGO);
                }
                else
                {
                    _tempTooltipGO.DestroyAnyway();
                }

                _currentTooltipGO = null;
            }
        }
    }
}
