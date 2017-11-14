using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class AnchoredPositionAnimation : UIViewAnimation
{
    float _duration;
    GoEaseType _easeType;
    AnimationCurve _easeCurve;
    Vector2 _finalPosition;
    RectTransform _rectTransform;

    public void Load(GameObject gameObject)
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
    }
        
    public AnchoredPositionAnimation(float duration, Vector2 finalPosition, GoEaseType easeType, AnimationCurve easeCurve)
    {
        _duration = duration;
        _finalPosition = finalPosition;
        _easeType = easeType;
        _easeCurve = easeCurve;
    }

    public IEnumerator Animate()
    {
        CreateTween(_finalPosition);

        yield return null;
    }

    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_rectTransform, _duration, new GoTweenConfig().anchoredPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_rectTransform, _duration, new GoTweenConfig().anchoredPosition(finalValue).setEaseType(_easeType));
        }
    }
}

[CreateAssetMenu(menuName = "UI Animations/Anchored Position Animation")]
public class AnchoredPositionAnimationFactory : UIViewAnimationFactory
{
    public float Duration;
    public Vector2 FinalPosition;
    public GoEaseType EaseType = GoEaseType.Linear;
    public AnimationCurve EaseCurve = default(AnimationCurve);

    public override UIViewAnimation Create()
    {
        return new AnchoredPositionAnimation(Duration, FinalPosition, EaseType, EaseCurve);
    }
}