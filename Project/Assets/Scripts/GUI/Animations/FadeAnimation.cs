using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Fade Animation")]
public class FadeAnimation : UIViewAnimation<UIViewController>
{
    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    float _minAlpha = 0.0f;

    [SerializeField]
    float _maxAlpha = 1.0f;

    UIViewController _ctrl;

    public override void Load(UIViewController ctrl)
    {
        _maxAlpha = ctrl.Alpha;
        _ctrl = ctrl;
    }

    public FadeAnimation(float time)
    {
        _time = time;
    }
        
    public override IEnumerator Appear()
    {
        _ctrl.Alpha = _minAlpha;

        var elapsedTime = 0.0f;
        while(elapsedTime <= _time)
        {
            elapsedTime += Time.deltaTime;
            _ctrl.Alpha = Mathf.Lerp(_minAlpha, _maxAlpha, (elapsedTime / _time));
            yield return null;
        }
    }

    public override IEnumerator Disappear()
    {
        _ctrl.Alpha = _maxAlpha;

        var elapsedTime = 0.0f;
        while(elapsedTime <= _time)
        {
            elapsedTime += Time.deltaTime;
            _ctrl.Alpha = Mathf.Lerp(_maxAlpha, _minAlpha, (elapsedTime / _time));
            yield return null;
        }
    }

    public override void Reset()
    {
        if(_ctrl != null)
        {
            _ctrl.Alpha = _maxAlpha;
        }
    }

    public override object Clone()
    {
        var anim = new FadeAnimation(_time);
        return anim;
    }
}