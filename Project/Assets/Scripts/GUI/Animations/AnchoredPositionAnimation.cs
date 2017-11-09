using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Anchored Position Animation")]
public class AnchoredPositionAnimation : UIViewAnimation
{
    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    GoEaseType _easeType = GoEaseType.Linear;

    [SerializeField]
    AnimationCurve _easeCurve = default(AnimationCurve);

    [SerializeField]
    Vector2 _finalPosition;
 
    public override void Load(GameObject gameObject = null)
    {
        base.Load(gameObject);

        // HINT: If we want to move a gameobject with Canvas component on it, we force to move his first child instead
        var canvas = _gameObject.GetComponent<Canvas>();
        if(canvas != null)
        {
            if(_transform.childCount > 0)
            {
                _transform = _transform.GetChild(0);
                _gameObject = _transform.gameObject;
            }
        }

        _rectTransform = _transform as RectTransform;
    }
        
    public AnchoredPositionAnimation(float time, Vector2 finalPosition, GoEaseType easeType = GoEaseType.Linear, AnimationCurve easeCurve = default(AnimationCurve))
    {
        _time = time;
        _easeType = easeType;
        _easeCurve = easeCurve;
        _finalPosition = finalPosition;
    }

    public override IEnumerator Animate()
    {
        CreateTween(_finalPosition);

        yield return null;
    }

    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_rectTransform, 0.3f, new GoTweenConfig().anchoredPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_rectTransform, 0.3f, new GoTweenConfig().anchoredPosition(finalValue).setEaseType(_easeType));
        }
    }
        
    public override object Clone()
    {
        return new AnchoredPositionAnimation(_time, _finalPosition, _easeType, _easeCurve);
    }
}