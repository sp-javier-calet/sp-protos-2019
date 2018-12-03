using System;
using SocialPoint.Alert;
using SocialPoint.AppEvents;
using SocialPoint.Attributes;
using SocialPoint.Dependency;
using SocialPoint.GameLoading;
using SocialPoint.Helpshift;
using SocialPoint.Locale;
using SocialPoint.Login;
using SocialPoint.Restart;
using SocialPoint.ServerSync;
using SocialPoint.Social;
using SocialPoint.Tutorial;
using SocialPoint.Utils;
#if ADMIN_PANEL
using SocialPoint.AdminPanel;
#endif

public class GameInstaller : Installer, IInitializable
{
    [Serializable]
    public class SettingsData
    {
        public string InitialJsonGameResource = "game";
        public string InitialJsonPlayerResource = "user";
        public bool EditorDebug = true;
        public bool LoadLocalJson;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IInitializable>().ToInstance(this);

#if UNITY_EDITOR
        container.Bind<bool>("game_debug").ToInstance(Settings.EditorDebug);
#else
        container.Bind<bool>("game_debug").ToInstance(UnityEngine.Debug.isDebugBuild);
#endif
        container.Install(new GameModelInstaller());

        container.Bind<IGameErrorHandler>().ToMethod<GameErrorHandler>(CreateErrorHandler);
        container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        container.Bind<GameLoader>().ToMethod<GameLoader>(CreateGameLoader);
        container.Bind<IGameLoader>().ToLookup<GameLoader>();
        container.Listen<GameLoader>().Then(SetupGameLoader);

        #if ADMIN_PANEL
        container.BindAdminPanelConfigurer(CreateAdminPanel);
        #endif

        container.Bind<IPlayerData>().ToMethod<PlayerDataProvider>(CreatePlayerData);
        container.Bind<TutorialManager>().ToMethod(CreateTutorialManager);

        container.Install(new EconomyInstaller());
    }

    

    public void Initialize(IResolutionContainer container)
    {
        if(Settings.LoadLocalJson)
        {
            var loader = container.Resolve<IGameLoader>();
            if(loader != null)
            {
                loader.Load(null);
            }
        }
    }

    #if ADMIN_PANEL
    AdminPanelGame CreateAdminPanel(IResolutionContainer container)
    {
        return new AdminPanelGame(
            container.Resolve<IAppEvents>(),
            container.Resolve<IGameLoader>(),
            container.Resolve<GameModel>());
    }
    #endif

    GameLoader CreateGameLoader(IResolutionContainer container)
    {
        return new GameLoader(
            Settings.InitialJsonGameResource,
            Settings.InitialJsonPlayerResource,
            container.Resolve<IAttrObjParser<GameModel>>(),
            container.Resolve<IAttrObjParser<ConfigModel>>(),
            container.Resolve<IAttrObjParser<PlayerModel>>(),
            container.Resolve<IAttrObjParser<ConfigPatch>>(),
            container.Resolve<IAttrObjSerializer<PlayerModel>>(),
            container.Resolve<GameModel>(),
            container.Resolve<ILogin>());
    }

    void SetupGameLoader(IResolutionContainer container, GameLoader loader)
    {
        var commandQueue = container.Resolve<ICommandQueue>();
        if(commandQueue != null)
        {
            commandQueue.AutoSync = loader.OnAutoSync;
        }
    }

    GameErrorHandler CreateErrorHandler(IResolutionContainer container)
    {
        return new GameErrorHandler(
            container.Resolve<IAlertView>(),
            container.Resolve<Localization>(),
            container.Resolve<IAppEvents>(),
            container.Resolve<IRestarter>(),
            container.Resolve<IHelpshift>(),
            container.Resolve<bool>("game_debug"));

    }

    PlayerDataProvider CreatePlayerData(IResolutionContainer container)
    {
        return new PlayerDataProvider(
            container.Resolve<ILoginData>(),
            container.Resolve<PlayerModel>());
    }

    TutorialManager CreateTutorialManager(IResolutionContainer container)
    {
        return new TutorialManager(container.Resolve<IUpdateScheduler>());
    }

    /// <summary>
    /// Game must integrate data to implement an unique provider of player data
    /// </summary>
    class PlayerDataProvider : IPlayerData
    {
        ILoginData _loginData;
        readonly PlayerModel _playerModel;

        public PlayerDataProvider(ILoginData loginData, PlayerModel playerModel)
        {
            _loginData = loginData;
            _playerModel = playerModel;
        }

        #region IPlayerData implementation

        public string Id
        {
            get
            {
                return _loginData.UserId.ToString();
            }
        }

        public string Name
        {
            get
            {
                return "Player Name";
            }
        }

        public int Level
        {
            get
            {
                return _playerModel.Level;
            }
        }

        #endregion
    }
}
