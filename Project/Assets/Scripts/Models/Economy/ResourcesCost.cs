using System;
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
        return new NotEnoughResourcesError(ResourcePool.Missing(playerModel.Resources, _cost));
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