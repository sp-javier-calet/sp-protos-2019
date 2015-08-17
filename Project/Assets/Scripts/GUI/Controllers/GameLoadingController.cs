
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using Zenject;
using UnityEngine;

public class GameLoadingController : UIViewController
{
    [Inject]
    [HideInInspector]
    public ILogin Login;

    [Inject]
    [HideInInspector]
    public PopupsController Popups;

    [Inject]
    [HideInInspector]
    public ICrashReporter CrashReporter;

    [Inject]
    [HideInInspector]
    public IParser<GameModel> GameParser;

    public GameObject ProgressContainer;

    public string SceneToLoad = "Main";

    override protected void OnAppeared()
    {
        base.OnAppeared();

        CrashReporter.Enable();

        Login.ErrorEvent += OnLoginError;
        Login.NewUserEvent += OnLoginNewUser;
        DoLogin();
    }

    void DoLogin()
    {
        if(ProgressContainer != null)
        {
            ProgressContainer.SetActive(true);
        }
        Login.Login(OnLoginEnd);
    }

    override protected void OnDisappearing()
    {
        Login.ErrorEvent -= OnLoginError;
        Login.NewUserEvent -= OnLoginNewUser;
        base.OnDisappearing();
    }

    void OnLoginError(ErrorType error, string msg, Attr data)
    {
        Debug.Log(string.Format("Login Error {0} {1} {2}", error, msg, data));
    }

    void OnLoginEnd(Error err)
    {
        if(ProgressContainer != null)
        {
            ProgressContainer.SetActive(false);
        }
        if(!Error.IsNullOrEmpty(err))
        {
            Debug.Log(string.Format("Login End Error {0}", err));
            var popup = Popups.CreateChild<GameLoadingErrorPopupController>();
            popup.Text = err.Msg;
            popup.Dismissed += OnErrorPopupDismissed;
            Popups.Push(popup);
        }
    }

    void OnErrorPopupDismissed()
    {
        DoLogin();
    }
    
    void OnLoginNewUser(Attr data)
    {
        var model = GameParser.Parse(data);
        ZenUtil.LoadScene(SceneToLoad, (DiContainer container) => {
            container.BindInstance(model);
        });
    }

}