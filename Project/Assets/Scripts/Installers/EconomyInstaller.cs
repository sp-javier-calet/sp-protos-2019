
using Zenject;
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

public class EconomyInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.BindIFactory<ResourcePool, ResourcesCost>().ToFactory();
        Container.BindIFactory<string, PurchaseCost>().ToMethod(CreatePurchaseCost);

        Container.BindIFactory<ResourcePool, ResourcesReward>().ToFactory();

        Container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        Container.Bind<IChildParser<ICost>>().ToSingle<ResourcesCostParser>();
        Container.Bind<IChildParser<ICost>>().ToSingle<PurchaseCostParser>();

        Container.Rebind<IParser<IReward>>().ToSingleMethod<FamilyParser<IReward>>(CreateRewardParser);
        Container.Rebind<IParser<ICost>>().ToSingleMethod<FamilyParser<ICost>>(CreateCostParser);
    }

    FamilyParser<IReward> CreateRewardParser(InjectContext ctx)
    {
        var children = Container.Resolve<List<IChildParser<IReward>>>();
        return new FamilyParser<IReward>(children);
    }

    FamilyParser<ICost> CreateCostParser(InjectContext ctx)
    {
        var children = Container.Resolve<List<IChildParser<ICost>>>();
        return new FamilyParser<ICost>(children);
    }

    PurchaseCost CreatePurchaseCost(DiContainer container, string productId)
    {
        var purchaseCost = new PurchaseCost(productId);
        container.Inject(purchaseCost);
        return purchaseCost;
    }
}
