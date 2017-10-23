using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Rotate Animation")]
public class RotateAnimation : UIViewAnimation<Transform>
{
    [SerializeField]
    float _time = 1.0f;

    [SerializeField]
    Quaternion _rotation;

    Transform _transform;

    public override void Load(Transform transform)
    {
        _transform = transform;
    }

    public RotateAnimation(float time)
    {
        _time = time;
    }
        
    public override IEnumerator Appear()
    {
        _transform.localRotation = Quaternion.identity;

//        var elapsedTime = 0.0f;
        while(true)
        {
//            elapsedTime += Time.deltaTime;
            _transform.Rotate(0f, 0f, -360f * Time.deltaTime); 
//            _transform.localRotation = Quaternion.Lerp(Quaternion.identity, _rotation, (elapsedTime / _time));
            yield return null;
        }
    }

    public override IEnumerator Disappear()
    {
        yield return null;
//        _ctrl.Alpha = _maxAlpha;
//
//        var elapsedTime = 0.0f;
//        while(elapsedTime <= _time)
//        {
//            elapsedTime += Time.deltaTime;
//            _ctrl.Alpha = Mathf.Lerp(_maxAlpha, _minAlpha, (elapsedTime / _time));
//            yield return null;
//        }
    }

    public override void Reset()
    {
//        if(_ctrl != null)
//        {
//            _ctrl.Alpha = _maxAlpha;
//        }
    }

    public override object Clone()
    {
        var anim = new RotateAnimation(_time);
        return anim;
    }
}