
using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;
using SocialPoint.Dependency;

public struct NotEnoughResourcesEvent
{
    public ResourcePool Cost;
}

public class ResourcesCost : ICost
{
    ResourcePool _playerResources;
    IEventDispatcher _dispatcher;
    ResourcePool _cost;

    public ResourcesCost(ResourcePool cost)
    {
        _cost = cost;
        _playerResources = ServiceLocator.Instance.Resolve<ResourcePool>();
        _dispatcher = ServiceLocator.Instance.Resolve<IEventDispatcher>();
    }

    #region ICost implementation

    public void Spend(Action<Error> finished)
    {
        if(!_playerResources.CanSubstract(_cost))
        {
            _dispatcher.Raise(new NotEnoughResourcesEvent{ Cost = _cost });

            if(finished != null)
            {
                finished(new Error("Not enough resources!"));
            }
            return;
        }
        _playerResources.Substract(_cost);
        if(finished != null)
        {
            finished(null);
        }
    }

    #endregion

}
