
using SocialPoint.Attributes;


public class ResourcesRewardParser : IChildParser<IReward>
{
    ResourcePoolParser _resourcePoolParser;

    public ResourcesRewardParser()
    {
        _resourcePoolParser = new ResourcePoolParser();
    }

    #region IChildParser implementation

    const string NameValue = "resources";

    public string Name
    {
        get
        {
            return NameValue;
        }
    }

    public FamilyParser<IReward> Parent{ set { } }

    public IReward Parse(Attr data)
    {
        return new ResourcesReward(_resourcePoolParser.Parse(data));
    }

    #endregion



}
