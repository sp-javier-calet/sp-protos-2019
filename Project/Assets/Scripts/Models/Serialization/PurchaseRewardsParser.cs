using System;
using SocialPoint.Attributes;
using System.Collections.Generic;

public class PurchaseRewardsParser : IParser<IDictionary<string, IReward>>
{
    const string AttrKeyPurchases = "purchases";
    const string AttrKeyProductId = "productId";
    const string AttrKeyReward = "reward";

    IParser<IReward> _rewardParser;

    public PurchaseRewardsParser(IParser<IReward> rewardParser)
    {
        _rewardParser = rewardParser;
    }

    #region IParser implementation

    public IDictionary<string, IReward> Parse(Attr data)
    {
        var purchaseRewards = new Dictionary<string, IReward>();
        var attrPurchaseRewards = data.AsDic[AttrKeyPurchases].AsList;
        foreach(var kvp in attrPurchaseRewards)
        {
            purchaseRewards.Add(kvp.AsDic[AttrKeyProductId].ToString(), _rewardParser.Parse(kvp.AsDic[AttrKeyReward]));
        }
        return purchaseRewards;
    }

    #endregion
    
}