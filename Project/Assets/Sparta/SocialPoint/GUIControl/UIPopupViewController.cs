namespace SocialPoint.GUIControl
{
    public class UIPopupViewController : UIViewController
    {
        public override ViewCtrlType ViewType
        {
            get
            {
                return ViewCtrlType.Popup;
            }
        }

        public override bool AnimateShowHide
        {
            get
            {
                return true;
            }
        }
    }
}
