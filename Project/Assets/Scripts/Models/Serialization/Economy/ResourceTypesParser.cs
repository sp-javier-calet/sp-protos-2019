using SocialPoint.Attributes;
using System.Collections.Generic;

public class ResourceTypeParser : IParser<ResourceType>, ISerializer<ResourceType>
{
    const string AttrKeyId = "id";
    const string AttrKeyName = "name";
    const string AttrKeyStoreItemId = "store_id";

    #region IParser implementation

    public ResourceType Parse(Attr data)
    {
        var dataDic = data.AsDic;
        return new ResourceType(
            dataDic.GetValue(AttrKeyId).ToString(),
            dataDic.GetValue(AttrKeyName).ToString(),
            dataDic.GetValue(AttrKeyStoreItemId).ToString());
    }

    public Attr Serialize(ResourceType resource)
    {
        var data = new AttrDic();
        data.SetValue(AttrKeyId, resource.Id);
        data.SetValue(AttrKeyName, resource.Name);
        return data;
    }

    #endregion
}

public class ResourceTypesParser : IParser<IDictionary<string, ResourceType>>, ISerializer<IDictionary<string, ResourceType>>
{
    #region IParser implementation

    public IDictionary<string, ResourceType> Parse(Attr data)
    {
        var resources = new Dictionary<string, ResourceType>();
        var dataList = data.AsList;
        var parser = new ResourceTypeParser();
        foreach(var elm in dataList)
        {
            var resource = parser.Parse(elm);
            resources.Add(resource.Id, resource);
        }
        return resources;
    }

    public Attr Serialize(IDictionary<string, ResourceType> resources)
    {
        var data = new AttrDic();
        var resParser = new ResourceTypeParser();
        foreach(var kvp in resources)
        {
            data.Set(kvp.Key, resParser.Serialize(kvp.Value));
        }
        return data;
    }

    #endregion

}