using Zenject;
using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public class ResourcesCostFactory
{
    ResourcePool _playerResources;
    IEventDispatcher _dispatcher;

    public ResourcesCostFactory(ResourcePool playerResources, IEventDispatcher dispatcher)
    {
        _playerResources = playerResources;
        _dispatcher = dispatcher;
    }

    public ResourcesCost CreateResourcesCost(ResourcePool resources)
    {
        var cost = new ResourcesCost(resources);
        cost.Init(_playerResources, _dispatcher);
        return cost;
    }
}
