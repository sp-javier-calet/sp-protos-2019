
using SocialPoint.GameLoading;
using SocialPoint.Attributes;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Alert;
using SocialPoint.Events;
using SocialPoint.AppEvents;
using SocialPoint.AdminPanel;
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
    IParser<GameModel> _gameParser;

    [Inject]
    AdminPanel _adminPanel;

    [Inject]
    GameModel _model;

    [SerializeField]
    string _sceneToLoad = "Main";

    LoadingOperation _parseModelOperation;

    override protected void OnAppeared()
    {
        base.OnAppeared();
        _parseModelOperation = new LoadingOperation();
        RegisterLoadingOperation(_parseModelOperation);

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
        _parseModelOperation.UpdateProgress(0.1f, "parsing game model");
        var newModel = _gameParser.Parse(data);
        _model.Assign(newModel);
        _parseModelOperation.FinishProgress("game model parsed");
    }

    protected override void OnAllOperationsLoaded()
    {
        base.OnAllOperationsLoaded();
        ZenUtil.LoadScene(_sceneToLoad, BeforeSceneLoaded, AfterSceneLoaded);
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
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility -= OnAdminPanelChange;
        }
        base.OnDisappearing();
    }

}
