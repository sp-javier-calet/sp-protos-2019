using UnityEngine;
using System.Collections;
using SocialPoint.GUI;
using UnityEngine.UI;

public class SlideAnimation : UIViewAnimation {
    
    private float _speed = 1.0f;
    private UIViewController _controller;
    private RectTransform _transform;

    public enum DirectionType
    {
        Left,
        Right
    }

    public DirectionType Direction;
    
    public void Load(UIViewController ctrl)
    {
        _controller = ctrl;
        _transform = _controller.gameObject.GetComponent<RectTransform>();
        if(_transform == null)
        {
            throw new MissingComponentException("Could not find RectTransform component.");
        }
    }
    
    public SlideAnimation(float speed, DirectionType dir=DirectionType.Right)
    {
        _speed = speed;
        Direction = dir;
    }
    
    public IEnumerator Appear()
    {
        var p = _transform.localPosition;
        var np = _transform.localPosition;
        if(Direction == DirectionType.Right)
        {
            np.x = (float)_transform.sizeDelta.x;
        }
        else
        if(Direction == DirectionType.Left)
        {
            np.x = (float)-_transform.sizeDelta.x;
        }
        _transform.localPosition = np;
        var tween = Go.to(_transform, 1f, new GoTweenConfig().localPosition(p));
        yield return _controller.StartCoroutine(tween.waitForCompletion());
    }
    
    public IEnumerator Disappear()
    {
        var op = _transform.localPosition;
        var p = op;
        if(Direction == DirectionType.Right)
        {
            p.x = (float)-_transform.sizeDelta.x;
        }
        else
        if(Direction == DirectionType.Left)
        {
            p.x = (float)_transform.sizeDelta.x;
        }
        var tween = Go.to(_transform, 1f, new GoTweenConfig().localPosition(p));
        yield return _controller.StartCoroutine(tween.waitForCompletion());
        _transform.localPosition = op;
    }

    public void Reset()
    {

    }
    
    public object Clone()
    {
        return new SlideAnimation(_speed, Direction);
    }
}

