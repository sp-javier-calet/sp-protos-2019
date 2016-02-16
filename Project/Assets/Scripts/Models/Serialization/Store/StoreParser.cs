
using System;
using System.Collections.Generic;

using SocialPoint.Attributes;

public class StoreParser : IParser<StoreModel>
{
    IParser<IDictionary<string, IReward>> _purchaseRewardsParser;

    public StoreParser(IParser<IDictionary<string, IReward>> purchaseRewardsParser)
    {
        _purchaseRewardsParser = purchaseRewardsParser;
    }

    #region IParser implementation

    public StoreModel Parse(Attr data)
    {
        var storeModel = new StoreModel();
        storeModel.PurchaseRewards = _purchaseRewardsParser.Parse(data);
        return storeModel;
    }

    #endregion
    
}

