
using System;
using System.Collections.Generic;

using SocialPoint.Attributes;

public class StoreParser : IAttrObjParser<StoreModel>
{
    StoreModel _storeModel;
    PlayerModel _playerModel;
    IAttrObjParser<IDictionary<string, IReward>> _purchaseRewardsParser;

    public StoreParser(PlayerModel playerModel, StoreModel storeModel, IAttrObjParser<IDictionary<string, IReward>> purchaseRewardsParser)
    {
        _storeModel = storeModel;
        _playerModel = playerModel;
        _purchaseRewardsParser = purchaseRewardsParser;
    }

    #region IParser implementation

    public StoreModel Parse(Attr data)
    {
        var purchaseRewards = _purchaseRewardsParser.Parse(data);
        return _storeModel.Init(purchaseRewards, _playerModel);
    }

    #endregion
    
}

