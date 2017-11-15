using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class RotateForEverAnimation : UIViewAnimation
{
    bool _animateX;
    bool _animateY;
    bool _animateZ;
    bool _keepOriginalRotation;
    float _angleX;
    float _angleY;
    float _angleZ;

    Transform _transform;

    public void Load(GameObject gameObject)
    {
        _transform = gameObject.transform;
    }

    public RotateForEverAnimation(bool animateX, float angleX, bool animateY, float angleY, bool animateZ, float angleZ, bool keepOriginalRotation)
    {
        _animateX = animateX;
        _animateY = animateY;
        _animateZ = animateZ;
        _angleX = angleX;
        _angleY = angleY;
        _angleZ = angleZ;
        _keepOriginalRotation = keepOriginalRotation;
    }

    public IEnumerator Animate()
    {
        if(!_keepOriginalRotation)
        {
            _transform.localRotation = Quaternion.identity;
        }

        while(true)
        {
            _transform.Rotate((_animateX ? _angleX  * Time.deltaTime : 0f), (_animateY ? _angleY  * Time.deltaTime : 0f), (_animateZ ? _angleZ  * Time.deltaTime : 0f)); 
            yield return null;
        }
    }        
}

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimationFactory : UIViewAnimationFactory
{
    public bool _animateX;
    public bool _animateY;
    public bool _animateZ;
    public bool _keepOriginalRotation;
    public float _angleX;
    public float _angleY;
    public float _angleZ;

    public override UIViewAnimation Create()
    {
        return new RotateForEverAnimation(_animateX, _angleX, _animateY, _angleY, _animateZ, _angleZ, _keepOriginalRotation);
    }
}

