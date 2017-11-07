using UnityEngine;
using System.Collections;
using SocialPoint.GUIControl;

/// <summary>
/// This class can execute Unity Animations without Animator.
/// Remember to check the animation as legacy (Animation -> Inspector -> Debug Mode -> Legacy)
/// </summary>
public class UnityLegacyAnimation : UIViewAnimation 
{
    Animation _animation;
    float _animSpeed;
    string _animName = string.Empty;

    public void Load(GameObject gameObject)
    {
        _animation = gameObject.GetComponent<Animation>();
        if(_animation == null)
        {
            throw new MissingComponentException("Missing Animation component in UIViewAnimation Load");
        }
    }
        
    public UnityLegacyAnimation(Animation animation, string animName, float animSpeed)
    {
        _animation = animation;
        _animName = animName;
        _animSpeed = animSpeed;
    }
        
    public IEnumerator Animate()
    {
        if(_animation != null && !string.IsNullOrEmpty(_animName))
        {
            _animation[_animName].speed = _animSpeed;
            _animation.Play(_animName);

            while(_animation.IsPlaying(_animName))
            {
                yield return null;
            }

            _animation.Stop(_animName);
        }
    }
}

[CreateAssetMenu(menuName = "UI Animations/Unity Animation")]
public class UnityLegacyAnimationFactory : UIViewAnimationFactory
{    
    public Animation Animation;
    public string AnimName = string.Empty;
    public float AmimSpeed;

    public override UIViewAnimation Create()
    {
        return new UnityLegacyAnimation(Animation, AnimName, AmimSpeed);
    }

    public UIViewAnimation Create(Animation animation, string animName, float animSpeed = 1f)
    {
        Animation = animation;
        AnimName = animName;
        AmimSpeed = animSpeed;

        return Create();
    }
}