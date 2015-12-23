using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SocialPoint.GUIControl;
using System.Collections.Generic;

public class FadeAnimation : UIViewAnimation
{
    private float _speed = 1.0f;
    private float _maxAlpha = 1.0f;
    private CanvasGroup _canvasGroup;
    private IList<Material> _3dElements = new List<Material>();

    public void Load(UIViewController ctrl)
    {
        _canvasGroup = ctrl.gameObject.GetComponent<CanvasGroup>();
        if(_canvasGroup == null)
        {
            throw new MissingComponentException("Could not find CanvasGroup component.");
        }
        _maxAlpha = _canvasGroup.alpha;

        _3dElements.Clear();

        foreach(GameObject element in ctrl.UI3DContainers)
        {
            _3dElements.Add(element.GetComponent<Renderer>().material);
        }
    }

    public FadeAnimation(float speed)
    {
        _speed = speed;
    }

    public IEnumerator Appear()
    {
        _canvasGroup.alpha = 0.0f;

        Set3DElementsAlpha(0.0f);

        while(_canvasGroup.alpha < _maxAlpha)
        {
            float alpha = _canvasGroup.alpha + (_speed * Time.deltaTime);

            _canvasGroup.alpha = alpha;

            Set3DElementsAlpha(alpha);

            yield return null;
        }
    }

    private void Set3DElementsAlpha(float alpha)
    {
        foreach(Material element in _3dElements)
        {
            element.color = new Color(element.color.r, element.color.g, element.color.b, alpha);
        }
    }

    public IEnumerator Disappear()
    {
        while(_canvasGroup.alpha > 0.0f)
        {
            float alpha = _canvasGroup.alpha - (_speed * Time.deltaTime);

            _canvasGroup.alpha = alpha;

            Set3DElementsAlpha(alpha);

            yield return null;
        }
    }

    public void Reset()
    {
        _canvasGroup.alpha = _maxAlpha;

        Set3DElementsAlpha(_maxAlpha);
    }

    public object Clone()
    {
        var anim = new FadeAnimation(_speed);
        return anim;
    }
}