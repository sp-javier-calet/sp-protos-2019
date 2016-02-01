
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

    public FamilyParser<IReward> Parent{ set{} }

    [Inject]
    IFactory<ResourcePool, ResourcesReward> resourceRewardFactory;

    public IReward Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return resourceRewardFactory.Create(poolParser.Parse(data));
    }

    #endregion



}
