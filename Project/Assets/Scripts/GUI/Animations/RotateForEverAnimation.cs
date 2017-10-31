using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimation : UIViewAnimation
{
    public override IEnumerator Animate()
    {
        _transform.localRotation = Quaternion.identity;

        while(true)
        {
            _transform.Rotate(0f, 0f, -360f * Time.deltaTime); 
            yield return null;
        }
    }
        
    public override object Clone()
    {
        return new RotateForEverAnimation();
    }
}