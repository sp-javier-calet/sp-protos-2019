
using Zenject;
using System;
using System.Collections.Generic;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;
using SocialPoint.GameLoading;

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

        Container.Rebind<IGameErrorHandler>().ToSingle<GameErrorHandler>();
        Container.Bind<IDisposable>().ToLookup<IGameErrorHandler>();

        Container.Rebind<IParser<GameModel>>().ToSingle<GameParser>();
        Container.Rebind<IParser<ConfigModel>>().ToSingle<ConfigParser>();
        Container.Rebind<IParser<PlayerModel>>().ToSingle<PlayerParser>();
        Container.Rebind<ISerializer<PlayerModel>>().ToSingle<PlayerParser>();

        Container.Rebind<GameModel>().ToSingleMethod<GameModel>(CreateGameModel);
        Container.Rebind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Rebind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        Container.Rebind<IGameLoader>().ToSingle<GameLoader>();
        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();

        Container.Rebind<IParser<StoreModel>>().ToSingle<StoreParser>();
        Container.Rebind<IParser<IDictionary<string, IReward>>>().ToSingle<PurchaseRewardsParser>();

        Container.Rebind<StoreModel>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Rebind<ResourcePool>().ToGetter<PlayerModel>((player) => player.Resources);

        Container.Install<EconomyInstaller>();
    }

    void OnGameModelMoved(GameModel game)
    {
        Container.Inject(game.Player);
        Container.Inject(game.Config);
        Container.Inject(game.Config.Store);
    }

    GameModel CreateGameModel(InjectContext ctx)
    {
        var model = new GameModel();
        model.Moved += OnGameModelMoved;
        return model;
    }

}
