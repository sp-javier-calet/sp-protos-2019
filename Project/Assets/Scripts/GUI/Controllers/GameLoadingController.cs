
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using Zenject;
using UnityEngine;

public class GameLoadingController : UIViewController
{
    [Inject]
    public ILogin Login;

    [Inject]
    public PopupsController Popups;

    [Inject]
    public IParser<GameModel> GameParser;

    public string SceneToLoad = "Main";

    override protected void OnAppeared()
    {
        base.OnAppeared();

        Login.ErrorEvent += OnLoginError;
        Login.NewUserEvent += OnLoginNewUser;
        Login.Login();
    }

    override protected void OnDisappearing()
    {
        Login.ErrorEvent -= OnLoginError;
        base.OnDisappearing();
    }

    void OnLoginError(ErrorType error, string msg, Attr data)
    {
        var popup = Popups.CreateChild<GameLoadingErrorPopupController>();
        popup.Text = msg;
        Popups.Push(popup);
    }
    
    void OnLoginNewUser(Attr data)
    {
        var model = GameParser.Parse(data);
        ZenUtil.LoadScene(SceneToLoad, (DiContainer container) => {
            container.BindInstance(model);
        });
    }

}