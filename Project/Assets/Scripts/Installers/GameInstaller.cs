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

    public override void InstallBindings()
    {
        Container.Bind<IInitializable>().ToInstance(this);

#if UNITY_EDITOR
        Container.Bind<bool>("game_debug").ToInstance(Settings.EditorDebug);
#else
        Container.Bind<bool>("game_debug").ToInstance(UnityEngine.Debug.isDebugBuild);
#endif
        Container.Install<GameModelInstaller>();

        Container.Bind<IGameErrorHandler>().ToMethod<GameErrorHandler>(CreateErrorHandler);
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Bind<GameLoader>().ToMethod<GameLoader>(CreateGameLoader);
        Container.Bind<IGameLoader>().ToLookup<GameLoader>();
        Container.Listen<GameLoader>().Then(SetupGameLoader);

        #if ADMIN_PANEL
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGame>(CreateAdminPanel);
        #endif

        Container.Bind<IPlayerData>().ToMethod<PlayerDataProvider>(CreatePlayerData);

        Container.Install<EconomyInstaller>();
    }

    public void Initialize()
    {
        if(Settings.LoadLocalJson)
        {
            var loader = Container.Resolve<IGameLoader>();
            if(loader != null)
            {
                loader.Load(null);
            }
        }
    }

    #if ADMIN_PANEL
    AdminPanelGame CreateAdminPanel()
    {
        return new AdminPanelGame(
            Container.Resolve<IAppEvents>(),
            Container.Resolve<IGameLoader>(),
            Container.Resolve<GameModel>());
    }
    #endif

    GameLoader CreateGameLoader()
    {
        return new GameLoader(
            Settings.InitialJsonGameResource,
            Settings.InitialJsonPlayerResource,
            Container.Resolve<IAttrObjParser<GameModel>>(),
            Container.Resolve<IAttrObjParser<ConfigModel>>(),
            Container.Resolve<IAttrObjParser<PlayerModel>>(),
            Container.Resolve<IAttrObjParser<ConfigPatch>>(),
            Container.Resolve<IAttrObjSerializer<PlayerModel>>(),
            Container.Resolve<GameModel>(),
            Container.Resolve<ILogin>());
    }

    void SetupGameLoader(GameLoader loader)
    {
        var commandQueue = Container.Resolve<ICommandQueue>();
        if(commandQueue != null)
        {
            commandQueue.AutoSync = loader.OnAutoSync;
        }
    }

    GameErrorHandler CreateErrorHandler()
    {
        return new GameErrorHandler(
            Container.Resolve<IAlertView>(),
            Container.Resolve<Localization>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<IRestarter>(),
            Container.Resolve<IHelpshift>(),
            Container.Resolve<bool>("game_debug"));

    }

    PlayerDataProvider CreatePlayerData()
    {
        return new PlayerDataProvider(
            Container.Resolve<ILoginData>(),
            Container.Resolve<PlayerModel>());
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
