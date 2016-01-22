using System.Linq;
using System.Collections.Generic;
using SocialPoint.Utils;

public class StoreCategory
{
    public string Id{ get; private set; }

    public virtual string Name { get; private set; }

    public List<StoreItem> Items { get; private set; }

    public StoreCategory(string id, string name, List<StoreItem> storeItems)
    {
        Id = id;
        Name = name;
        Items = storeItems;
    }

    public bool ContainsItem(string itemId)
    {
        return Items.FirstOrDefault((item) => item.Id == itemId) != null;
    }

    public override string ToString()
    {
        return string.Format("[StoreCategory: Id={0}, Name={1}, Items={2}]", Id, Name, StringUtils.Join(Items));
    }
}


