using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;
using System.Collections.Generic;

public class GameModelInstaller : SubInstaller
{
    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<IAttrObjParser<GameModel>>().ToMethod<GameParser>(CreateGameParser);
        container.Bind<IAttrObjParser<ConfigModel>>().ToMethod<ConfigParser>(CreateConfigParser);
        container.Bind<PlayerParser>().ToMethod<PlayerParser>(CreatePlayerParser);
        container.Bind<IAttrObjParser<PlayerModel>>().ToLookup<PlayerParser>();
        container.Bind<IAttrObjSerializer<PlayerModel>>().ToLookup<PlayerParser>();
        container.Bind<IAttrObjParser<ConfigPatch>>().ToSingle<ConfigPatchParser>();

        container.Bind<GameModel>().ToMethod<GameModel>(CreateGameModel);
        container.Bind<PlayerModel>().ToGetter<GameModel>((game) => game.Player);
        container.Bind<ConfigModel>().ToGetter<GameModel>((game) => game.Config);

        container.Bind<IAttrObjParser<StoreModel>>().ToMethod<StoreParser>(CreateStoreParser);

        container.Bind<IAttrObjParser<IDictionary<string, IReward>>>().ToMethod<PurchaseRewardsParser>(CreatePurchaseRewardsParser);

        container.Bind<StoreModel>().ToGetter<ConfigModel>((Config) => Config.Store);
        container.Bind<ResourcePool>().ToGetter<PlayerModel>((player) => player.Resources);

        container.Bind<IChildParser<IModelCondition>>().ToSingle<AndConditionTypeModelParser>();
        container.Bind<IChildParser<IModelCondition>>().ToSingle<OrConditionTypeModelParser>();
        container.Bind<IAttrObjParser<IModelCondition>>().ToMethod<FamilyParser<IModelCondition>>(CreateModelConditionParser);
        container.Bind<IAttrObjParser<GoalsTypeModel>>().ToMethod<GoalsTypeModelParser>(CreateGoalsParser);
    }

    GameParser CreateGameParser(IResolutionContainer container)
    {
        return new GameParser(
            container.Resolve<GameModel>(),
            container.Resolve<IAttrObjParser<ConfigModel>>(),
            container.Resolve<IAttrObjParser<PlayerModel>>(),
            container.Resolve<IAttrObjParser<ConfigPatch>>());
    }

    ConfigParser CreateConfigParser(IResolutionContainer container)
    {
        return new ConfigParser(
            container.Resolve<ConfigModel>(),
            container.Resolve<IAttrObjParser<StoreModel>>(),
            container.Resolve<IAttrObjParser<GoalsTypeModel>>(),
            container.Resolve<IAttrObjParser<ScriptModel>>());
    }

    PlayerParser CreatePlayerParser(IResolutionContainer container)
    {
        return new PlayerParser(
            container.Resolve<PlayerModel>(),
            container.Resolve<ConfigModel>(),
            container.Resolve<IScriptEventProcessor>());
    }

    GameModel CreateGameModel(IResolutionContainer container)
    {
        var gameModel = new GameModel();
        gameModel.Initialized += OnGameModelInitialized;
        return gameModel;
    }

    GoalsTypeModelParser CreateGoalsParser(IResolutionContainer container)
    {
        return new GoalsTypeModelParser(container.Resolve<IAttrObjParser<IModelCondition>>(), container.Resolve<IAttrObjParser<IReward>>());
    }

    FamilyParser<IModelCondition> CreateModelConditionParser(IResolutionContainer container)
    {
        var children = container.ResolveList<IChildParser<IModelCondition>>();
        return new FamilyParser<IModelCondition>(children);
    }

    void OnGameModelInitialized(GameModel game)
    {

    }

    PurchaseRewardsParser CreatePurchaseRewardsParser(IResolutionContainer container)
    {
        return new PurchaseRewardsParser(
            container.Resolve<IAttrObjParser<IReward>>());
    }

    StoreParser CreateStoreParser(IResolutionContainer container)
    {
        var storeModel = container.Resolve<StoreModel>();
        storeModel.PurchaseStore = container.Resolve<IGamePurchaseStore>();
        return new StoreParser(
            container.Resolve<PlayerModel>(),
            storeModel,
            container.Resolve<IAttrObjParser<IDictionary<string, IReward>>>());
    }
}