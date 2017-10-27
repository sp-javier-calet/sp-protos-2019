using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "UI Animations/Combined Animation")]
public class CombinedAnimation : UIViewAnimation
{
    [SerializeField]
    UIViewAnimation[] _animations;

    public override void Load(UIViewController ctrl)
    {
        if(_animations.Length == 0)
        {
            throw new MissingComponentException("Combined animation needs some simple animations.");
        }

        if(ctrl == null)
        {
            throw new MissingComponentException("UIViewController does not exist");
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
        List<IEnumerator> enums = new List<IEnumerator>();
        for(int i = 0; i < _animations.Length; ++i)
        {
            var anim = _animations[i];
            if(anim != null)
            {
                enums.Add(_animations[i].Animate());
            }
        }

        while(enums.Count > 0)
        {
            for(int i = enums.Count - 1; i >= 0; --i)
            {
                if(!enums[i].MoveNext())
                {
                    enums.RemoveAt(i);
                }
            }
            yield return null;
        }
    }
        
    public override object Clone()
    {
        UIViewAnimation[] clonedAnimations = new UIViewAnimation[_animations.Length];

        for(int i = 0; i < _animations.Length; ++i)
        {
            var anim = _animations[i];
            if(anim != null)
            {
                clonedAnimations[i] = (UIViewAnimation)_animations[i].Clone();
            }
        }

        return new CombinedAnimation(clonedAnimations);
    }
}