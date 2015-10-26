
using SocialPoint.GameLoading;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
using SocialPoint.Base;
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
    GameLoader _gameLoader;

    [Inject]
    AdminPanel _adminPanel;

    [Inject]
    GameModel _model;

    [Inject]
    DiContainer _container;

    [Inject]
    SceneManager _sceneManager;

    [SerializeField]
    string _sceneToLoad = "Main";

    LoadingOperation _loadModelOperation;
    LoadingOperation _loadSceneOperation;
    SceneLoadingArgs _sceneLoadingArgs;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _loadModelOperation = new LoadingOperation(1.0f);
        _loadSceneOperation = new LoadingOperation(1.0f);
        RegisterOperation(_loadModelOperation);
        RegisterOperation(_loadSceneOperation);

        Login.NewUserEvent += OnLoginNewUser;
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility += OnAdminPanelChange;
        }
    }

    void OnAdminPanelChange()
    {
        Paused = _adminPanel.Visible;
    }

    void OnLoginNewUser(Attr data, bool changed)
    {
        _loadModelOperation.Message = "loading game model...";
        _gameLoader.Load(data);
        _loadModelOperation.Finish("game model loaded");

        _loadSceneOperation.Message = "loading main scene...";
        _sceneManager.ChangeSceneToAsync(_sceneToLoad, false, (SceneLoadingArgs obj) => {
            _sceneLoadingArgs = obj;
            _loadSceneOperation.Finish("main scene loaded");
        });
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

    override protected void OnAllOperationsLoaded()
    {
        base.OnAllOperationsLoaded();
        DebugUtils.Assert(_sceneLoadingArgs != null, "Real scene load not started");
        _sceneLoadingArgs.ActivateScene();
    }

}
