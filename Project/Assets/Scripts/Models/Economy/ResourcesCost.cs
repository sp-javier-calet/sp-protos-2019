using System;
using SocialPoint.Base;
using SocialPoint.Locale;
using SocialPoint.ScriptEvents;

public class ResourcesCost : ICost
{
    const string _notEnoughResourcesMessage = "Not enough resources!";

    ResourcePool _cost;

    public ResourcesCost(ResourcePool cost)
    {
        _cost = cost;
    }

    bool HasEnoughResources(PlayerModel playerModel)
    {
        return playerModel.Resources.CanSubstract(_cost);
    }

    #region ICost implementation

    public void Spend(PlayerModel playerModel)
    {
        if(!HasEnoughResources(playerModel))
        {
            throw new Exception(_notEnoughResourcesMessage);
        }
        playerModel.Resources.Substract(_cost);
    }

    public void Validate(PlayerModel playerModel, Action<Error> finished)
    {
        Error error = null;
        if(!HasEnoughResources(playerModel))
        {
            error = new Error(_notEnoughResourcesMessage);
        }
        if(finished != null)
        {
            finished(error);
        }
    }

    #endregion
}
