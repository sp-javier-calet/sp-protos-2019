using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Scale Animation")]
public class ScaleAnimation : UIViewAnimation
{
    [SerializeField]
    Transform _transform;

    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    Vector3 _initialScale = Vector3.zero;

    [SerializeField]
    Vector3 _finalScale = Vector3.one;

    [SerializeField]
    GoEaseType _easeType = GoEaseType.Linear;

    [SerializeField]
    AnimationCurve _easeCurve = default(AnimationCurve);

    UIViewController _ctrl;

    public override void Load(UIViewController ctrl)
    {
        if(ctrl == null)
        {
            throw new MissingComponentException("UIViewController does not exist");
        }

        _ctrl = ctrl;

        if(_transform == null && _ctrl.transform.childCount > 0)
        {
            _transform = _ctrl.transform.GetChild(0);
        }
            
        if(_transform == null)
        {
            throw new MissingComponentException("Could not find First Child Transform component.");
        }
    }

    public ScaleAnimation(float time, Vector3 initialScale, Vector3 finalScale, GoEaseType easeType = GoEaseType.Linear, AnimationCurve easeCurve = default(AnimationCurve))
    {
        _time = time;
        _initialScale = initialScale;
        _finalScale = finalScale;
        _easeType = easeType;
        _easeCurve = easeCurve;
    }
        
    public override IEnumerator Animate()
    {
        if(_transform != null)
        {
            _transform.localScale = _initialScale;

            yield return _ctrl.StartCoroutine(CreateTween(_finalScale).waitForCompletion());
        }
    }
        
    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_transform, _time, new GoTweenConfig().scale(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_transform, _time, new GoTweenConfig().scale(finalValue).setEaseType(_easeType));
        }
    }
        
    public override object Clone()
    {
        return new ScaleAnimation(_time, _initialScale, _finalScale, _easeType, _easeCurve);
    }
}