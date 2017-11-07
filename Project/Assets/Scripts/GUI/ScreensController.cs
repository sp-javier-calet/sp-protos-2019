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

        var appearAnimationFactory = ScriptableObject.CreateInstance<FadeAnimationFactory>();
        appearAnimationFactory.Create(AnimationTime, 0f, 1f);
        AppearAnimationFactory = appearAnimationFactory;

        var disappearAnimationFactory = ScriptableObject.CreateInstance<FadeAnimationFactory>();
        disappearAnimationFactory.Create(AnimationTime, 1f, 0f);
        DisappearAnimationFactory = disappearAnimationFactory;

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