using System;
using SocialPoint.Base;

public class PurchaseCost : ICost
{
    public delegate void PurchaseDelegate(string productId, Action<Error> finished);

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
