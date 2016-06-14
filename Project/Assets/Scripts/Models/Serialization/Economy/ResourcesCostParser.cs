
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

    ResourcePoolParser _resourcePoolParser;

    public ResourcesCostParser()
    {
        _resourcePoolParser = new ResourcePoolParser();
    }


    public ICost Parse(Attr data)
    {
        return new ResourcesCost(_resourcePoolParser.Parse(data));
    }

    #endregion



}
