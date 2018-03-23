using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;
using System.Collections.Generic;

public class GameModelInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<IAttrObjParser<GameModel>>().ToMethod<GameParser>(CreateGameParser);
        Container.Bind<IAttrObjParser<ConfigModel>>().ToMethod<ConfigParser>(CreateConfigParser);
        Container.Bind<PlayerParser>().ToMethod<PlayerParser>(CreatePlayerParser);
        Container.Bind<IAttrObjParser<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Bind<IAttrObjSerializer<PlayerModel>>().ToLookup<PlayerParser>();
        Container.Bind<IAttrObjParser<ConfigPatch>>().ToSingle<ConfigPatchParser>();

        Container.Bind<GameModel>().ToMethod<GameModel>(CreateGameModel);
        Container.Bind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        Container.Bind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        Container.Bind<IAttrObjParser<StoreModel>>().ToMethod<StoreParser>(CreateStoreParser);

        Container.Bind<IAttrObjParser<IDictionary<string, IReward>>>().ToMethod<PurchaseRewardsParser>(CreatePurchaseRewardsParser);

        Container.Bind<StoreModel>().ToGetter<ConfigModel>((Config) => Config.Store);
        Container.Bind<ResourcePool>().ToGetter<PlayerModel>((player) => player.Resources);

        Container.Bind<IChildParser<IModelCondition>>().ToSingle<AndConditionTypeModelParser>();
        Container.Bind<IChildParser<IModelCondition>>().ToSingle<OrConditionTypeModelParser>();
        Container.Bind<IAttrObjParser<IModelCondition>>().ToMethod<FamilyParser<IModelCondition>>(CreateModelConditionParser);
        Container.Bind<IAttrObjParser<GoalsTypeModel>>().ToMethod<GoalsTypeModelParser>(CreateGoalsParser);
    }

    GameParser CreateGameParser()
    {
        return new GameParser(
            Container.Resolve<GameModel>(),
            Container.Resolve<IAttrObjParser<ConfigModel>>(),
            Container.Resolve<IAttrObjParser<PlayerModel>>(),
            Container.Resolve<IAttrObjParser<ConfigPatch>>());
    }

    ConfigParser CreateConfigParser()
    {
        return new ConfigParser(
            Container.Resolve<ConfigModel>(),
            Container.Resolve<IAttrObjParser<StoreModel>>(),
            Container.Resolve<IAttrObjParser<GoalsTypeModel>>(),
            Container.Resolve<IAttrObjParser<ScriptModel>>());
    }

    PlayerParser CreatePlayerParser()
    {
        return new PlayerParser(
            Container.Resolve<PlayerModel>(),
            Container.Resolve<ConfigModel>(),
            Container.Resolve<IScriptEventProcessor>());
    }

    GameModel CreateGameModel()
    {
        var gameModel = new GameModel();
        gameModel.Initialized += OnGameModelInitialized;
        return gameModel;
    }

    GoalsTypeModelParser CreateGoalsParser()
    {
        return new GoalsTypeModelParser(Container.Resolve<IAttrObjParser<IModelCondition>>(), Container.Resolve<IAttrObjParser<IReward>>());
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
            Container.Resolve<IAttrObjParser<IReward>>());
    }

    StoreParser CreateStoreParser()
    {
        var storeModel = Container.Resolve<StoreModel>();
        storeModel.PurchaseStore = Container.Resolve<IGamePurchaseStore>();
        return new StoreParser(
            Container.Resolve<PlayerModel>(),
            storeModel,
            Container.Resolve<IAttrObjParser<IDictionary<string, IReward>>>());
    }
}