using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.GUIControl;
using System.Collections.Generic;

public class FadeAnimation : UIViewAnimation
{
    private float _speed = 1.0f;
    private float _maxAlpha = 1.0f;
    private UIViewController _ctrl;

    public void Load(UIViewController ctrl)
    {
        _maxAlpha = ctrl.Alpha;
        _ctrl = ctrl;
    }

    public FadeAnimation(float speed)
    {
        _speed = speed;
    }

    public IEnumerator Appear()
    {
        _ctrl.Alpha = 0.0f;

        while(_ctrl.Alpha < _maxAlpha)
        {
            float alpha = _ctrl.Alpha + (_speed * Time.deltaTime);

            _ctrl.Alpha = alpha;

            yield return null;
        }
    }

    public IEnumerator Disappear()
    {
        while(_ctrl.Alpha > 0.0f)
        {
            float alpha = _ctrl.Alpha - (_speed * Time.deltaTime);

            _ctrl.Alpha = alpha;

            yield return null;
        }
    }

    public void Reset()
    {
        _ctrl.Alpha = _maxAlpha;
    }

    public object Clone()
    {
        var anim = new FadeAnimation(_speed);
        return anim;
    }
}