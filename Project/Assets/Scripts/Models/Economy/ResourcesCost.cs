﻿using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public class NotEnoughResourcesError : ModelError
{
    public ResourcePool MissingResources { get; private set; }

    public NotEnoughResourcesError(ResourcePool missingResources)
    {
        MissingResources = missingResources;
    }

    public override string ToString()
    {
        return "Not enough resources. Missing resources: " + MissingResources;
    }
}

public class ResourcesCost : ICost
{
    ResourcePool _cost;

    public ResourcesCost(ResourcePool cost)
    {
        _cost = cost;
    }

    ModelError CheckEnoughResources(PlayerModel playerModel)
    {
        if(playerModel.Resources.CanSubstract(_cost))
        {
            return null;
        }
        return new NotEnoughResourcesError(playerModel.Resources.GetMissingResources(_cost));
    }

    #region ICost implementation

    public void Spend(PlayerModel playerModel)
    {
        ModelError error = CheckEnoughResources(playerModel);
        if(error != null)
        {
            throw new ModelException(error);
        }
        playerModel.Resources.Substract(_cost);
    }

    public void Validate(PlayerModel playerModel, Action<ModelError> finished)
    {
        if(finished != null)
        {
            finished(CheckEnoughResources(playerModel));
        }
    }

    #endregion
}

public static class ResourcePoolCostExtensions
{
    public static ResourcePool GetMissingResources(this ResourcePool playerResources, ResourcePool resources)
    {
        ResourcePool missingResources = new ResourcePool();
        var enumerator = resources.GetEnumerator();
        while(enumerator.MoveNext())
        {
            long playerAmount = playerResources[enumerator.Current.Key];
            if(playerAmount < enumerator.Current.Value)
            {
                missingResources[enumerator.Current.Key] = enumerator.Current.Value - playerAmount;
            }
        }
        enumerator.Dispose();
        return missingResources;
    }
}
