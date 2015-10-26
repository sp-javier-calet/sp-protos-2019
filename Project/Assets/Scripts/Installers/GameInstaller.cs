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

        Container.Bind<IAdminPanelConfigurer>().ToSingle<AdminPanelGame>();

        if(!Container.HasBinding<GameParser>())
        {
            Container.BindAllInterfacesToSingle<GameParser>();
        }
        if(!Container.HasBinding<GameModel>())
        {
            Container.Bind<GameModel>().ToSingleMethod<GameModel>(CreateGameModel);
        }
        if(!Container.HasBinding<PlayerModel>())
        {
            Container.Bind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        }
        if(!Container.HasBinding<ConfigModel>())
        {
            Container.Bind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);
        }         
        if(!Container.HasBinding<ISerializer<PlayerModel>>())
        {
            Container.Bind<ISerializer<PlayerModel>>()
                .ToSingleMethod<PlayerParser>(CreatePlayerParser);
        }
        if(!Container.HasBinding<GameLoader>())
        {
            Container.Bind<GameLoader>().ToSingle();
        }
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