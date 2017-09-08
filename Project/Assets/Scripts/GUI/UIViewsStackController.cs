using SocialPoint.GUIControl;
using SocialPoint.Dependency;

public class UIViewsStackController : UIStackController
{
    public const float DefaultAnimationTime = 1.0f;
    float AnimationTime = DefaultAnimationTime;

    override protected void OnLoad()
    {
        AnimationTime = Services.Instance.Resolve("popup_animation_time", DefaultAnimationTime);
        ChildAnimation = new FadeAnimation(AnimationTime);
        base.OnLoad();
    }
}