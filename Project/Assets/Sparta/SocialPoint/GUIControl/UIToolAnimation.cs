using System;
using System.Collections;
using SocialPoint.GUIAnimation;

namespace SocialPoint.GUIControl
{
    public sealed class UIToolAnimation : UIViewAnimation
    {
        string _name;
        Animation _anim;

        public override float Duration
        {
            get
            {
                if(_anim != null)
                {
                    return _anim.GetEndingTime();
                }

                return 0f;
            }
        }

        public UIToolAnimation(string name)
        {
            _name = name;
        }

        public override void Load(UIViewController ctrl)
        {
            _anim = GUIAnimationUtility.GetAnimation(ctrl.gameObject, _name);
        }

        public override IEnumerator Animate()
        {
            if(!Revert())
            {
                yield break;
            }
            yield return Play(false);
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