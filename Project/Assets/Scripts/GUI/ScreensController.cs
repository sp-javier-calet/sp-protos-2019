using SocialPoint.GUIControl;
using SocialPoint.Dependency;
using UnityEngine;

public class ScreensController : UIStackController
{
    const string kShowAnimName = "AnimIn";
    const string kHideAnimName = "AnimOut";
    const float kDefaultAnimationTime = 1.0f;

    override protected void OnLoad()
    {
        float animationTime = Services.Instance.Resolve("popup_animation_time", kDefaultAnimationTime);

        if(Animation == null)
        {
            // Example ScaleAnimation
            AppearAnimation = new ScaleAnimation(animationTime, Vector3.zero, Vector3.one, GoEaseType.QuadIn);
            DisappearAnimation = new ScaleAnimation(animationTime, Vector3.one, Vector3.zero, GoEaseType.QuadIn);

            // Example SlideAnimation
//            AppearAnimation = new SlideAnimation(animationTime, SlideAnimation.PosType.Left, SlideAnimation.PosType.Center, GoEaseType.QuadIn);
//            DisappearAnimation = new SlideAnimation(animationTime, SlideAnimation.PosType.Center, SlideAnimation.PosType.Down, GoEaseType.QuadIn);

            // Example FadeAnimation
//            AppearAnimation = new FadeAnimation(animationTime, 0f, 1f);
//            DisappearAnimation = new FadeAnimation(animationTime, 1f, 0f);

            // Example CombinedAnimation
//            UIViewAnimation[] appearAnimations = new UIViewAnimation[2];
//            appearAnimations[0] = new SlideAnimation(animationTime, SlideAnimation.PosType.Left, SlideAnimation.PosType.Center, GoEaseType.QuadIn);
//            appearAnimations[1] = new FadeAnimation(animationTime, 0f, 1f);
//
//            UIViewAnimation[] disappearAnimations = new UIViewAnimation[2];
//            disappearAnimations[0] = new ScaleAnimation(animationTime, Vector3.one, Vector3.zero, GoEaseType.QuadIn);
//            disappearAnimations[1] = new FadeAnimation(animationTime, 1f, 0f);
//
//            AppearAnimation = new CombinedAnimation(appearAnimations);
//            DisappearAnimation = new CombinedAnimation(disappearAnimations);

            // Example UnityAnimation
//            AppearAnimation = new UnityLegacyAnimation(kShowAnimName);
//
//            UIViewAnimation[] disappearAnimations = new UIViewAnimation[2];
//            disappearAnimations[0] = new SlideAnimation(animationTime, SlideAnimation.PosType.Center, SlideAnimation.PosType.Top, GoEaseType.QuadIn);
//            disappearAnimations[1] = new FadeAnimation(animationTime, 1f, 0f);
//
//            DisappearAnimation = new CombinedAnimation(disappearAnimations);

//            DisappearAnimation =  new UnityLegacyAnimation(kHideAnimName);
        }

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