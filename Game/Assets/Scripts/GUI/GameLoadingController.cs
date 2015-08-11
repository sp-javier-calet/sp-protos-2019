
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Utils;
using Zenject;
using UnityEngine;

public class GameLoadingController : UIViewController
{
    [Inject]
    public ILogin Login;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        Login.Login(OnLoggedIn);
    }

    private void OnLoggedIn(Error err)
    {
        Debug.Log("Logged in with error " + err);
    }
}