using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Fade Animation")]
public class FadeAnimation : UIViewAnimation
{
    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    float _initialAlpha = 0.0f;

    [SerializeField]
    float _finalAlpha = 1.0f;

    CanvasGroup _canvasGroup;

    public override void Load(GameObject gameObject = null)
    {
        base.Load(gameObject);

        _canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if(_canvasGroup == null)
        {
            throw new MissingComponentException("Missing CanvasGroup component in UIViewAnimation Load");
        }
    }

    public FadeAnimation(float time, float initialAlpha, float finalAlpha)
    {
        _time = time;
        _initialAlpha = initialAlpha;
        _finalAlpha = finalAlpha;
    }
        
    public override IEnumerator Animate()
    {
        _canvasGroup.alpha = _initialAlpha;

        var elapsedTime = 0.0f;
        while(elapsedTime <= _time)
        {
            elapsedTime += Time.deltaTime;
            _canvasGroup.alpha = Mathf.Lerp(_initialAlpha, _finalAlpha, (elapsedTime / _time));
            yield return null;
        }
    }
        
    public override object Clone()
    {
        return new FadeAnimation(_time, _initialAlpha, _finalAlpha);
    }
}