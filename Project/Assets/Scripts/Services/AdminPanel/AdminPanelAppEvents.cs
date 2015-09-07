using Zenject;
using SocialPoint.AppEvents;

public class AdminPanelAppEvents : AdminPanelAppEventsGUI 
{
    [Inject]
    public IAppEvents InjectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }
}
