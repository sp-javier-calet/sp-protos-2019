using UnityEngine;
using Zenject;
using System;
using SocialPoint.AdminPanel;
using SocialPoint.Attributes;

public class GameInstaller : MonoInstaller
{  
    [Serializable]
    public class SettingsData
    {
        public string InitialJsonResource = "game";
    }

    public SettingsData Settings;

    public override void InstallBindings()
    {
        Container.BindInstance("game_initial_json_resource", Settings.InitialJsonResource);

        Container.Rebind<IParser<GameModel>>().ToSingle<GameParser>();
        Container.Rebind<GameModel>().ToSingleMethod<GameModel>(CreateGameModel);
        Container.Rebind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Rebind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);
        Container.Rebind<ISerializer<PlayerModel>>().ToSingleMethod<PlayerParser>(CreatePlayerParser);
        Container.Rebind<GameLoader>().ToSingle();
    }
    
    void OnGameModelAssigned()
    {
    }
    
    PlayerParser CreatePlayerParser(InjectContext ctx)
    {
        var model = Container.Resolve<GameModel>();
        return new PlayerParser(model.Config);
    }
    
    GameModel CreateGameModel(InjectContext ctx)
    {
        var model = new GameModel();
        model.Assigned += OnGameModelAssigned;
        return model;
    }

}