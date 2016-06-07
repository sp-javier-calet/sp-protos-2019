
using System;
using System.Collections.Generic;

using SocialPoint.Attributes;

public class StoreParser : IParser<StoreModel>
{
    StoreModel _storeModel;
    PlayerModel _playerModel;
    IParser<IDictionary<string, IReward>> _purchaseRewardsParser;

    public StoreParser(StoreModel storeModel, IParser<IDictionary<string, IReward>> purchaseRewardsParser)
    {
        _storeModel = storeModel;
        _purchaseRewardsParser = purchaseRewardsParser;
    }

    #region IParser implementation

    public StoreModel Parse(Attr data)
    {
        var purchaseRewards = _purchaseRewardsParser.Parse(data);
        return _storeModel.Init(purchaseRewards);
    }

    #endregion
    
}

