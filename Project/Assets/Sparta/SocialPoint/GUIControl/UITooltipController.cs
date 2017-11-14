using UnityEngine;
using SocialPoint.Pooling;
using SocialPoint.Base;
using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public class UITooltipController : MonoBehaviour 
    {
        public const float DefaultAnimationTime = 1.0f;
        float AnimationTime = DefaultAnimationTime;

        [Header("Animations")]
        public UIViewAnimation AppearAnimation;
        public UIViewAnimation DisappearAnimation;

        [Header("System")]
        public bool UsePooling = true;
        public Vector2 ScreenBoundsDelta = Vector2.zero;

        GameObject _currentTooltipGO;
        GameObject _tempTooltipGO;

        public bool TooltipIsShown
        {
            get { return _currentTooltipGO != null; }
        }

        void Start()
        {
            AnimationTime = Services.Instance.Resolve("tooltip_animation_time", DefaultAnimationTime);
            AppearAnimation = new FadeAnimation(AnimationTime, 0f, 1f);
            DisappearAnimation = new FadeAnimation(AnimationTime, 1f, 0f);
        }  

        public void ShowTooltip(GameObject prefab, Transform parent, SPTooltipView.ArrowPosition arrowPosition, Vector3 offset, float timeToclose)
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
//                        spTooltipItem.Load(); // TODO
                        spTooltipItem.Init(this, ScreenBoundsDelta, parent, arrowPosition, offset, timeToclose, DestroyTooltip);
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
//                        spTooltipItem.Load(); // TODO
                    spTooltipItem.HideTooltip(immediate);
                }
            }
        }
            
        GameObject CreateTooltip(GameObject prefab)
        {
            return UsePooling ? UnityObjectPool.Spawn(prefab) : Object.Instantiate(prefab);
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
