using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.GameLoading;
using SocialPoint.Login;
using SocialPoint.Locale;
using Zenject;

public class LoadingController : GameLoadingController
{
    [InjectOptional("loading_controller_login")]
    public ILogin InjectLogin
    {
        set
        {
            Login = value;
        }
    }

    [InjectOptional("loading_controller_Popup")]
    public PopupsController InjectPopups
    {
        set
        {
            Popups = value;
        }
    }

    [InjectOptional("loading_controller_Localization")]
    public Localization InjectLocalization
    {
        set
        {
            Localization = value;
        }
    }

    [InjectOptional]
    public ICrashReporter CrashReporter;

    [Inject]
    public IParser<GameModel> GameParser;

    public string SceneToLoad = "Main";


    GameModel _model;
    LoadingOperation _parseModelOperation;
    //LoadingOperation _aditionalFakeOperation;

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
        _parseModelOperation = new LoadingOperation();
        Operations.Add(_parseModelOperation);
        LoadingBar.RegisterLoadingOperation(_parseModelOperation);

        //TODO: delete, only mock pourpose
        /*
        _aditionalFakeOperation = new LoadingOperation();
        Operations.Add(_aditionalFakeOperation);
        LoadingBar.RegisterLoadingOperation(_aditionalFakeOperation);
        StartCoroutine(_aditionalFakeOperation.FakeLoadingProcess(UnityEngine.Random.Range(2, 6)));
        */
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
