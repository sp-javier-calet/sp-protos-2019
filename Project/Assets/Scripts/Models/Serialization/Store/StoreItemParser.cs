using System.Collections.Generic;
using SocialPoint.Attributes;

public class StoreItemParser : IParser<StoreItem>
{

    const string AttrKeyId = "id";
    const string AttrKeyName = "name";
    const string AttrKeyDescription = "description";
    const string AttrKeyReward = "reward";
    const string AttrKeyCost = "cost";
    const string AttrkeyHidden = "hidden";

    IParser<IReward> _rewardParser;
    IParser<ICost> _costParser;

    public StoreItemParser(IParser<IReward> rewardParser, IParser<ICost> costParser)
    {
        _rewardParser = rewardParser;
        _costParser = costParser;
    }

    #region IParser implementation

    public StoreItem Parse(Attr data)
    {
        var dataDic = data.AsDic;
        var id = dataDic[AttrKeyId].ToString();
        var name = dataDic[AttrKeyName].ToString();
        var desc = dataDic[AttrKeyDescription].ToString();
        var isHidden = dataDic[AttrkeyHidden].AsValue.ToBool();
        var cost = _costParser.Parse(dataDic[AttrKeyCost]);
        var reward = _rewardParser.Parse(dataDic[AttrKeyReward]);
        var storeItem = new StoreItem(id, name, desc, isHidden, cost, reward);
        return storeItem;
    }

    #endregion

}

public class StoreItemsParser : IParser<IDictionary<string, StoreItem>>
{
    const string AttrKeyItems = "items";

    IParser<StoreItem> _storeItemParser;

    public StoreItemsParser(IParser<StoreItem> storeItemParser)
    {
        _storeItemParser = storeItemParser;
    }

    #region IParser implementation

    public IDictionary<string, StoreItem> Parse(Attr data)
    {
        var storeItems = new Dictionary<string, StoreItem>();
        var attrStoreItems = data.AsDic[AttrKeyItems].AsDic;
        foreach(var kvp in attrStoreItems)
        {
            var item = _storeItemParser.Parse(kvp.Value);
            storeItems.Add(item.Id, item);
        }
        return storeItems;
    }

    #endregion
    
}


