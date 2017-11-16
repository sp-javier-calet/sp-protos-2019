using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class RotateForEverAnimation : UIViewAnimation
{
    bool _keepOriginalRotation;
    float _angleX;
    float _angleY;
    float _angleZ;

    Transform _transform;

    public void Load(GameObject gameObject)
    {
        _transform = gameObject.transform;
    }

    public RotateForEverAnimation(float angleX, float angleY, float angleZ, bool keepOriginalRotation)
    {
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
            _transform.Rotate(_angleX  * Time.deltaTime, _angleY  * Time.deltaTime, _angleZ  * Time.deltaTime); 
            yield return null;
        }
    }        
}

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimationFactory : UIViewAnimationFactory
{
    public bool _keepOriginalRotation;
    public float _angleX;
    public float _angleY;
    public float _angleZ;

    public override UIViewAnimation Create()
    {
        return new RotateForEverAnimation(_angleX, _angleY, _angleZ, _keepOriginalRotation);
    }
}

