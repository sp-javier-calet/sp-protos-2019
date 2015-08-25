
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.Locale;
using SocialPoint.Base;
using Zenject;
using UnityEngine;

public class GameLoadingController : UIViewController
{
    [Inject]
    public ILogin Login;

    [Inject]
    [HideInInspector]
    public PopupsController Popups;

    [InjectOptional]
    public ICrashReporter CrashReporter;

    [Inject]
    public IParser<GameModel> GameParser;

    [Inject]
    public Localization Localization;

    public GameObject ProgressContainer;

    public string SceneToLoad = "Main";

    override protected void OnLoad()
    {
        base.OnLoad();
                
        if(CrashReporter != null)
        {
            CrashReporter.Enable();
        }
    }

    override protected void OnAppeared()
    {
        base.OnAppeared();

        Login.ErrorEvent += OnLoginError;
        Login.NewUserEvent += OnLoginNewUser;
        DoLogin();
    }

    void DebugLog(string msg)
    {
        Debug.Log(msg);
    }

    void DoLogin()
    {
        if(ProgressContainer != null)
        {
            ProgressContainer.SetActive(true);
        }
        DebugLog(string.Format("Start Login"));
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
        DebugLog(string.Format("Login Error {0} {1} {2}", error, msg, data));
    }

    void OnLoginEnd(Error err)
    {
        if(ProgressContainer != null)
        {
            ProgressContainer.SetActive(false);
        }
        if(!Error.IsNullOrEmpty(err))
        {
            DebugLog(string.Format("Login End Error {0}", err));
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