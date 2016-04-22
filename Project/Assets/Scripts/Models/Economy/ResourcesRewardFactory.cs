using Zenject;
using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public class ResourcesRewardFactory
{
    ResourcePool _playerResources;

    public ResourcesRewardFactory(ResourcePool playerResources)
    {
        _playerResources = playerResources;
    }

    public ResourcesReward CreateResourcesReward(ResourcePool resources)
    {
        var cost = new ResourcesReward(resources);
        cost.Init(_playerResources);
        return cost;
    }
}
