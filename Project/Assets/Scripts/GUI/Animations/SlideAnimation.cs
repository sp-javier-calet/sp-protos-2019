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
    float _time = 1.0f;

    [SerializeField]
    PosType _moveFromPos = PosType.Center;

    [SerializeField]
    PosType _moveToPos = PosType.Center;

    [SerializeField]
    GoEaseType _easeType = GoEaseType.Linear;

    [SerializeField]
    AnimationCurve _easeCurve = default(AnimationCurve);
 
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
        CreateTween(finalPos);

        yield return null;
    }

    GoTween CreateTween(Vector3 finalValue)
    {
        if(_easeType == GoEaseType.AnimationCurve && _easeCurve != null)
        {
            return Go.to(_gameObject, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType).setEaseCurve(_easeCurve));
        }
        else
        {
            return Go.to(_gameObject, _time, new GoTweenConfig().localPosition(finalValue).setEaseType(_easeType));
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

    public override object Clone()
    {
        return new SlideAnimation(_time, _moveFromPos, _moveToPos, _easeType, _easeCurve);
    }
}