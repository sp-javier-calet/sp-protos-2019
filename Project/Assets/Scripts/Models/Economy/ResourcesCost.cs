using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public class NotEnoughResourcesError : CostError
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

    CostError CheckEnoughResources(PlayerModel playerModel)
    {
        if(playerModel.Resources.CanSubstract(_cost))
        {
            return null;
        }
        return new NotEnoughResourcesError(playerModel.GetMissingResources(_cost));
    }

    #region ICost implementation

    public void Spend(PlayerModel playerModel)
    {
        CostError error = CheckEnoughResources(playerModel);
        if(error != null)
        {
            throw new CostException(error);
        }
        playerModel.Resources.Substract(_cost);
    }

    public void Validate(PlayerModel playerModel, Action<CostError> finished)
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
