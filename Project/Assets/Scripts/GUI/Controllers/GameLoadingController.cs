
using SocialPoint.GameLoading;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
using SocialPoint.Utils;
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

    LoadingOperation _parseModelOperation;
    LoadingOperation _loadSceneOperation;
    SceneLoadingArgs _sceneLoadingArgs;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _parseModelOperation = new LoadingOperation(5);
        _loadSceneOperation = new LoadingOperation(5);
        RegisterLoadingOperation(_parseModelOperation);
        RegisterLoadingOperation(_loadSceneOperation);

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
        _parseModelOperation.UpdateProgress(0.1f, "loaded game model");
        _gameLoader.Load(data);
        _parseModelOperation.FinishProgress("game model loaded");

        _sceneManager.ChangeSceneToAsync(_sceneToLoad, false, (SceneLoadingArgs obj) => {
            _sceneLoadingArgs = obj;
            _loadSceneOperation.FinishProgress();
            UnityEngine.Debug.Log(Time.time);
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

        UnityEngine.Debug.Log("allowSceneActivation");
        UnityEngine.Debug.Log(Time.time);
        _sceneLoadingArgs.ActivateScene();
    }

}
