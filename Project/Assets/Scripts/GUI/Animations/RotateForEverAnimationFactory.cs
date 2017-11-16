using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

public class RotateForEverAnimation : UIViewAnimation
{
    Transform _transform;

    public void Load(GameObject gameObject)
    {
        _transform = gameObject.transform;
    }

    public IEnumerator Animate()
    {
        _transform.localRotation = Quaternion.identity;

        while(true)
        {
            _transform.Rotate(0f, 0f, -360f * Time.deltaTime); 
            yield return null;
        }
    }        
}

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimationFactory : UIViewAnimationFactory
{
    public override UIViewAnimation Create()
    {
        return new RotateForEverAnimation();
    }
}

