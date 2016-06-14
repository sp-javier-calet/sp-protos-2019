using SocialPoint.GUIControl;
using SocialPoint.Dependency;

public class PopupsController : UIStackController
{
    public const float DefaultFadeSpeed = 4.0f;
    float FadeSpeed = DefaultFadeSpeed;

    override protected void OnLoad()
    {
        FadeSpeed = ServiceLocator.Instance.Resolve("popup_fade_speed", DefaultFadeSpeed);
        ChildAnimation = new FadeAnimation(FadeSpeed);
        base.OnLoad();
    }
}