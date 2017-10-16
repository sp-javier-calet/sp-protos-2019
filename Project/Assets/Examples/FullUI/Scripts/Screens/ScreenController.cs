using SocialPoint.GUIControl;

public class ScreenController : UIViewController 
{
    public ScrollViewExample ScrollView;

    public ScreenController()
    {
        IsFullScreen = true;
    }

    void Start()
    {
        if(ScrollView != null)
        {
            ScrollView.Init();
        }
    }
}