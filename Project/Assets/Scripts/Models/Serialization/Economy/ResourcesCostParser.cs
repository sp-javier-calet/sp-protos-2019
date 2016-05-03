
using SocialPoint.Attributes;
using SocialPoint.ScriptEvents;

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

    public FamilyParser<ICost> Parent{ set { } }

    ResourcePool _playerResources;
    IEventDispatcher _dispatcher;

    public ResourcesCostParser(ResourcePool playerResources, IEventDispatcher dispatcher)
    {
        _playerResources = playerResources;
        _dispatcher = dispatcher;
    }


    public ICost Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return new ResourcesCost(poolParser.Parse(data), _playerResources, _dispatcher);
    }

    #endregion



}
