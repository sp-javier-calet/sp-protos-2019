
using System;
using System.Collections.Generic;

using SocialPoint.Attributes;

public class StoreParser : IParser<StoreModel>
{
    IParser<StoreItem> _storeItemParser;
    IParser<IDictionary<string, IReward>> _purchaseRewardsParser;

    public StoreParser(IParser<StoreItem> storeItemParser, IParser<IDictionary<string, IReward>> purchaseRewardsParser)
    {
        _storeItemParser = storeItemParser;
        _purchaseRewardsParser = purchaseRewardsParser;
    }

    #region IParser implementation

    public StoreModel Parse(Attr data)
    {
        var storeModel = new StoreModel();
        var storeItemsParser = new StoreItemsParser(_storeItemParser);
        var items = storeItemsParser.Parse(data);
        var storeCategoriesParser = new StoreCategoriesParser(items);
        storeModel.Categories = storeCategoriesParser.Parse(data);
        storeModel.PurchaseRewards = _purchaseRewardsParser.Parse(data);
        return storeModel;
    }

    #endregion
    
}

