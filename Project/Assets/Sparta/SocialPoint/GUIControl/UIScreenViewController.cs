namespace SocialPoint.GUIControl
{
    public class UIScreenViewController : UIViewController
    {
        public override ViewCtrlType ViewType
        {
            get
            {
                return ViewCtrlType.Screen;
            }
        }

        public override bool AnimateShowHide
        {
            get
            {
                return false;
            }
        }
    }
}
