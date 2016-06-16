using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;
using SocialPoint.Purchase;

public class PurchaseCostFactory
{
    PurchaseDelegate _purchaseDelegate;

    public PurchaseCostFactory(PurchaseDelegate purchaseDelegate)
    {
        _purchaseDelegate = purchaseDelegate;
    }

    public PurchaseCost CreatePurchaseCost(string productId)
    {
        return new PurchaseCost(productId, _purchaseDelegate);
    }
}
