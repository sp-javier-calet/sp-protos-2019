using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class FadeAnimation : UIViewAnimation
{
    float _time = 1.0f;
    float _minAlpha = 0.0f;
    float _maxAlpha = 1.0f;
    UIViewController _ctrl;

    public void Load(UIViewController ctrl)
    {
        _maxAlpha = ctrl.Alpha;
        _ctrl = ctrl;
    }

    public FadeAnimation(float time)
    {
        _time = time;
    }
        
    public IEnumerator Appear()
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

    public IEnumerator Disappear()
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

    public void Reset()
    {
        _ctrl.Alpha = _maxAlpha;
    }

    public object Clone()
    {
        var anim = new FadeAnimation(_time);
        return anim;
    }
}