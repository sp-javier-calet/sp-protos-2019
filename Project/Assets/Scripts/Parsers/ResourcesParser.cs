using SocialPoint.Attributes;
using System.Collections.Generic;

public class ResourceParser : IParser<Resource>
{
    #region IParser implementation
    public Resource Parse(Attr data)
    {
        const string AttrNameKey = "name";
        var dataDic = data.AsDic;
        return new Resource("", dataDic.GetValue(AttrNameKey).ToString());
    }
    #endregion

}

public class ResourcesParser : IParser<List<Resource>>
{
    #region IParser implementation

    public List<Resource> Parse(Attr data)
    {
        var resources = new List<Resource>();
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


