using System;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.GameLoading;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.ServerSync;
using SocialPoint.Social;
using SocialPoint.Login;

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

        Container.Rebind<IGameErrorHandler>().ToMethod<GameErrorHandler>(CreateErrorHandler);
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Rebind<IGameLoader>().ToMethod<GameLoader>(CreateGameLoader, SetupGameLoader);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGame>(CreateAdminPanel);

        Container.Rebind<IPlayerData>().ToMethod<PlayerDataProvider>(CreatePlayerData);

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

    AdminPanelGame CreateAdminPanel()
    {
        return new AdminPanelGame(
            Container.Resolve<IAppEvents>(),
            Container.Resolve<IGameLoader>(),
            Container.Resolve<GameModel>());
    }

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
        PlayerModel _playerModel;

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
