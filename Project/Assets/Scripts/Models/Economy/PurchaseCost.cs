using System;
using SocialPoint.Base;
using SocialPoint.Purchase;
using Zenject;

public class PurchaseCost : ICost
{

    [Inject]
    IGamePurchaseStore _purchaseStore;

    string _productId;
    Action<Error> _finished;

    public PurchaseCost(string productId)
    {
        _productId = productId;
    }

    [PostInject]
    void PostInject()
    {
        _purchaseStore.PurchaseUpdated += OnPurchaseUpdated;
    }

    void OnPurchaseUpdated(PurchaseState state, string productId)
    {
        if(state == PurchaseState.ValidateSuccess)
        {
            if(_finished != null)
            {
                _finished(null);
            }
        }
    }

    #region ICost implementation

    public void Spend(Action<Error> finished)
    {
        _finished += finished;
        _purchaseStore.Purchase(_productId);
    }

    #endregion
}