
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.GameLoading;
using SocialPoint.Alert;
using SocialPoint.Locale;
using SocialPoint.AppEvents;
using SocialPoint.ScriptEvents;
using SocialPoint.Login;

public class GameInstaller : Installer
{
    [Serializable]
    public class SettingsData
    {
        public string InitialJsonGameResource = "game";
        public string InitialJsonPlayerResource = "user";
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
#if UNITY_EDITOR
        Container.BindInstance("game_debug", Settings.EditorDebug);
#else
        Container.BindInstance("game_debug", UnityEngine.Debug.isDebugBuild);
#endif

        Container.Rebind<IGameErrorHandler>().ToMethod<GameErrorHandler>(CreateErrorHandler);
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Rebind<IParser<GameModel>>().ToMethod<GameParser>(CreateGameParser);
        Container.Rebind<IParser<ConfigModel>>().ToMethod<ConfigParser>(CreateConfigParser);
        Container.Rebind<PlayerParser>().ToMethod<PlayerParser>(CreatePlayerParser);
        Container.Rebind<IParser<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Rebind<ISerializer<PlayerModel>>().ToLookup<PlayerParser>();

        Container.Rebind<GameModel>().ToMethod<GameModel>(CreateGameModel);
        Container.Rebind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Rebind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        Container.Rebind<IGameLoader>().ToMethod<GameLoader>(CreateGameLoader);
        Container.Bind<IAdminPanelConfigurer>().ToMethod<AdminPanelGame>(CreateAdminPanel);

        Container.Rebind<IParser<StoreModel>>().ToMethod<StoreParser>(CreateStoreParser);
        Container.Rebind<IParser<IDictionary<string, IReward>>>().ToMethod<PurchaseRewardsParser>(CreatePurchaseRewardsParser);

        Container.Rebind<StoreModel>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Rebind<ResourcePool>().ToGetter<PlayerModel>((player) => player.Resources);

        Container.Install<EconomyInstaller>();
    }

    PurchaseRewardsParser CreatePurchaseRewardsParser()
    {
        return new PurchaseRewardsParser(
            Container.Resolve<IParser<IReward>>());
    }

    StoreParser CreateStoreParser()
    {
        return new StoreParser(
            Container.Resolve<IParser<IDictionary<string, IReward>>>());
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
            Container.Resolve<IParser<GameModel>>(),
            Container.Resolve<IParser<PlayerModel>>(),
            Container.Resolve<ISerializer<PlayerModel>>(),
            Container.Resolve<GameModel>(),
            Container.Resolve<ILogin>());
    }

    GameModel CreateGameModel()
    {
        return new GameModel();
    }

    PlayerParser CreatePlayerParser()
    {
        return new PlayerParser(
            Container.Resolve<ConfigModel>());
    }

    GameParser CreateGameParser()
    {
        return new GameParser(
            Container.Resolve<IParser<ConfigModel>>(),
            Container.Resolve<IParser<PlayerModel>>());
    }

    ConfigParser CreateConfigParser()
    {
        return new ConfigParser(
            Container.Resolve<IParser<StoreModel>>(),
            Container.Resolve<IParser<ScriptModel>>());
    }

    GameErrorHandler CreateErrorHandler()
    {
        return new GameErrorHandler(
            Container.Resolve<IAlertView>(),
            Container.Resolve<Localization>(),
            Container.Resolve<IAppEvents>(),
            Container.Resolve<bool>("game_debug"));

    }
}
