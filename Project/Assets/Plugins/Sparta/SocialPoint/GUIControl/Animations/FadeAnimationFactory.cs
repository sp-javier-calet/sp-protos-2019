using UnityEngine;
using System.Collections;

namespace SocialPoint.GUIControl
{
    public class FadeAnimation : UIViewAnimation
    {
        float _duration;
        float _initialAlpha;
        float _finalAlpha;
        CanvasGroup _canvasGroup;

        public void Load(GameObject gameObject)
        {
            _canvasGroup = gameObject.GetComponent<CanvasGroup>();
            if(_canvasGroup == null)
            {
                throw new MissingComponentException("Missing CanvasGroup component in UIViewAnimation Load");
            }
        }

        public FadeAnimation(float duration, float initialAlpha, float finalAlpha)
        {
            _duration = duration;
            _initialAlpha = initialAlpha;
            _finalAlpha = finalAlpha;
        }

        public IEnumerator Animate()
        {
            _canvasGroup.alpha = _initialAlpha;

            var elapsedTime = 0.0f;
            while(elapsedTime <= _duration)
            {
                elapsedTime += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(_initialAlpha, _finalAlpha, (elapsedTime / _duration));
                yield return null;
            }
        }
    }

    [CreateAssetMenu(menuName = "UI Animations/Fade Animation")]
    public class FadeAnimationFactory : UIViewAnimationFactory
    {
        public float Duration;
        public float InitialAlpha;
        public float FinalAlpha;

        public override UIViewAnimation Create()
        {
            return new FadeAnimation(Duration, InitialAlpha, FinalAlpha);
        }
    }
}
