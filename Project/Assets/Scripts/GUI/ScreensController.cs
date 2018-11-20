using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

public class ScreensController : UIStackController
{
    protected override void OnLoad()
    {
        float animationTime = Services.Instance.Resolve<float>("popup_animation_time");
        AppearAnimation = new FadeAnimation(animationTime, 0f, 1f);
        DisappearAnimation = new FadeAnimation(animationTime, 1f, 0f);

        base.OnLoad();
    }

    #if UNITY_EDITOR || UNITY_ANDROID
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            OnSpecialButtonClickedEvent();
        }
    }
    #endif
}