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

        public override bool IsAnimated
        {
            get
            {
                return false;
            }
        }
    }
}
