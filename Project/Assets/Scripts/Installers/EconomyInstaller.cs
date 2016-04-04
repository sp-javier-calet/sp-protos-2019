
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Attributes;

public class EconomyInstaller : Installer
{
    public override void InstallBindings()
    {
        Container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        Container.Bind<IChildParser<ICost>>().ToSingle<ResourcesCostParser>();
        Container.Bind<IChildParser<ICost>>().ToSingle<PurchaseCostParser>();

        Container.Rebind<IParser<IReward>>().ToSingleMethod<FamilyParser<IReward>>(CreateRewardParser);
        Container.Rebind<IParser<ICost>>().ToSingleMethod<FamilyParser<ICost>>(CreateCostParser);
    }

    FamilyParser<IReward> CreateRewardParser()
    {
        var children = Container.Resolve<List<IChildParser<IReward>>>();
        return new FamilyParser<IReward>(children);
    }

    FamilyParser<ICost> CreateCostParser()
    {
        var children = Container.Resolve<List<IChildParser<ICost>>>();
        return new FamilyParser<ICost>(children);
    }
}
