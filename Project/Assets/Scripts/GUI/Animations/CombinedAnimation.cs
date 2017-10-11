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
            enums.Add(_animations[i].Animate());
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