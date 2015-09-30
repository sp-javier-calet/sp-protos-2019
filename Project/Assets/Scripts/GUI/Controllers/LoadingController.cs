
using SocialPoint.GameLoading;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Alert;
using SocialPoint.Events;
using SocialPoint.AppEvents;
using Zenject;

public class LoadingController : GameLoadingController
{
    [Inject]
    ILogin injectLogin
    {
        set
        {
            Login = value;
        }
    }

    [Inject]
    PopupsController injectPopups
    {
        set
        {
            Popups = value;
        }
    }

    [Inject]
    Localization injectLocalization
    {
        set
        {
            Localization = value;
        }
    }

    [Inject]
    IAlertView injectAlertView
    {
        set
        {
            AlertView = value;
        }
    }

    [Inject]
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    [Inject]
    ILocalizationManager injectLocalizationManager
    {
        set
        {
            LocalizationManager = value;
        }
    }

    [Inject]
    IParser<GameModel> _gameParser;

    public string SceneToLoad = "Main";

    GameModel _model;
    LoadingOperation _parseModelOperation;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _parseModelOperation = new LoadingOperation();
        RegisterLoadingOperation(_parseModelOperation);

        Login.NewUserEvent += OnLoginNewUser;
    }

    void OnLoginNewUser(Attr data, bool changed)
    {
        _parseModelOperation.UpdateProgress(0.1f, "parsing game model");
        _model = _gameParser.Parse(data);
        _parseModelOperation.FinishProgress("game model parsed");
    }

    protected override void AllOperationsLoaded()
    {
        base.AllOperationsLoaded();
        ZenUtil.LoadScene(SceneToLoad, BeforeSceneLoaded, AfterSceneLoaded);
    }

    void BeforeSceneLoaded(DiContainer container)
    {
        container.BindInstance(_model);
    }

    void AfterSceneLoaded(DiContainer container)
    {
    }

    override protected void OnDisappearing()
    {
        Login.NewUserEvent -= OnLoginNewUser;
        base.OnDisappearing();
    }

}
