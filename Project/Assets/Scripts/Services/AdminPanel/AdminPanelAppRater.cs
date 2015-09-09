using Zenject;
using SocialPoint.AppRater;

public class AdminPanelAppRater :AdminPanelAppRaterGUI
{
    [Inject]
    public AppRater InjectAppRater
    {
        set
        {
            AppRater = value;
        }
    }
}


