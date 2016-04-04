
using SocialPoint.Attributes;


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

    public IReward Parse(Attr data)
    {
        var poolParser = new ResourcePoolParser();
        return new ResourcesReward(poolParser.Parse(data));
    }

    #endregion



}
