using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.GameLoading;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.ServerEvents;
using SocialPoint.Utils;
using SocialPoint.Base;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

public class GameLoadingController : SocialPoint.GameLoading.GameLoadingController
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
    ICrashReporter injectCrashReporter
    {
        set
        {
            CrashReporter = value;
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
    IAppEvents injectAppEvents
    {
        set
        {
            AppEvents = value;
        }
    }

    [Inject]
    IGameErrorHandler injectGameErrorHandler
    {
        set
        {
            ErrorHandler = value;
        }
    }

    [Inject]
    IGameLoader _gameLoader;

    #if ADMIN_PANEL
    [Inject]
    #endif
    AdminPanel _adminPanel = null;

    [Inject]
    GameModel _model;

    #region services that need to be loaded when the game starts

    [Inject]
    ICrashReporter _crashReporter;

    [Inject]
    IEventTracker _eventTracker;

    [InjectOptional]
    QualityStats _qualityStats;

    #endregion

    [SerializeField]
    string _sceneToLoad = "Main";

    public string SceneToLoad { set { _sceneToLoad = value; } }

    LoadingOperation _loadModelOperation;
    LoadingOperation _loadSceneOperation;

    const float ExpectedLoadModelDuration = 1.0f;
    const float ExpectedLoadSceneDuration = 2.0f;

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
        this.LoadSceneAsync(_sceneToLoad, op => {
            op.allowSceneActivation = true;
            _loadSceneOperation.Finish("main scene loaded");
        });
    }

    void OnAdminPanelChange()
    {
        Paused = _adminPanel.Visible;
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
