using SocialPoint.Attributes;
using System.Collections.Generic;

public class ResourceParser : IParser<ResourceType>
{
    const string AttrNameKey = "name";
    #region IParser implementation
    public ResourceType Parse(Attr data)
    {
        var dataDic = data.AsDic;
        return new ResourceType("", dataDic.GetValue(AttrNameKey).ToString());
    }
    #endregion

}

public class ResourcesParser : IParser<List<ResourceType>>
{
    #region IParser implementation

    public List<ResourceType> Parse(Attr data)
    {
        var resources = new List<ResourceType>();
        var dataDic = data.AsDic;
        foreach(var kvp in dataDic)
        {
            var parser = new ResourceParser();
            var resource = parser.Parse(kvp.Value);
            resource.ID = kvp.Key;
            resources.Add(resource);
        }
        return resources;
    }
    #endregion

}


