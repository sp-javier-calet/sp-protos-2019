using System.Collections;
using SocialPoint.GUIAnimation;
using UnityEngine;

namespace SocialPoint.GUIControl
{
    public sealed class UIToolAnimation : UIViewAnimation
    {
        string _name;
        SocialPoint.GUIAnimation.Animation _anim;

        public UIToolAnimation(string name)
        {
            _name = name;
        }
            
        public void Load(GameObject gameObject)
        {
            _anim = GUIAnimationUtility.GetAnimation(gameObject, _name);
        }

        public IEnumerator Animate()
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
            
        bool Revert()
        {
            if(_anim == null)
            {
                return false;
            }
            _anim.RevertToOriginal(false);
            return true;
        }
    }
}