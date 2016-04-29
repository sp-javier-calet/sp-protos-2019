﻿using SocialPoint.Base;
using SocialPoint.Dependency;

public class ResourcesReward : IReward
{
    ResourcePool _playerResources;
    ResourcePool _resources;

    public ResourcesReward(ResourcePool resources)
    {
        _playerResources = ServiceLocator.Instance.Resolve<ResourcePool>();
        _resources = resources;
    }

    public void Init(ResourcePool playerResources)
    {
        _playerResources = playerResources;
    }

    #region IReward implementation

    public Error Obtain()
    {
        _playerResources.Add(_resources);
        return null;
    }

    #endregion

}
