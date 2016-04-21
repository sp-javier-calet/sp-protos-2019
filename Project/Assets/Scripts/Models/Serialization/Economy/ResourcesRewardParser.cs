
using SocialPoint.Attributes;
using Zenject;

public class ResourcesRewardParser : IChildParser<IReward>
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

    public FamilyParser<IReward> Parent{ set { } }

    [Inject]
    ResourcesRewardFactory resourceRewardFactory;

    public IReward Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return resourceRewardFactory.CreateResourcesReward(poolParser.Parse(data));
    }

    #endregion



}
