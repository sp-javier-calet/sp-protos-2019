
using SocialPoint.Attributes;

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

    public ICost Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return new ResourcesCost(poolParser.Parse(data));
    }

    #endregion



}
