using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Slide Animation")]
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

    [SerializeField]
    RectTransform _transform;

    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    PosType _moveFromPos = PosType.Center;

    [SerializeField]
    PosType _moveToPos = PosType.Center;

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
            _transform = _ctrl.transform.GetChild(0) as RectTransform;
        }
            
        if(_transform == null)
        {
            throw new MissingComponentException("Could not find First Child RectTransform component.");
        }
    }
    
    public SlideAnimation(float time, PosType moveFromPos = PosType.Center, PosType moveToPos = PosType.Center, GoEaseType easeType = GoEaseType.Linear, AnimationCurve easeCurve = default(AnimationCurve))
    {
        _time = time;
        _moveFromPos = moveFromPos;
        _moveToPos = moveToPos;
        _easeType = easeType;
        _easeCurve = easeCurve;
    }

    public override IEnumerator Animate()
    {
        var initialPos = _transform.localPosition;
        var finalPos = initialPos;

        GetPosition(ref initialPos, _moveFromPos);
        GetPosition(ref finalPos, _moveToPos);

        _transform.localPosition = initialPos;
        yield return _ctrl.StartCoroutine(CreateTween(finalPos).waitForCompletion());
    }

    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_transform, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_transform, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType));
        }
    }

    void GetPosition(ref Vector3 pos, PosType position)
    {
        if(position == PosType.Right)
        {
            pos.x = (_transform.sizeDelta.x + _transform.rect.width);
        }
        else if(position == PosType.Left)
        {
            pos.x = -(_transform.sizeDelta.x + _transform.rect.width);
        }
        else if(position == PosType.Top)
        {
            pos.y = (_transform.sizeDelta.y + _transform.rect.height);
        }
        else if(position == PosType.Down)
        {
            pos.y = -(_transform.sizeDelta.y + _transform.rect.height);
        }
    }

    public override object Clone()
    {
        return new SlideAnimation(_time, _moveFromPos, _moveToPos, _easeType, _easeCurve);
    }
}