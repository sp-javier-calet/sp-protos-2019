
using System;
using System.Collections.Generic;
using SocialPoint.Attributes;

public class StoreCategoryParser : IParser<StoreCategory>
{
    const string AttrKeyId = "id";
    const string AttrKeyName = "name";
    const string AttrKeyItems = "items";

    IDictionary<string, StoreItem> _items;

    public StoreCategoryParser(IDictionary<string, StoreItem> items)
    {
        _items = items;   
    }

    #region IParser implementation

    public StoreCategory Parse(Attr data)
    {
        var dataDic = data.AsDic;
        var itemsIds = dataDic[AttrKeyItems].AsList;
        var items = new List<StoreItem>();
        foreach(var item in itemsIds)
        {
            items.Add(_items[item.ToString()]);
        }
        var storeCategory = new StoreCategory(dataDic[AttrKeyId].ToString(), dataDic[AttrKeyName].ToString(), items);

        return storeCategory;
    }

    #endregion
    
}

public class StoreCategoriesParser : IParser<IDictionary<string, StoreCategory>>
{
    const string AttrKeyCategories = "categories";

    IDictionary<string, StoreItem> _items;

    public StoreCategoriesParser(IDictionary<string, StoreItem>items)
    {
        _items = items;
    }

    #region IParser implementation

    public IDictionary<string, StoreCategory> Parse(Attr data)
    {
        var categories = new Dictionary<string, StoreCategory>();
        var attrCategories = data.AsDic[AttrKeyCategories].AsDic;
        var categoryParser = new StoreCategoryParser(_items);
        foreach(var kvp in attrCategories)
        {
            var category = categoryParser.Parse(kvp.Value);
            categories.Add(category.Id, category);
        }
        return categories;
    }

    #endregion
    
}
