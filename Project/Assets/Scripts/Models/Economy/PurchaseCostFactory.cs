using Zenject;
using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;

public class PurchaseCostFactory
{
    IGamePurchaseStore _purchaseStore;

    public PurchaseCostFactory(IGamePurchaseStore purchaseStore)
    {
        _purchaseStore = purchaseStore;
    }

    public PurchaseCost CreatePurchaseCost(string productId)
    {
        var cost = new PurchaseCost(productId);
        cost.Init(_purchaseStore);
        return cost;
    }
}
