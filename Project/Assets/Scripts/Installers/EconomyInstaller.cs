
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;

public class EconomyInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ResourcesCostFactory>().ToMethod<ResourcesCostFactory>(CreateResourcesCostFactory);
        Container.Bind<PurchaseCostFactory>().ToMethod<PurchaseCostFactory>(CreatePurchaseCostFactory);
        Container.Bind<ResourcesRewardFactory>().ToMethod<ResourcesRewardFactory>(CreateResourcesRewardFactory);

        Container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        Container.Bind<IChildParser<ICost>>().ToMethod<ResourcesCostParser>(CreateResourcesCostParser);
        Container.Bind<IChildParser<ICost>>().ToMethod<PurchaseCostParser>(CreatePurchaseCostParser);

        Container.Rebind<IParser<IReward>>().ToMethod<FamilyParser<IReward>>(CreateRewardParser);
        Container.Rebind<IParser<ICost>>().ToMethod<FamilyParser<ICost>>(CreateCostParser);
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
