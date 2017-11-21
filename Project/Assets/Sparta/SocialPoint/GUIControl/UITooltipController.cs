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

        public void ShowTooltip(GameObject prefab, RectTransform triggerTransform, SPTooltipViewController.SpikePosition spikePosition, Vector3 offset, float timeToclose)
        {
            HideTooltip(true);

            if(prefab != null)
            {
                _currentTooltipGO = CreateTooltip(prefab);
                if(_currentTooltipGO != null)
                {
                    var spTooltipItem = _currentTooltipGO.GetComponent<SPTooltipViewController>();
                    if(spTooltipItem != null)
                    {
                        spTooltipItem.HideTimedCallback = HideTooltipTimed;
                        spTooltipItem.Init(_screenBounds, _rectTransform, triggerTransform, spikePosition, offset, timeToclose);
                        spTooltipItem.LoadAppearAnimation(AppearAnimationFactory, AppearAnimation);
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
                        spTooltipItem.LoadDisappearAnimation(DisappearAnimationFactory, DisappearAnimation);
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
