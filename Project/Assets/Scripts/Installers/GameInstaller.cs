
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
        public string InitialJsonResource = "game";
        public bool EditorDebug = true;
    }

    public SettingsData Settings = new SettingsData();

    public override void InstallBindings()
    {
        Container.BindInstance("game_initial_json_resource", Settings.InitialJsonResource);
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
    }

    void OnGameModelAssigned()
    {
        var game = Container.Resolve<GameModel>();
        Container.Inject(game.Player);
        Container.Inject(game.Config);
    }

    GameModel CreateGameModel(InjectContext ctx)
    {
        var model = new GameModel();
        model.Assigned += OnGameModelAssigned;
        return model;
    }

}
