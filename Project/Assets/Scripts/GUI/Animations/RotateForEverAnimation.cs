using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Rotate For Ever Animation")]
public class RotateForEverAnimation : UIViewAnimation
{
    public override void Load(GameObject gameObject = null)
    {
        base.Load(gameObject);

        // HINT: If we want to rotate a gameobject with Canvas component on it, we force to rotate his first child instead
        var canvas = _gameObject.GetComponent<Canvas>();
        if(canvas != null)
        {
            if(_transform.childCount > 0)
            {
                _transform = _transform.GetChild(0);
                _gameObject = _transform.gameObject;
            }
        }

        _rectTransform = _transform as RectTransform;
    }

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