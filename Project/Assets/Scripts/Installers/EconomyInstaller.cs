
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;

public class EconomyInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<ResourcesCostFactory>().ToSingleMethod<ResourcesCostFactory>(CreateResourcesCostFactory);
        Container.Bind<PurchaseCostFactory>().ToSingleMethod<PurchaseCostFactory>(CreatePurchaseCostFactory);
        Container.Bind<ResourcesRewardFactory>().ToSingleMethod<ResourcesRewardFactory>(CreateResourcesRewardFactory);

        Container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        Container.Bind<IChildParser<ICost>>().ToSingleMethod<ResourcesCostParser>(CreateResourcesCostParser);
        Container.Bind<IChildParser<ICost>>().ToSingleMethod<PurchaseCostParser>(CreatePurchaseCostParser);

        Container.Rebind<IParser<IReward>>().ToSingleMethod<FamilyParser<IReward>>(CreateRewardParser);
        Container.Rebind<IParser<ICost>>().ToSingleMethod<FamilyParser<ICost>>(CreateCostParser);
    }

    ResourcesCostParser CreateResourcesCostParser()
    {
        return new ResourcesCostParser(
            Container.Resolve<ResourcePool>(),
            Container.Resolve<IEventDispatcher>());
    }

    PurchaseCostParser CreatePurchaseCostParser()
    {
        return new PurchaseCostParser(
            Container.Resolve<IGamePurchaseStore>());
    }

    FamilyParser<IReward> CreateRewardParser()
    {
        var children = Container.ResolveList<IChildParser<IReward>>();
        return new FamilyParser<IReward>(children);
    }

    FamilyParser<ICost> CreateCostParser()
    {
        var children = Container.ResolveList<IChildParser<ICost>>();
        return new FamilyParser<ICost>(children);
    }

    ResourcesCostFactory CreateResourcesCostFactory()
    {
        var playerResources = Container.Resolve<ResourcePool>();
        var eventDispatcher = Container.Resolve<IEventDispatcher>();
        return new ResourcesCostFactory(playerResources, eventDispatcher);
    }

    PurchaseCostFactory CreatePurchaseCostFactory()
    {
        var store = Container.Resolve<IGamePurchaseStore>();
        return new PurchaseCostFactory(store);
    }

    ResourcesRewardFactory CreateResourcesRewardFactory()
    {
        var playerResources = Container.Resolve<ResourcePool>();
        return new ResourcesRewardFactory(playerResources);
    }
}
