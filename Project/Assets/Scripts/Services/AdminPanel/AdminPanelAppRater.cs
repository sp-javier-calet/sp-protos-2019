using Zenject;
using SocialPoint.Rating;

public class AdminPanelAppRater : AdminPanelAppRaterGUI
{
    [Inject]
    public IAppRater InjectAppRater
    {
        set
        {
            AppRater = value;
        }
    }
}


