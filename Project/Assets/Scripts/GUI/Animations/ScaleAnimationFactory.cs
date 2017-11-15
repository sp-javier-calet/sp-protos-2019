using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class ScaleAnimation : UIViewAnimation
{
    float _duration;
    Vector3 _initialScale;
    Vector3 _finalScale;
    GoEaseType _easeType;
    AnimationCurve _easeCurve;
    Transform _transform;

    public ScaleAnimation(float duration, Vector3 initialScale, Vector3 finalScale, GoEaseType easeType, AnimationCurve easeCurve)
    {
        _duration = duration;
        _initialScale = initialScale;
        _finalScale = finalScale;
        _easeType = easeType;
        _easeCurve = easeCurve;
    }
        
    public void Load(GameObject gameObject)
    {
        _transform = gameObject.transform;
    }

    public IEnumerator Animate()
    {
        _transform.localScale = _initialScale;
        CreateTween(_finalScale);

        yield return null;
    }
        
    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_transform, _duration, new GoTweenConfig().scale(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_transform, _duration, new GoTweenConfig().scale(finalValue).setEaseType(_easeType));
        }
    }
}

[CreateAssetMenu(menuName = "UI Animations/Scale Animation")]
public class ScaleAnimationFactory : UIViewAnimationFactory
{
    public float Duration;
    public Vector3 InitialScale = Vector3.zero;
    public Vector3 FinalScale = Vector3.one;
    public GoEaseType EaseType = GoEaseType.Linear;
    public AnimationCurve EaseCurve = default(AnimationCurve);

    public override UIViewAnimation Create()
    {
        return new ScaleAnimation(Duration, InitialScale, FinalScale, EaseType, EaseCurve);
    }
}