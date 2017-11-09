using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;
using UnityEngine.UI;

[CreateAssetMenu(menuName = "UI Animations/Scale Animation")]
public class ScaleAnimation : UIViewAnimation
{
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

    public ScaleAnimation(float time, Vector3 initialScale, Vector3 finalScale, GoEaseType easeType = GoEaseType.Linear, AnimationCurve easeCurve = default(AnimationCurve))
    {
        _time = time;
        _initialScale = initialScale;
        _finalScale = finalScale;
        _easeType = easeType;
        _easeCurve = easeCurve;
    }
        
    public override void Load(GameObject gameObject = null)
    {
        base.Load(gameObject);

        // HINT: If we want to scale a gameobject with Canvas Scaler component on it, we force to scale his first child instead
        var canvasScaler = _gameObject.GetComponent<CanvasScaler>();
        if(canvasScaler != null)
        {
            if(_transform.childCount > 0)
            {
                _transform = _transform.GetChild(0);
                _gameObject = _transform.gameObject;
            }
        }

        _rectTransform = _transform as RectTransform;
    }

    public override IEnumerator Animate()
    {
        if(_transform != null)
        {
            _transform.localScale = _initialScale;
            CreateTween(_finalScale);

            yield return null;
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