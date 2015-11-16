
using SocialPoint.GameLoading;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Base;
using SocialPoint.Crash;
using SocialPoint.ServerEvents;
using SocialPoint.QualityStats;
using Zenject;
using UnityEngine;

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

    [Inject]
    AdminPanel _adminPanel;

    [Inject]
    GameModel _model;

    [Inject]
    SceneManager _sceneManager;

    #region services that need to be loaded when the game starts

    [Inject]
    ICrashReporter _crashReporter;

    [Inject]
    IEventTracker _eventTracker;

    [Inject]
    QualityStats _qualityStats;

    #endregion

    [SerializeField]
    string _sceneToLoad = "Main";

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

        Login.NewUserEvent += OnLoginNewUser;
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility += OnAdminPanelChange;
        }
    }

    void OnLoadSceneStart()
    {
        _sceneManager.ChangeSceneToAsync(_sceneToLoad, false, (SceneLoadingArgs args) => {
            args.ActivateScene();
            _loadSceneOperation.Finish("main scene loaded");
        });
    }

    void OnAdminPanelChange()
    {
        Paused = _adminPanel.Visible;
    }

    void OnLoginNewUser(Attr data, bool changed)
    {
        _gameLoader.Load(data);
        _loadModelOperation.Finish("game model loaded");
    }

    override protected void OnDisappearing()
    {
        Login.NewUserEvent -= OnLoginNewUser;
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility -= OnAdminPanelChange;
        }
        base.OnDisappearing();
    }

}
