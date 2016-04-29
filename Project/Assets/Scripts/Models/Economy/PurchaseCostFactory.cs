using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;

public class PurchaseCostFactory
{
    IGamePurchaseStore _store;

    public PurchaseCostFactory(IGamePurchaseStore store)
    {
        _store = store;
    }

    public PurchaseCost CreatePurchaseCost(string productId)
    {
        return new PurchaseCost(productId, _store);
    }
}
