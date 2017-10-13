using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

/// <summary>
/// This class can execute Unity Animations without Animator.
/// Remember to check the animation as legacy (Animation -> Inspector -> Debug Mode -> Legacy)
/// </summary>
[CreateAssetMenu(menuName = "UI Animations/Unity Animation")]
public class UnityLegacyAnimation : UIViewAnimation 
{
    [SerializeField]
    Animation _animation;

    [SerializeField]
    float _speed = 1.0f;

    [SerializeField]
    string _animName = string.Empty;

    UIViewController _ctrl;

    public override void Load(UIViewController ctrl)
    {
        if(ctrl == null)
        {
            throw new MissingComponentException("UIViewController does not exist");
        }

        _ctrl = ctrl;

        if(_animation == null)        
        {
            _animation = _ctrl.GetComponent<Animation>();
            if(_animation == null)
            {
                throw new MissingComponentException("Could not find Animation component.");
            }
        }
    }
        
    public UnityLegacyAnimation(string animName, float speed = 1f, Animation animation = null)
    {
        _speed = speed;
        _animation = animation;
        _animName = animName;
    }
        
    public override IEnumerator Animate()
    {
        if(_animation != null && !string.IsNullOrEmpty(_animName))
        {
            _animation[_animName].speed = _speed;
            _animation.Play(_animName);

            while(_animation.IsPlaying(_animName))
            {
                yield return null;
            }

            _animation.Stop(_animName);
        }
    }

    public override object Clone()
    {
        return new UnityLegacyAnimation(_animName, _speed, _animation);
    }
}