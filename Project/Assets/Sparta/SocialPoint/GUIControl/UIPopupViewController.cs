using SocialPoint.Dependency;

namespace SocialPoint.GUIControl
{
    public class UIPopupViewController : UIViewController
    {
        public const float DefaultAnimationTime = 1.0f;
        float AnimationTime = DefaultAnimationTime;

        public override ViewCtrlType ViewType
        {
            get
            {
                return ViewCtrlType.Popup;
            }
        }

        public override bool IsAnimated
        {
            get
            {
                return true;
            }
        }

        override protected void OnAwake()
        {
            AnimationTime = Services.Instance.Resolve("popup_animation_time", DefaultAnimationTime);
            Animation = new FadeAnimation(AnimationTime);

            base.OnAwake();
        }
    }
}
