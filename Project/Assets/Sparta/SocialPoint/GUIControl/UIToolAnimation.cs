using System;
using System.Collections;
using SocialPoint.GUIAnimation;

namespace SocialPoint.GUIControl
{
    public sealed class UIToolAnimation : UIViewAnimation<UIViewController>
    {
        string _name;
        Animation _anim;

        public UIToolAnimation(string name)
        {
            _name = name;
        }

        public override void Load(UIViewController ctrl)
        {
            _anim = GUIAnimationUtility.GetAnimation(ctrl.gameObject, _name);
        }

        public override IEnumerator Appear()
        {
            if(!Revert())
            {
                yield break;
            }
            yield return Play(false);
        }

        public override IEnumerator Disappear()
        {
            if(!Revert())
            {
                yield break;
            }
            yield return Play(true);
        }

        IEnumerator Play(bool inverted)
        {
            if(inverted != _anim.IsInverted)
            {
                _anim.Invert();
            }
            _anim.Play();
            while(!_anim.HasFinished())
            {
                yield return null;
            }
            _anim.Stop();
        }

        public override void Reset()
        {
            Revert();
        }

        bool Revert()
        {
            if(_anim == null)
            {
                return false;
            }
            _anim.RevertToOriginal(false);
            return true;
        }

        public override object Clone()
        {
            var clone = new UIToolAnimation(_name);
            clone._anim = _anim;
            return clone;
        }

    }
}