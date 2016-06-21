using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Utils;
using UnityEngine;

public class GameLoadingController : SocialPoint.GameLoading.GameLoadingController
{
    IGameLoader _gameLoader;
    AdminPanel _adminPanel = null;
    ICoroutineRunner _coroutineRunner;

    [SerializeField]
    string _sceneToLoad = "Main";

    public string SceneToLoad { set { _sceneToLoad = value; } }

    LoadingOperation _loadModelOperation;
    LoadingOperation _loadSceneOperation;

    const float ExpectedLoadModelDuration = 1.0f;
    const float ExpectedLoadSceneDuration = 2.0f;

    protected override void OnLoad()
    {
        Login = ServiceLocator.Instance.Resolve<ILogin>();
        CrashReporter = ServiceLocator.Instance.Resolve<ICrashReporter>();
        Localization = ServiceLocator.Instance.Resolve<Localization>();
        AppEvents = ServiceLocator.Instance.Resolve<IAppEvents>();
        ErrorHandler = ServiceLocator.Instance.Resolve<IGameErrorHandler>();
        _coroutineRunner = ServiceLocator.Instance.Resolve<ICoroutineRunner>();
        _gameLoader = ServiceLocator.Instance.Resolve<IGameLoader>();
        #if ADMIN_PANEL
        _adminPanel = ServiceLocator.Instance.Resolve<AdminPanel>();
        #else
        _adminPanel = null;
        #endif
        base.OnLoad();
    }

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _loadModelOperation = new LoadingOperation(ExpectedLoadModelDuration);
        _loadModelOperation.Message = "loading game model...";
        RegisterOperation(_loadModelOperation);
        _loadSceneOperation = new LoadingOperation(ExpectedLoadSceneDuration, OnLoadSceneStart);
        _loadSceneOperation.Message = "loading main scene...";
        RegisterOperation(_loadSceneOperation);

        Login.NewUserStreamEvent += OnLoginNewUser;
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility += OnAdminPanelChange;
        }
    }

    void OnLoadSceneStart()
    {
        _coroutineRunner.LoadSceneAsyncProgress(_sceneToLoad, op => {
            _loadSceneOperation.Progress = op.progress;
            if(op.isDone)
            {
                Hide();
                op.allowSceneActivation = true;
                _loadSceneOperation.Finish("main scene loaded");
            }
        });
    }

    void OnAdminPanelChange()
    {
        if(_adminPanel != null)
        {
            Paused = _adminPanel.Visible;
        }
    }

    bool OnLoginNewUser(IStreamReader reader)
    {
        var data = reader.ParseElement();
        _gameLoader.Load(data);
        _loadModelOperation.Finish("game model loaded");
        return true;
    }

    override protected void OnDisappearing()
    {
        Login.NewUserStreamEvent -= OnLoginNewUser;
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility -= OnAdminPanelChange;
        }
        base.OnDisappearing();
    }

}
