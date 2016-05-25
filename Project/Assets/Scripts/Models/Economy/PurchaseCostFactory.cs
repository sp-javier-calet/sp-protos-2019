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
        return new PurchaseCost(productId, Purchase);
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
        _store.Purchase(productId, callback);
    }
}
