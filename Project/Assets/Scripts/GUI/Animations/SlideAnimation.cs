using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Slide Animation")]
public class SlideAnimation : UIViewAnimation<UIViewController>
{
    [SerializeField]
    float _time = 1.0f;

    UIViewController _controller;
    RectTransform _transform;

    public enum DirectionType
    {
        Left,
        Right
    }

    [SerializeField]
    DirectionType _direction;
    
    public override void Load(UIViewController ctrl)
    {
        _controller = ctrl;
        _transform = _controller.gameObject.GetComponent<RectTransform>();
        if(_transform == null)
        {
            throw new MissingComponentException("Could not find RectTransform component.");
        }
    }
    
    public SlideAnimation(float time, DirectionType dir=DirectionType.Right)
    {
        _time = time;
        _direction = dir;
    }
    
    public override IEnumerator Appear()
    {
        var p = _transform.localPosition;
        var np = _transform.localPosition;

        if(_direction == DirectionType.Right)
        {
            np.x = _transform.sizeDelta.x;
        }
        else
        if(_direction == DirectionType.Left)
        {
            np.x = -_transform.sizeDelta.x;
        }
        
        _transform.localPosition = np;
        var tween = Go.to(_transform, _time, new GoTweenConfig().localPosition(p));

        yield return _controller.StartCoroutine(tween.waitForCompletion());
    }
    
    public override IEnumerator Disappear()
    {
        var op = _transform.localPosition;
        var p = op;

        if(_direction == DirectionType.Right)
        {
            p.x = -_transform.sizeDelta.x;
        }
        else
        if(_direction == DirectionType.Left)
        {
            p.x = _transform.sizeDelta.x;
        }
        
        var tween = Go.to(_transform, _time, new GoTweenConfig().localPosition(p));

        yield return _controller.StartCoroutine(tween.waitForCompletion());
        _transform.localPosition = op;
    }

    public override void Reset() {}
        
    public override object Clone()
    {
        return new SlideAnimation(_time, _direction);
    }
}