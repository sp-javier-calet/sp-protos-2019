using System;
using SocialPoint.Base;

public delegate void PurchaseDelegate(string productId, Action<Error> finished);

public class PurchaseCost : ICost
{
    PurchaseDelegate _purchase;

    string _productId;

    public PurchaseCost(string productId, PurchaseDelegate purchase)
    {
        _productId = productId;
        _purchase = purchase;
    }

    #region ICost implementation

    public void Validate(PlayerModel playerModel, Action<Error> finished)
    {
        if(_purchase != null)
        {
            _purchase(_productId, finished);
        }
        else if(finished != null)
        {
            finished(null);
        }
    }

    public void Spend(PlayerModel playerModel)
    {
        // Purchase transaction is autovalidated by the IGamePurchaseStore
    }

    #endregion
}
