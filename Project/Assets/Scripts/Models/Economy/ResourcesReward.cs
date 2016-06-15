using SocialPoint.Base;

public class ResourcesReward : IReward
{
    ResourcePool _resources;

    public ResourcesReward(ResourcePool resources)
    {
        _resources = resources;
    }

    #region IReward implementation

    public ModelError Obtain(PlayerModel playerModel)
    {
        playerModel.Resources.Add(_resources);
        return null;
    }

    #endregion

}
