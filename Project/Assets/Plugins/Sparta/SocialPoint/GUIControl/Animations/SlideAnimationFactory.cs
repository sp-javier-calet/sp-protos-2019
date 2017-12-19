using UnityEngine;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public class SlideAnimation : UIViewAnimation
    {
        public enum PosType
        {
            Left,
            Right,
            Top,
            Down,
            Center
        }

        float _duration;
        PosType _fromPos = PosType.Center;
        PosType _toPos = PosType.Center;
        GoEaseType _easeType = GoEaseType.Linear;
        AnimationCurve _easeCurve = default(AnimationCurve);
        RectTransform _rectTransform;

        public void Load(GameObject gameObject)
        {
            _rectTransform = gameObject.GetComponent<RectTransform>();
        }

        public SlideAnimation(float duration, PosType fromPos, PosType toPos, GoEaseType easeType, AnimationCurve easeCurve)
        {
            _duration = duration;
            _fromPos = fromPos;
            _toPos = toPos;
            _easeType = easeType;
            _easeCurve = easeCurve;
        }

        public IEnumerator Animate()
        {
            var initialPos = _rectTransform.localPosition;
            var finalPos = initialPos;

            GetPosition(ref initialPos, _fromPos);
            GetPosition(ref finalPos, _toPos);

            _rectTransform.localPosition = initialPos;
            CreateTween(finalPos);

            yield return null;
        }

        GoTween CreateTween(Vector3 finalValue)
        {
            if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
            {
                return Go.to(_rectTransform, _duration, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
            }
            else
            {
                return Go.to(_rectTransform, _duration, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType));
            }
        }

        void GetPosition(ref Vector3 pos, PosType position)
        {
            if(position == PosType.Right)
            {
                pos.x = (_rectTransform.sizeDelta.x + _rectTransform.rect.width);
            }
            else if(position == PosType.Left)
            {
                pos.x = -(_rectTransform.sizeDelta.x + _rectTransform.rect.width);
            }
            else if(position == PosType.Top)
            {
                pos.y = (_rectTransform.sizeDelta.y + _rectTransform.rect.height);
            }
            else if(position == PosType.Down)
            {
                pos.y = -(_rectTransform.sizeDelta.y + _rectTransform.rect.height);
            }
        }
    }

    [CreateAssetMenu(menuName = "UI Animations/Slide Animation")]
    public class SlideAnimationFactory : UIViewAnimationFactory
    {
        public float Duration;
        public SlideAnimation.PosType FromPos = SlideAnimation.PosType.Center;
        public SlideAnimation.PosType ToPos = SlideAnimation.PosType.Center;
        public GoEaseType EaseType = GoEaseType.Linear;
        public AnimationCurve EaseCurve = default(AnimationCurve);

        public override UIViewAnimation Create()
        {
            return new SlideAnimation(Duration, FromPos, ToPos, EaseType, EaseCurve);
        }
    }
}