using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace SocialPoint.GUIControl
{
    public class CombinedAnimation : UIViewAnimation
    {
        UIViewAnimation[] _animations;

        public void Load(GameObject gameObject)
        {  
            for(int i = 0; i < _animations.Length; ++i)
            {
                _animations[i].Load(gameObject);
            }
        }

        public CombinedAnimation(UIViewAnimation[] animations)
        {
            _animations = animations;
        }

        public IEnumerator Animate()
        {
            var enums = new List<IEnumerator>();
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
    }

    [CreateAssetMenu(menuName = "UI Animations/Combined Animation")]
    public class CombinedAnimationFactory : UIViewAnimationFactory
    {
        public UIViewAnimation[] Animations;

        public override UIViewAnimation Create()
        {
            if(Animations.Length == 0)
            {
                throw new UnityException("Combined UIViewAnimation needs some simple animations to work properly");
            }

            return new CombinedAnimation(Animations);
        }
    }
}