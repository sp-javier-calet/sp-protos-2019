using SocialPoint.AdminPanel;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Utils;
using SocialPoint.Social;
using UnityEngine;

public class GameLoadingController : SocialPoint.GameLoading.GameLoadingController
{
    IGameLoader _gameLoader;
    ICoroutineRunner _coroutineRunner;

    // Explicit assignation to avoid warnings when ADMIN_PANEL is not enabled
    AdminPanel _adminPanel = null;

    [SerializeField]
    string _sceneToLoad = "Main";

    public string SceneToLoad { set { _sceneToLoad = value; } }

    LoadingOperation _loadModelOperation;
    LoadingOperation _loadSceneOperation;

    const float ExpectedLoadModelDuration = 1.0f;
    const float ExpectedLoadSceneDuration = 2.0f;

    AlliancesManager _alliances;

    protected override void OnLoad()
    {
        Login = Services.Instance.Resolve<ILogin>();
        CrashReporter = Services.Instance.Resolve<ICrashReporter>();
        Localization = Services.Instance.Resolve<Localization>();
        AppEvents = Services.Instance.Resolve<IAppEvents>();
        ErrorHandler = Services.Instance.Resolve<IGameErrorHandler>();
        _alliances = Services.Instance.Resolve<AlliancesManager>();
        _coroutineRunner = Services.Instance.Resolve<ICoroutineRunner>();
        _gameLoader = Services.Instance.Resolve<IGameLoader>();

        #if ADMIN_PANEL
        _adminPanel = Services.Instance.Resolve<AdminPanel>();
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
        Login.ConfirmLinkEvent += OnConfirmLinkEvent;
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

        if(_alliances != null)
        {
            _alliances.ParseAllianceInfo(data.AsDic.Get("alliance").AsDic);
        }

        _gameLoader.Load(data);
        _loadModelOperation.Finish("game model loaded");
        return true;
    }

    void OnConfirmLinkEvent(ILink link, LinkConfirmType type, Attr data, ConfirmBackLinkDelegate cbk)
    {
        ErrorHandler.ShowLink(link, type, data, cbk); 
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
