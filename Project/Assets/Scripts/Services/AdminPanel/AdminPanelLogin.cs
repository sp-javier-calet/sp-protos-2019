using Zenject;
using SocialPoint.Login;

public class AdminPanelLogin : AdminPanelLoginGUI {

    [Inject]
    public ILogin InjectLogin
    {
        set
        {
            Login = value;
        }
    }
}
