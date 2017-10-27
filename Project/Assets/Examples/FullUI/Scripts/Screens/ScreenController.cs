using SocialPoint.GUIControl;

public class ScreenController : UIViewController
{
    public ScrollViewExample ScrollView;

    public ScreenController()
    {
        IsFullScreen = true;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if(ScrollView != null)
        {
            ScrollView.Init();
        }
    }
}