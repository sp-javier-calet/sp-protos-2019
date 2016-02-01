
using SocialPoint.Attributes;

using Zenject;

public class ResourcesCostParser : IChildParser<ICost>
{

    #region IChildParser implementation

    const string NameValue = "resources";
    public string Name
    {
        get
        {
            return NameValue;
        }
    }

    public FamilyParser<ICost> Parent{ set{} }

    [Inject]
    IFactory<ResourcePool, ResourcesCost> _resourcesCostFactory;

    public ICost Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return _resourcesCostFactory.Create(poolParser.Parse(data));
    }

    #endregion



}
