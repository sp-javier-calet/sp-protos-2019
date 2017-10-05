using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

[CreateAssetMenu(menuName = "UI Animations/Combined Animation")]
public class CombinedAnimation : UIViewAnimation
{
    [SerializeField]
    UIViewAnimation[] _animations;

    UIViewController _ctrl;

    public override float Duration
    {
        get
        {
            float max = 0f;

            if(_animations != null)
            {
                for(int i = 0; i < _animations.Length; ++i)
                {
                    if(_animations[i].Duration > max)
                    {
                        max = _animations[i].Duration;
                    }
                }
            }

            return max;
        }
    }

    public override void Load(UIViewController ctrl)
    {
        _ctrl = ctrl;

        if(_animations.Length == 0)
        {
            throw new MissingComponentException("Combined animation needs some simple animations.");
        }

        for(int i = 0; i < _animations.Length; ++i)
        {
            _animations[i].Load(ctrl);
        }
    }

    public CombinedAnimation(UIViewAnimation[] animations)
    {
        _animations = animations;
    }
        
    public override IEnumerator Animate()
    {
        for(int i = 0; i < _animations.Length; ++i)
        {
            var anim = _animations[i];
            if(anim != null)
            {
                _ctrl.StartCoroutine(anim.Animate());
            }
        }

        yield return new WaitForSeconds(Duration);
    }
         
    public override void Reset() 
    {
        if(_animations != null)
        {
            for(int i = 0; i < _animations.Length; ++i)
            {
                _animations[i].Reset();
            }
        }
    }

    public override object Clone()
    {
        return new CombinedAnimation(_animations);
    }
}