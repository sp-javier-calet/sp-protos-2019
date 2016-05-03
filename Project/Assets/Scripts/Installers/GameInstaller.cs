
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

public class GameInstaller : MonoInstaller
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
        Container.BindInstance("game_initial_json_game_resource", Settings.InitialJsonGameResource);
        Container.BindInstance("game_initial_json_player_resource", Settings.InitialJsonPlayerResource);
#if UNITY_EDITOR
        Container.BindInstance("game_debug", Settings.EditorDebug);
#else
        Container.BindInstance("game_debug", UnityEngine.Debug.isDebugBuild);
#endif

        Container.Rebind<IGameErrorHandler>().ToSingleMethod<GameErrorHandler>(CreateErrorHandler);
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Rebind<IParser<GameModel>>().ToSingleMethod<GameParser>(CreateGameParser);
        Container.Rebind<IParser<ConfigModel>>().ToSingleMethod<ConfigParser>(CreateConfigParser);
        Container.Rebind<PlayerParser>().ToSingleMethod<PlayerParser>(CreatePlayerParser);
        Container.Rebind<IParser<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Rebind<ISerializer<PlayerModel>>().ToLookup<PlayerParser>();

        Container.Rebind<GameModel>().ToSingleMethod<GameModel>(CreateGameModel);
        Container.Rebind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Rebind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        Container.Rebind<IGameLoader>().ToSingleMethod<GameLoader>(CreateGameLoader);
        Container.Bind<IAdminPanelConfigurer>().ToSingleMethod<AdminPanelGame>(CreateAdminPanel);

        Container.Rebind<IParser<StoreModel>>().ToSingleMethod<StoreParser>(CreateStoreParser);
        Container.Rebind<IParser<IDictionary<string, IReward>>>().ToSingleMethod<PurchaseRewardsParser>(CreatePurchaseRewardsParser);

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
            Container.Resolve<string>("game_initial_json_game_resource"),
            Container.Resolve<string>("game_initial_json_player_resource"));
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
            Container.Resolve<IAppEvents>());

    }
}
