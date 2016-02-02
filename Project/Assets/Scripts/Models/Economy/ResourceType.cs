public class ResourceType
{
    public string Id;
    public string Name;
    public string StoreId;

    //public string Description;
    //public string Icon;
    //public Color Color;

    public ResourceType(string id, string name, string storeId)
    {
        Id = id;
        Name = name;
        StoreId = storeId;
    }

    public override string ToString()
    {
        return string.Format("[ResourceType Id={0}, Name={1} StoreId={2}]", Id, Name, StoreId);
    }
}
