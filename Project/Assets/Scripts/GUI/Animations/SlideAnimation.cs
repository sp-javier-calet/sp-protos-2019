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
    Transform _transform;

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

    RectTransform _rectTransform;
    UIViewController _ctrl;

    public override float Duration
    {
        get
        {
            return _time;
        }
    }

    public override void Load(UIViewController ctrl)
    {
        _ctrl = ctrl;
        if(_ctrl != null)        
        {
            if(_transform == null)
            {
                _transform = ctrl.transform;
                _rectTransform = _transform.GetChild(0).GetComponent<RectTransform>();
            }
            else
            {
                _rectTransform = _transform.GetComponent<RectTransform>();
            }

            if(_rectTransform == null)
            {
                throw new MissingComponentException("Could not find First Child RectTransform component.");
            }
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
        var initialPos = _rectTransform.localPosition;
        var finalPos = initialPos;

        GetPosition(ref initialPos, _moveFromPos);
        GetPosition(ref finalPos, _moveToPos);
        
        _rectTransform.localPosition = initialPos;
        yield return _ctrl.StartCoroutine(CreateTween(finalPos).waitForCompletion());
    }

    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_rectTransform, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_rectTransform, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType));
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

    public override void Reset() {}

    public override object Clone()
    {
        return new SlideAnimation(_time, _moveFromPos, _moveToPos, _easeType, _easeCurve);
    }
}