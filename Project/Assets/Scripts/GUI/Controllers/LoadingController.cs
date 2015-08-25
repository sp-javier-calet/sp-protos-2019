using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.GameLoading;
using SocialPoint.Login;
using SocialPoint.Locale;
using Zenject;

public class LoadingController : GameLoadingController
{
    [Inject]
    public ILogin InjectLogin
    {
        set
        {
            Login = value;
        }
    }

    [Inject]
    public PopupsController InjectPopups
    {
        set
        {
            Popups = value;
        }
    }

    [Inject]
    public Localization InjectLocalization
    {
        set
        {
            Localization = value;
        }
    }

    [InjectOptional]
    public ICrashReporter InjectCrashReporter
    {
        set
        {
            CrashReporter = value;
        }
    }

    [Inject]
    public IParser<GameModel> GameParser;

    public string SceneToLoad = "Main";

    GameModel _model;
    LoadingOperation _parseModelOperation;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _parseModelOperation = new LoadingOperation();
        RegisterLoadingOperation(_parseModelOperation);

        Login.NewUserEvent += OnLoginNewUser;

        AllOperationsLoaded += OnAllOperationsLoaded;
    }

    void OnLoginNewUser(Attr data)
    {
        _parseModelOperation.UpdateProgress(0.1f, "parsing game model");
        _model = GameParser.Parse(data);
        _parseModelOperation.FinishProgress("game model parsed");
    }

    void OnAllOperationsLoaded()
    {
        ZenUtil.LoadScene(SceneToLoad, (DiContainer container) => container.BindInstance(_model));
    }

    override protected void OnDisappearing()
    {
        Login.NewUserEvent -= OnLoginNewUser;
        base.OnDisappearing();
    }

}
