using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using System.Collections.Generic;

public class GameModelInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Rebind<IParser<GameModel>>().ToMethod<GameParser>(CreateGameParser);
        Container.Rebind<IParser<ConfigModel>>().ToMethod<ConfigParser>(CreateConfigParser);
        Container.Rebind<PlayerParser>().ToMethod<PlayerParser>(CreatePlayerParser);
        Container.Rebind<IParser<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Rebind<ISerializer<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Rebind<IParser<ConfigPatch>>().ToSingle<ConfigPatchParser>();

        Container.Rebind<GameModel>().ToMethod<GameModel>(CreateGameModel);
        Container.Rebind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Rebind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        Container.Rebind<IParser<StoreModel>>().ToMethod<StoreParser>(CreateStoreParser);

        Container.Rebind<IParser<IDictionary<string, IReward>>>().ToMethod<PurchaseRewardsParser>(CreatePurchaseRewardsParser);

        Container.Rebind<StoreModel>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Rebind<ResourcePool>().ToGetter<PlayerModel>((player) => player.Resources);
       
        Container.Bind<IChildParser<IModelCondition>>().ToSingle<AndConditionTypeModelParser>();
        Container.Bind<IChildParser<IModelCondition>>().ToSingle<OrConditionTypeModelParser>();
        Container.Rebind<IParser<IModelCondition>>().ToMethod<FamilyParser<IModelCondition>>(CreateModelConditionParser);
        Container.Rebind<IParser<GoalsTypeModel>>().ToMethod<GoalsTypeModelParser>(CreateGoalsParser);
    }

    GameParser CreateGameParser()
    {
        return new GameParser(
            Container.Resolve<GameModel>(),
            Container.Resolve<IParser<ConfigModel>>(),
            Container.Resolve<IParser<PlayerModel>>(),
            Container.Resolve<IParser<ConfigPatch>>());
    }

    ConfigParser CreateConfigParser()
    {
        return new ConfigParser(
            Container.Resolve<ConfigModel>(),
            Container.Resolve<IParser<StoreModel>>(),
            Container.Resolve<IParser<GoalsTypeModel>>(),
            Container.Resolve<IParser<ScriptModel>>());
    }

    PlayerParser CreatePlayerParser()
    {
        return new PlayerParser(
            Container.Resolve<PlayerModel>(),
            Container.Resolve<ConfigModel>(),
            Container.Resolve<IScriptEventDispatcher>());
    }

    GameModel CreateGameModel()
    {
        var gameModel = new GameModel();
        gameModel.Initialized += OnGameModelInitialized;
        return gameModel;
    }

    GoalsTypeModelParser CreateGoalsParser()
    {
        return new GoalsTypeModelParser(Container.Resolve<IParser<IModelCondition>>(), Container.Resolve<IParser<IReward>>());
    }

    FamilyParser<IModelCondition> CreateModelConditionParser()
    {
        var children = Container.ResolveList<IChildParser<IModelCondition>>();
        return new FamilyParser<IModelCondition>(children);
    }

    void OnGameModelInitialized(GameModel game)
    {
        
    }

    PurchaseRewardsParser CreatePurchaseRewardsParser()
    {
        return new PurchaseRewardsParser(
            Container.Resolve<IParser<IReward>>());
    }

    StoreParser CreateStoreParser()
    {
        return new StoreParser(
            Container.Resolve<PlayerModel>(),
            Container.Resolve<StoreModel>(),
            Container.Resolve<IParser<IDictionary<string, IReward>>>());
    }
}