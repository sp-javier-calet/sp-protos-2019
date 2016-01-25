﻿using Zenject;
using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public struct NotEnoughResourcesEvent
{
    public ResourcePool Cost;
}

public class ResourcesCost : ICost
{
    [Inject]
    ResourcePool _playerResources;

    [Inject]
    IEventDispatcher _dispatcher;

    ResourcePool _cost;

    public ResourcesCost(ResourcePool cost)
    {
        _cost = cost;
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
