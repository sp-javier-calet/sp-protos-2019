
using SocialPoint.GUI;
using Zenject;

public class PopupsController : UIStackController
{
    public const float DefaultFadeSpeed = 4.0f;
    [Inject("popup_fade_speed")]
    public float FadeSpeed = DefaultFadeSpeed;

    protected override void OnLoad()
    {
        Animation = new FadeAnimation(FadeSpeed);
        ChildAnimation = new FadeAnimation(FadeSpeed);
        base.OnLoad();
    }

}