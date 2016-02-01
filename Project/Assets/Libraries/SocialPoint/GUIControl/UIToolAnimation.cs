using System;
using System.Collections;
using SocialPoint.GUIAnimation;

namespace SocialPoint.GUIControl
{
    public class UIToolAnimation : UIViewAnimation
    {
        string _name;
        SocialPoint.GUIAnimation.Animation _anim;

        public UIToolAnimation(string name)
        {
            _name = name;
        }

        public void Load(UIViewController ctrl)
        {
            _anim = GUIAnimationUtility.GetAnimation(ctrl.gameObject, _name);
        }

        public IEnumerator Appear()
        {
            if(!Revert())
            {
                yield break;
            }
            yield return Play();
        }

        public IEnumerator Disappear()
        {
            if(!Revert())
            {
                yield break;
            }
            _anim.Invert();
            yield return Play();
        }

        IEnumerator Play()
        {
            _anim.Play();
            while(!_anim.HasFinished())
            {
                yield return null;
            }
        }

        public void Reset()
        {
            Revert();
        }

        bool Revert()
        {
            if(_anim == null)
            {
                return false;
            }
            _anim.RevertToOriginal();
            return true;
        }

        public object Clone()
        {
            var clone = new UIToolAnimation(_name);
            clone._anim = _anim;
            return clone;
        }

    }
}