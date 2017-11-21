using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class RotateForEverAnimation : UIViewAnimation
{
    Vector3 _eulerRotation;
    bool _keepOriginalRotation;

    Transform _transform;

    public void Load(GameObject gameObject)
    {
        _transform = gameObject.transform;
    }

    public RotateForEverAnimation(Vector3 eulerRotation, bool keepOriginalRotation)
    {
        _eulerRotation = eulerRotation;
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
            _transform.Rotate(_eulerRotation * Time.deltaTime); 
            yield return null;
        }
    }        
}

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimationFactory : UIViewAnimationFactory
{
    public Vector3 _eulerRotation;
    public bool _keepOriginalRotation;

    public override UIViewAnimation Create()
    {
        return new RotateForEverAnimation(_eulerRotation, _keepOriginalRotation);
    }
}
