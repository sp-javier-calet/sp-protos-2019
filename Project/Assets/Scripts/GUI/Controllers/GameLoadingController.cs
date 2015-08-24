using System.Collections;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.GUI;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Locale;
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
    public LoadingBarController LoadingBar;

    public string SceneToLoad = "Main";

    GameModel _model;

    List<LoadingOperation> Operations = new List<LoadingOperation>();
    LoadingOperation _loginOperation;
    LoadingOperation _parseModelOperation;
    LoadingOperation _aditionalFakeOperation;

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
        _loginOperation = new LoadingOperation();
        _parseModelOperation = new LoadingOperation();

        //TODO: delete, only mock pourpose
        _aditionalFakeOperation = new LoadingOperation();
        Operations = new List<LoadingOperation>();
        Operations.Add(_loginOperation);
        Operations.Add(_parseModelOperation);
        Operations.Add(_aditionalFakeOperation);
        LoadingBar.RegisterLoadingOperation(Operations);

        //TODO: delete, only mock pourpose
        StartCoroutine(_aditionalFakeOperation.FakeLoadingProcess(UnityEngine.Random.Range(2, 6)));


        StartCoroutine(CheckAllOperationsLoaded());

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
        _loginOperation.UpdateProgress(0.1f, "Logging into servers");
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
        _loginOperation.FinishProgress("login ready");
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
        _parseModelOperation.UpdateProgress(0.1f, "parsing game model");
        _model = GameParser.Parse(data);
        _parseModelOperation.FinishProgress("game model parsed");
    }

    IEnumerator CheckAllOperationsLoaded()
    {
        while(Operations.Exists(o => o.Progress < 1))
        {
            yield return null;
        }
        OnAllOperationsLoaded();
    }

    void OnAllOperationsLoaded()
    {
        ZenUtil.LoadScene(SceneToLoad, (DiContainer container) => container.BindInstance(_model));
    }

}
