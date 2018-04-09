using System;
using System.Collections.Generic;
using SocialPoint.Attributes;
using SocialPoint.Base;
using SocialPoint.Dependency;
using SocialPoint.Purchase;

public class EconomyInstaller : SubInstaller, IInitializable
{
    IResolutionContainer _container;
    
    public override void InstallBindings(IBindingContainer container)
    {
        container.Bind<PurchaseCostFactory>().ToMethod<PurchaseCostFactory>(CreatePurchaseCostFactory);

        container.Bind<IChildParser<IReward>>().ToSingle<ResourcesRewardParser>();

        container.Bind<IChildParser<ICost>>().ToSingle<ResourcesCostParser>();
        container.Bind<IChildParser<ICost>>().ToMethod<PurchaseCostParser>(CreatePurchaseCostParser);

        container.Bind<IAttrObjParser<IReward>>().ToMethod<FamilyParser<IReward>>(CreateRewardParser);
        container.Bind<IAttrObjParser<ICost>>().ToMethod<FamilyParser<ICost>>(CreateCostParser);
    }
    
    public void Initialize(IResolutionContainer container)
    {
        _container = container;
    }

    PurchaseCostParser CreatePurchaseCostParser(IResolutionContainer container)
    {
        return new PurchaseCostParser(
            container.Resolve<PurchaseCostFactory>());
    }

    FamilyParser<IReward> CreateRewardParser(IResolutionContainer container)
    {
        var children = container.ResolveList<IChildParser<IReward>>();
        return new FamilyParser<IReward>(children);
    }

    FamilyParser<ICost> CreateCostParser(IResolutionContainer container)
    {
        var children = container.ResolveList<IChildParser<ICost>>();
        return new FamilyParser<ICost>(children);
    }

    PurchaseCostFactory CreatePurchaseCostFactory(IResolutionContainer container)
    {
        return new PurchaseCostFactory(Purchase);
    }

    void Purchase(string productId, Action<Error> finished)
    {
        Action<PurchaseResponseType> callback = null;
        if(finished != null)
        {
            callback = responseType => {
                Error error = null;
                if(responseType != PurchaseResponseType.Complete)
                {
                    error = new Error("Purchase error: " + responseType);
                }
                finished(error);
            };
        }
        _container.Resolve<IGamePurchaseStore>().Purchase(productId, callback);
    }
}
