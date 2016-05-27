using System;
using SocialPoint.Base;

public delegate void PurchaseDelegate(string productId, Action<Error> finished);

public class PurchaseCostError : ModelError
{
    public Error Error { get; private set; }

    public PurchaseCostError(Error error)
    {
        Error = error;
    }
}

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

    public void Validate(PlayerModel playerModel, Action<ModelError> finished)
    {
        if(_purchase != null)
        {
            Action<Error> purchaseFinished = null;
            if(finished != null)
            {
                purchaseFinished = (Error error) => {
                    PurchaseCostError costError = null;
                    if(!Error.IsNullOrEmpty(error))
                    {
                        costError = new PurchaseCostError(error);
                    }
                    finished(costError);
                };
            }
            _purchase(_productId, purchaseFinished);
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
