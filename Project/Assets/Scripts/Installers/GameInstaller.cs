using UnityEngine;
using Zenject;
using SocialPoint.AdminPanel;

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
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
                
        Container.Bind<ISerializer<PlayerModel>>()
            .ToMethod(CreatePlayerParser);

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