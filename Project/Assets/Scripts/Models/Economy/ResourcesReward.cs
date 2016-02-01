using SocialPoint.Base;
using Zenject;

public class ResourcesReward : IReward
{
    [Inject]
    ResourcePool _playerResources;

    ResourcePool _resources;
    
    public ResourcesReward(ResourcePool resources)
    {
        _resources = resources;
    }

    #region IReward implementation

    public Error Obtain()
    {
        _playerResources.Add(_resources);
        return null;
    }

    #endregion

}
