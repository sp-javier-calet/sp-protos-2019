using SocialPoint.AppEvents;
using SocialPoint.AssetBundlesClient;
using SocialPoint.Attributes;
using SocialPoint.Crash;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Social;
using SocialPoint.Utils;
using UnityEngine;

#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

public class GameLoadingController : SocialPoint.GameLoading.GameLoadingController
{
    IGameLoader _gameLoader;
    ICoroutineRunner _coroutineRunner;

    #if ADMIN_PANEL
    // Explicit assignation to avoid warnings when ADMIN_PANEL is not enabled
    AdminPanel _adminPanel = null;
    #endif

    [SerializeField]
    string _sceneToLoad = "MainScene";

    LoadingOperation _loadModelOperation;
    LoadingOperation _loadSceneOperation;

    const float ExpectedLoadModelDuration = 1.0f;
    const float ExpectedLoadSceneDuration = 2.0f;

    AssetBundleManager _assetBundleManager;
    SocialManager _socialManager;

    protected override void OnLoad()
    {
        Application.targetFrameRate = 30;

        LoginService = Services.Instance.Resolve<ILoginService>();
        LinkingService = Services.Instance.Resolve<ILinkingService>();
        CrashReporter = Services.Instance.Resolve<ICrashReporter>();
        Localization = Services.Instance.Resolve<Localization>();
        AppEvents = Services.Instance.Resolve<IAppEvents>();
        NativeUtils = Services.Instance.Resolve<INativeUtils>();
        ErrorHandler = Services.Instance.Resolve<IGameErrorHandler>();
        _assetBundleManager = Services.Instance.Resolve<AssetBundleManager>();
        _socialManager = Services.Instance.Resolve<SocialManager>();
        _coroutineRunner = Services.Instance.Resolve<ICoroutineRunner>();
        _gameLoader = Services.Instance.Resolve<IGameLoader>();

        #if ADMIN_PANEL
        _adminPanel = Services.Instance.Resolve<AdminPanel>();
        #endif

        base.OnLoad();
    }

    public override bool OnBeforeClose()
    {
        return false;
    }

    protected override void OnAppeared()
    {
        base.OnAppeared();
        _loadModelOperation = new LoadingOperation(ExpectedLoadModelDuration);
        _loadModelOperation.Message = "loading game model...";
        RegisterOperation(_loadModelOperation);
        _loadSceneOperation = new LoadingOperation(ExpectedLoadSceneDuration, OnLoadSceneStart);
        _loadSceneOperation.Message = "loading main scene...";
        RegisterOperation(_loadSceneOperation);

        LoginService.NewGameDataStreamEvent += OnLoginNewGameData;
        LinkingService.ConfirmLinkEvent += OnConfirmLinkEvent;
        #if ADMIN_PANEL
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility += OnAdminPanelChange;
        }
        #endif
    }

    void OnLoadSceneStart()
    {
        if(!string.IsNullOrEmpty(_sceneToLoad))
        {
            _coroutineRunner.LoadSceneAsyncProgress(_sceneToLoad, op => {
                _loadSceneOperation.Progress = op.progress;
                if(op.isDone)
                {
                    HideImmediate(true);
                    op.allowSceneActivation = true;
                    _loadSceneOperation.Finish("main scene loaded");
                }
            });
        }
    }

    #if ADMIN_PANEL
    void OnAdminPanelChange()
    {
        if(_adminPanel != null)
        {
            Paused = _adminPanel.Visible;
        }
    }
    #endif

    bool OnLoginNewGameData(IStreamReader reader)
    {
        var data = reader.ParseElement();
        _gameLoader.Load(data);

        if(data != null)
        {
            ParseSFLocalPlayerData(data);
            ParseAssetBundlesData(data);
        }

        _loadModelOperation.Finish("game model loaded");
        return true;
    }

    void ParseSFLocalPlayerData(Attr data)
    {
        if(_socialManager != null)
        {
            _socialManager.SetLocalPlayerData(data.AsDic, Services.Instance.Resolve<IPlayerData>());
        }
    }

    void ParseAssetBundlesData(Attr data)
    {
        if(_assetBundleManager != null)
        {
            _assetBundleManager.Init(data);
        }
    }

    void OnConfirmLinkEvent(ILink link, LinkConfirmType type, Attr data, ConfirmBackLinkDelegate cbk)
    {
        ErrorHandler.ShowLink(link, type, data, cbk);
    }

    protected override void OnDisappearing()
    {
        LoginService.NewGameDataStreamEvent -= OnLoginNewGameData;
        LinkingService.ConfirmLinkEvent -= OnConfirmLinkEvent;
        #if ADMIN_PANEL
        if(_adminPanel != null)
        {
            _adminPanel.ChangedVisibility -= OnAdminPanelChange;
        }
        #endif
        base.OnDisappearing();
    }

}
