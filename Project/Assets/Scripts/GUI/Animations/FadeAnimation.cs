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

    UIViewController _ctrl;

    public override void Load(UIViewController ctrl)
    {
        if(ctrl == null)
        {
            throw new MissingComponentException("UIViewController does not exist");
        }

        _ctrl = ctrl;
    }

    public FadeAnimation(float time, float initialAlpha, float finalAlpha)
    {
        _time = time;
        _initialAlpha = initialAlpha;
        _finalAlpha = finalAlpha;
    }
        
    public override IEnumerator Animate()
    {
        _ctrl.Alpha = _initialAlpha;

        var elapsedTime = 0.0f;
        while(elapsedTime <= _time)
        {
            elapsedTime += Time.deltaTime;
            _ctrl.Alpha = Mathf.Lerp(_initialAlpha, _finalAlpha, (elapsedTime / _time));
            yield return null;
        }
    }
        
    public override object Clone()
    {
        return new FadeAnimation(_time, _initialAlpha, _finalAlpha);
    }
}