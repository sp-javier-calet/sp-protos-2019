using SocialPoint.Attributes;

class ResourcePoolParser : IAttrObjParser<ResourcePool>, IAttrObjSerializer<ResourcePool>
{
    public ResourcePool Parse(Attr data)
    {
        var pool = new ResourcePool();
        foreach(var kvp in data.AsDic)
        {
            pool[kvp.Key] += kvp.Value.AsValue.ToLong();
        }
        return pool;
    }

    public Attr Serialize(ResourcePool pool)
    {
        var data = new AttrDic();
        foreach(var kvp in pool)
        {
            data.SetValue(kvp.Key, kvp.Value);
        }
        return data;
    }

}