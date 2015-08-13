using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.GUI;

public class FadeAnimation : UIViewAnimation {

    private float _speed = 1.0f;
    private float _maxAlpha = 1.0f;    
    private CanvasGroup _canvasGroup;

    public void Load(UIViewController ctrl)
    {
        _canvasGroup = ctrl.gameObject.GetComponent<CanvasGroup>();
        if(_canvasGroup == null)
        {
            throw new MissingComponentException("Could not find CanvasGroup component.");
        }
        _maxAlpha = _canvasGroup.alpha;
    }

    public FadeAnimation(float speed)
    {
        _speed = speed;
    }
    
    public IEnumerator Appear()
    {
        _canvasGroup.alpha = 0.0f;
        while(_canvasGroup.alpha < _maxAlpha)
        {
            _canvasGroup.alpha += _speed * Time.deltaTime;
            yield return null;
        }
    }
    
    public IEnumerator Disappear()
    {
        while(_canvasGroup.alpha > 0.0f)
        {
            _canvasGroup.alpha -= _speed * Time.deltaTime;
            yield return null;
        }
    }

    public void Reset()
    {
        _canvasGroup.alpha = _maxAlpha;
    }

    public object Clone()
    {
        var anim = new FadeAnimation(_speed);
        return anim;
    }
}

