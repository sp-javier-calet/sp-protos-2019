using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

public class ScreensController : UIStackController
{
    public const float DefaultAnimationTime = 1.0f;
    float AnimationTime = DefaultAnimationTime;

    override protected void OnLoad()
    {
        AnimationTime = Services.Instance.Resolve("popup_animation_time", DefaultAnimationTime);
        ChildAnimationPopups = new FadeAnimation(AnimationTime);
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