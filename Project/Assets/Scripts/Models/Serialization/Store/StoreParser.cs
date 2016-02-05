
using System;
using System.Collections.Generic;

using SocialPoint.Attributes;
using SocialPoint.Purchase;

public class StoreParser : IParser<StoreModel>
{
    IParser<IDictionary<string, IReward>> _purchaseRewardsParser;
    IGamePurchaseStore _store;

    public StoreParser(IParser<IDictionary<string, IReward>> purchaseRewardsParser, IGamePurchaseStore store)
    {
        _purchaseRewardsParser = purchaseRewardsParser;
        _store = store;
    }

    #region IParser implementation

    public StoreModel Parse(Attr data)
    {
        var storeModel = new StoreModel(_store);
        storeModel.PurchaseRewards = _purchaseRewardsParser.Parse(data);
        return storeModel;
    }

    #endregion
    
}

