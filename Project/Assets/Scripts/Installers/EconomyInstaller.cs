
using System;
using System.Collections.Generic;
using SocialPoint.Dependency;
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;
using SocialPoint.Base;

public class EconomyInstaller : SubInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<PurchaseCostFactory>().ToMethod<PurchaseCostFactory>(CreatePurchaseCostFactory);

        Container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        Container.Bind<IChildParser<ICost>>().ToSingle<ResourcesCostParser>();
        Container.Bind<IChildParser<ICost>>().ToMethod<PurchaseCostParser>(CreatePurchaseCostParser);

        Container.Rebind<IParser<IReward>>().ToMethod<FamilyParser<IReward>>(CreateRewardParser);
        Container.Rebind<IParser<ICost>>().ToMethod<FamilyParser<ICost>>(CreateCostParser);
    }

    PurchaseCostParser CreatePurchaseCostParser()
    {
        return new PurchaseCostParser(
            Container.Resolve<PurchaseCostFactory>());
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

    PurchaseCostFactory CreatePurchaseCostFactory()
    {
        return new PurchaseCostFactory(Purchase);
    }

    void Purchase(string productId, Action<Error> finished)
    {
        Action<PurchaseResponseType> callback = null;
        if(finished != null)
        {
            callback = (PurchaseResponseType responseType) => {
                Error error = null;
                if(responseType != PurchaseResponseType.Complete)
                {
                    error = new Error("Purchase error: " + responseType);
                }
                finished(error);
            };
        }
        Container.Resolve<IGamePurchaseStore>().Purchase(productId, callback);
    }
}
